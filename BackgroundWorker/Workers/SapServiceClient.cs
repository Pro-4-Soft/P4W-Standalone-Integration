using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Pro4Soft.BackgroundWorker.Workers;

public class SapServiceClient : IDisposable
{
    private readonly HttpClient _httpClient;
    
    private readonly string _serviceLayerUrl;
    private readonly string _companyDb;
    private readonly string _userName;
    private readonly string _password;
    private string _sessionId;
    private readonly CookieContainer _cookieContainer;
    private DateTime _lastLoginTime = DateTime.MinValue;
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(25);

    private readonly Func<string, Task> _log = _ => Task.CompletedTask;
    private readonly Func<string, Task> _logError = _ => Task.CompletedTask;

    private static readonly ConcurrentDictionary<string, SapServiceClient> Instances = new();
    public static SapServiceClient GetInstance(string serviceLayerUrl, string companyDb, string userName, string password, Func<string, Task> log = null, Func<string, Task> logError = null)
    {
        var key = $"{serviceLayerUrl}{companyDb}{userName}{password}";
        if (Instances.TryGetValue(key, out var instance))
            return instance;
        Instances[key] = new(serviceLayerUrl, companyDb, userName, password, log, logError);
        return Instances[key];
    }

    private SapServiceClient(string serviceLayerUrl, string companyDb, string userName, string password, Func<string, Task> log = null, Func<string, Task> logError = null)
    {
        _log = log ?? _log;
        _logError = logError ?? _logError;

        _serviceLayerUrl = serviceLayerUrl.TrimEnd('/');
        _companyDb = companyDb;
        _userName = userName;
        _password = password;

        _cookieContainer = new();
        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri($"{_serviceLayerUrl.Trim('/')}/"),
            Timeout = TimeSpan.FromMinutes(5)
        };

        //_retryPolicy = Policy
        //    .HandleResult<HttpResponseMessage>(r =>
        //        r.StatusCode == HttpStatusCode.Unauthorized ||
        //        r.StatusCode == HttpStatusCode.ServiceUnavailable ||
        //        r.StatusCode == HttpStatusCode.TooManyRequests)
        //    .WaitAndRetryAsync(
        //        3,
        //        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        //        onRetry: async (outcome, timespan, retryCount, context) =>
        //        {
        //            if (outcome.Result?.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                _logger.LogWarning("Unauthorized response, attempting re-login (retry {RetryCount})", retryCount);
        //                await LoginAsync();
        //            }
        //            else
        //            {
        //                _logger.LogWarning("Retrying after {Timespan}s (retry {RetryCount})", timespan.TotalSeconds, retryCount);
        //            }
        //        });
    }

    public async Task LoginAsync()
    {
        try
        {
            var loginData = new
            {
                CompanyDB = _companyDb,
                UserName = _userName,
                Password = _password
            };

            var json = Utils.SerializeToStringJson(loginData);
            //await _log($"Login payload: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Login", content);

            if (response.IsSuccessStatusCode)
            {
                var cookies = _cookieContainer.GetCookies(new Uri(_serviceLayerUrl));
                _sessionId = cookies["B1SESSION"]?.Value;
                _lastLoginTime = DateTime.UtcNow;
                return;
            }

            var error = await response.Content.ReadAsStringAsync();
            await _logError($"Login failed: {response.StatusCode} - {error}");
        }
        catch (Exception ex)
        {
            await _logError($"Login failed: {ex}");
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (string.IsNullOrEmpty(_sessionId) || DateTime.UtcNow - _lastLoginTime > _sessionTimeout)
            await LoginAsync();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    //Get
    public async Task<T> GetFirst<T>(string endpoint, FilterRule rule = null) where T : BaseSapEntity
    {
        return (await Get<T>(endpoint, rule)).FirstOrDefault();
    }

    public async Task<List<T>> Get<T>(string endpoint, FilterRule rule = null) where T : BaseSapEntity
    {
        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(c => c.CanWrite)
            .Where(c => !Attribute.IsDefined(c, typeof(JsonPropertyAttribute)))
            .Select(c => c.Name).ToList();

        var result = new List<T>();
        var page = 0;
        var pageSize = 20;
        while (true)
        {
            var queryParams = new Dictionary<string, object>();
            if (rule != null)
                queryParams["$filter"] = rule.ToOdataQuery();// $"UpdateDate ge '{lastUpdated.Value:yyyy-MM-dd}' and UpdatedTime ge '{lastUpdated.Value:HH:mm:ss'Z'}'";
            queryParams["$skip"] = page * pageSize;
            queryParams["$top"] = pageSize;
            queryParams["$select"] = string.Join(",", props);
            var qry = $"{endpoint}?{string.Join("&", queryParams.Select(c => $"{c.Key}={c.Value}"))}";
            var resp = await GetAsync<List<T>>(qry);
            result.AddRange(resp);
            
            //await _log($"Qry: {qry} - {resp.Count} fetched");
            
            if (resp.Count < pageSize)
                break;
            page++;
        }

        return result;
    }

    //Low level
    public async Task<T> GetAsync<T>(string endpoint)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Utils.DeserializeFromJson<T>(content, "value");
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new BusinessWebException(response.StatusCode, error);
    }

    public async Task<T> Post<T>(string endpoint, object payload, Func<string, Task> printLog = null)
    {
        var content = await Post(endpoint, payload, printLog);
        return Utils.DeserializeFromJson<T>(content);
    }

    public async Task<string> Post(string endpoint, object payload, Func<string, Task> printLog = null)
    {
        await EnsureAuthenticatedAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (payload != null)
        {
            var serializedData = Utils.SerializeToStringJson(payload);
            request.Content = new StringContent(serializedData, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        var error = await response.Content.ReadAsStringAsync();

        if (printLog != null)
            await printLog($@"
Request: POST - {_httpClient.BaseAddress}{endpoint}
{Utils.SerializeToStringJson(payload, Formatting.Indented)}
Response: {response.StatusCode.ToString()} ({(int)response.StatusCode})
{error}");

        var parsed = Utils.DeserializeFromJson<ErrorSap>(error, "error");
        throw new BusinessWebException(response.StatusCode, parsed.Message);
    }

    //Static helpers
    public static FilterRule GetLastUpdatedRule(DateTime? lastUpdated) => lastUpdated == null
        ? null
        : new FilterRule(ConditionType.Or, [
            new(nameof(ProductSap.UpdateDate), Operator.Gt, $"'{lastUpdated.Value:yyyy-MM-dd}'"),
            new(ConditionType.And, [
                new(nameof(ProductSap.UpdateDate), Operator.Eq, $"'{lastUpdated.Value:yyyy-MM-dd}'"),
                new(nameof(ProductSap.UpdateTime), Operator.Gt, $"'{lastUpdated.Value:HH:mm:ss}'"),
            ])
        ]);
}