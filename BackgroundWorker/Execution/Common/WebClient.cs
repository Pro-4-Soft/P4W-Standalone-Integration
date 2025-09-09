using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public class WebClient
{
    public string Host { get; }
    public TimeSpan Timeout { get; }
    public readonly string ApiKey;
    private readonly string _apiKeyHeaderName;

    public readonly Dictionary<string, string> Headers = new();

    public WebClient(string host = null, string apiKey = null, string apiKeyHeaderName = null, TimeSpan? timeout = null)
    {
        Host = host;
        ApiKey = apiKey;
        _apiKeyHeaderName = apiKeyHeaderName ?? "ApiKey";
        Timeout = timeout ?? TimeSpan.FromMinutes(5);
    }

    //Sync
    public T GetInvoke<T>(string url, string root = null) where T : class
    {
        if (url.ToLower().StartsWith("/odata") || url.ToLower().StartsWith("odata"))
            root = "value";
        var content = GetInvoke(url);
        return Utils.DeserializeFromJson<T>(content, root);
    }

    public string GetInvoke(string url)
    {
        return WebInvoke(url).Content.ReadAsStringAsync().Result;
    }

    public T PostInvoke<T>(string url, object payload) where T : class
    {
        return Utils.DeserializeFromJson<T>(PostInvoke(url, payload));
    }

    public string PostInvoke(string url, object payload)
    {
        return WebInvoke(url, HttpMethod.Post, payload).Content.ReadAsStringAsync().Result;
    }

    public HttpResponseMessage WebInvoke(string url, HttpMethod method = null, object payload = null, int retry = 0)
    {
        return WebInvokeAsync(url, method, payload, retry).Result;
    }

    //Async
    public async Task<T> GetInvokeAsync<T>(string url, string root = null)
    {
        if (url.ToLower().StartsWith("/odata") || url.ToLower().StartsWith("odata"))
            root = "value";
        var content = await GetInvokeAsync(url);

        return Utils.DeserializeFromJson<T>(content, root);
    }

    public async Task<string> GetInvokeAsync(string url)
    {
        return await (await WebInvokeAsync(url)).Content.ReadAsStringAsync();
    }

    public async Task<byte[]> GetInvokeBytesAsync(string url)
    {
        return await (await WebInvokeAsync(url)).Content.ReadAsByteArrayAsync();
    }

    public async Task DownloadToFileAsync(string url, string fileName)
    {
        var resp = await WebInvokeAsync(url);
        await using var toStream = File.OpenWrite(fileName);
        await using var fromStream = await resp.Content.ReadAsStreamAsync();
        await fromStream.CopyToAsync(toStream);
        await fromStream.FlushAsync();
    }

    public async Task<Stream> GetInvokeStreamAsync(string url)
    {
        return await (await WebInvokeAsync(url)).Content.ReadAsStreamAsync();
    }

    public async Task<T> PostInvokeAsync<T>(string url, object payload)
    {
        return Utils.DeserializeFromJson<T>(await PostInvokeAsync(url, payload));
    }

    public async Task<string> PostInvokeAsync(string url, object payload)
    {
        var content = await (await WebInvokeAsync(url, HttpMethod.Post, payload)).Content.ReadAsStringAsync();
        return content;
    }

    public async Task<byte[]> PostInvokeBytesAsync(string url, object payload)
    {
        return await (await WebInvokeAsync(url, HttpMethod.Post, payload)).Content.ReadAsByteArrayAsync();
    }

    public async Task<T> PutInvokeAsync<T>(string url, object payload)
    {
        return Utils.DeserializeFromJson<T>(await PutInvokeAsync(url, payload));
    }

    public async Task<string> PutInvokeAsync(string url, object payload)
    {
        var content = await (await WebInvokeAsync(url, HttpMethod.Put, payload)).Content.ReadAsStringAsync();
        return content;
    }

    public async Task<HttpResponseMessage> WebInvokeAsync(string url, HttpMethod method = null, object payload = null, int retry = 0)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new BusinessWebException("Url cannot be null or empty");
        string host = null;
        if (!string.IsNullOrWhiteSpace(Host))
            host = Host?.Trim().TrimEnd('/') +"/";

        var request = new HttpRequestMessage(method ?? HttpMethod.Get, $"{host}{url.Trim().TrimStart('/')}");
        if(!string.IsNullOrWhiteSpace(ApiKey))
            request.Headers.Add(_apiKeyHeaderName, ApiKey);

        foreach(var key in Headers.Keys)
            request.Headers.Add(key, Headers[key]);

        if (payload != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
        {
            var serializedData = Utils.SerializeToStringJson(payload);
            request.Content = new StringContent(serializedData, Encoding.UTF8, "application/json");
        }
        var response = await new HttpClient
        {
            Timeout = Timeout,
        }.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode) 
            return response;

        var error = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != (HttpStatusCode)429) 
            throw new BusinessWebException(response.StatusCode, error);

        if (retry > 20)
            throw new BusinessWebException(response.StatusCode, error);

        Thread.Sleep(TimeSpan.FromSeconds(3));
        return await WebInvokeAsync(url, method, payload, retry + 1);
    }
    
    public async Task<byte[]> UploadStreamBytesAsync(string url, Stream source, string fileName = null, string contentType = null)
    {
        return await (await UploadStreamAsync(url, source, fileName, contentType)).Content.ReadAsByteArrayAsync();
    }

    public async Task<T> UploadStreamAsync<T>(string url, Stream source, string fileName = null, string contentType = null) where T : class
    {
        var resp = await UploadStreamAsync(url, source, fileName, contentType);
        return Utils.DeserializeFromJson<T>(await resp.Content.ReadAsStringAsync());
    }

    public async Task<HttpResponseMessage> UploadStreamAsync(string url, Stream source, string fileName = null, string contentType = null, int retry = 0)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Host}/{url.TrimStart('/')}");
        if(!string.IsNullOrWhiteSpace(ApiKey))
            request.Headers.Add("ApiKey", ApiKey);

        var multipartFormContent = new MultipartFormDataContent();
        
        var fileStreamContent = new StreamContent(source);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType??"image/png");

        multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName??"image.png");

        request.Content = multipartFormContent;

        var response = await new HttpClient().SendAsync(request);
        if (response.IsSuccessStatusCode) 
            return response;

        var error = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != (HttpStatusCode)429) 
            throw new BusinessWebException(response.StatusCode, error, false);

        if (retry > 20)
            throw new BusinessWebException(response.StatusCode, error, false);

        Thread.Sleep(TimeSpan.FromSeconds(3));
        return await UploadStreamAsync(url, source, fileName, contentType, retry + 1);
    }
}