using System.Collections.Concurrent;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public static class Extensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> inv)
    {
        if (source == null)
            return null;
        var list = source.ToList();
        foreach (var item in list)
            inv?.Invoke(item);
        return list;
    }

    public static IQueryable<T> ForEach<T>(this IQueryable<T> source, Action<T> inv)
    {
        if (source == null)
            return null;
        foreach (var item in source)
            inv?.Invoke(item);
        return source;
    }
    
    public static async Task ExecuteInParallel<T>(this IEnumerable<T> collection, Func<T, Task> processor, int threadCount = 3)
    {
        var queue = new ConcurrentQueue<T>(collection);
        var tasks = Enumerable.Range(0, threadCount).Select(async _ =>
        {
            while (queue.TryDequeue(out var item))
                await processor(item);
        });
        await Task.WhenAll(tasks);
    }

    public static void AddRange<T>(this ConcurrentBag<T> collection, IEnumerable<T> toAdd) => toAdd.AsParallel().ForAll(collection.Add);

    //https://docs.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/like-operator
    public static bool Like(this string toSearch, string toFind) =>
        new Regex($@"\A{new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
            .Replace(toFind, c => $@"\{c}")
            .Replace('_', '.')
            .Replace("%", ".*")}\z", RegexOptions.Singleline)
            .IsMatch(toSearch);

    public static string Truncate(this string value, int maxLength) =>
        string.IsNullOrEmpty(value) ? value : value[..Math.Min(value.Length, maxLength)];

    public static DateTimeOffset RoundDown(this DateTimeOffset dt, TimeSpan d) => 
        dt.AddTicks(-(dt.Ticks % d.Ticks));

    public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default) =>
        dict.TryGetValue(key, out var value) ? value : defaultValue;
    
    public static decimal? GetDecimal(this IDictionary<string, object> dict, string key, bool throwError = false, decimal? defaultValue = default) =>
        dict.GetString(key, throwError).ParseDecimalNullable(throwError, defaultValue);

    public static int? GetInt(this IDictionary<string, object> dict, string key, bool throwError = false, int? defaultValue = null) =>
        dict.GetString(key, throwError).ParseIntNullable(throwError, defaultValue);

    public static Guid? GetGuid(this IDictionary<string, object> dict, string key, bool throwError = false, Guid? defaultValue = null) =>
        dict.GetString(key, throwError).ParseGuidNullable(throwError, defaultValue);

    public static DateTime? GetDate(this IDictionary<string, object> dict, string key, bool throwError = false, DateTime? defaultValue = null) =>
        dict.GetString(key, throwError).ParseDateTimeNullable(throwError, defaultValue);

    public static T GetEnum<T>(this IDictionary<string, object> dict, string key, bool throwError = false, T defaultValue = default) where T : struct => 
        dict.GetString(key, throwError).ParseEnum(throwError, defaultValue);

    public static string GetString(this IDictionary<string, object> dict, string key, bool throwError = false, string defaultValue = null)
    {
        try
        {
            return dict.TryGetValue(key, out var strObj) ? strObj?.ToString() : throw new BusinessWebException(key, $"[{key}] is missing");
        }
        catch
        {
            if (throwError)
                throw;
            return defaultValue;
        }
    }

    public static bool ContainsAll(this IDictionary<string, object> dict, params string[] keys)
    {
        return keys.All(dict.ContainsKey);
    }

    public static bool ContainsAny(this IDictionary<string, object> dict, params string[] keys)
    {
        return keys.Any(dict.ContainsKey);
    }

    public static T ParseEnum<T>(this string value, bool throwExc = true, T defaultVal = default) =>
        Enum.TryParse(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T), value, true, out var result) ? (T)result :
        !throwExc ? defaultVal : throw new ArgumentException($"Invalid {typeof(T).Name} [{value}]");

    public static long ParseLong(this string value, bool throwExc = true, long defaultVal = default) =>
        long.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid long [{value}]");

    public static int ParseInt(this string value, bool throwExc = true, int defaultVal = default) =>
        int.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid int [{value}]");

    public static double ParseDouble(this string value, bool throwExc = true, double defaultVal = default) =>
        double.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid double [{value}]");

    public static decimal ParseDecimal(this string value, bool throwExc = true, decimal defaultVal = default) =>
        decimal.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid decimal [{value}]");

    public static Guid ParseGuid(this string value, bool throwExc = true, Guid defaultVal = default) =>
        Guid.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid GUID [{value}]");

    public static bool ParseBool(this string value, bool throwExc = true, bool defaultVal = default) =>
        bool.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid bool [{value}]");

    public static DateTime ParseDateTime(this string value, bool throwExc = true, DateTime defaultVal = default) => 
        DateTime.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid DateTime [{value}]");

    public static DateTimeOffset ParseDateTimeOffset(this string value, bool throwExc = true, DateTimeOffset defaultVal = default) =>
        DateTimeOffset.TryParse(value, out var result) ? result : !throwExc ? defaultVal : throw new ArgumentException($"Invalid DateTimeOffset [{value}]");

    public static long? ParseLongNullable(this string value, bool throwExc = true, long? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : long.TryParse(value, out var r) ? r : !throwExc ? defaultValue: throw new ArgumentException($"Invalid long [{value}]");

    public static int? ParseIntNullable(this string value, bool throwExc = true, int? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : int.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid int [{value}]");

    public static double? ParseDoubleNullable(this string value, bool throwExc = true, double? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : double.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid double [{value}]");

    public static decimal? ParseDecimalNullable(this string value, bool throwExc = true, decimal? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : decimal.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid decimal [{value}]");

    public static Guid? ParseGuidNullable(this string value, bool throwExc = true, Guid? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : Guid.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid GUID [{value}]");

    public static bool? ParseBoolNullable(this string value, bool throwExc = true, bool? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : bool.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid bool [{value}]");

    public static DateTime? ParseDateTimeNullable(this string value, bool throwExc = true, DateTime? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : DateTime.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid DateTime [{value}]");

    public static DateTimeOffset? ParseDateTimeOffsetNullable(this string value, bool throwExc = true, DateTimeOffset? defaultValue = null) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : DateTimeOffset.TryParse(value, out var r) ? r : !throwExc ? defaultValue : throw new ArgumentException($"Invalid DateTimeOffset [{value}]");

    public static bool TryGetValue<T>(this IDictionary<string, object> source, string key, out T result)
    {
        var parseRes = source.TryGetValue(key, out var rawResult);
        result = default;
        if (!parseRes || rawResult is not T value)
            return false;
        result = value;
        return true;
    }

    public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

    public static IDictionary<string, object> AsDictionary(this object source)
    {
        return source.GetType().GetProperties()
            .ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
    }
    
    public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
    {
        var newObj = new T();
        var newObjType = newObj.GetType();

        foreach (var (key, value) in source)
        {
            var property = newObjType.GetProperty(key);
            if (property == null)
                continue;
            if(property.PropertyType == value?.GetType())
                property.SetValue(newObj, value, null);
            else if (property.PropertyType == typeof(string))
                property.SetValue(newObj, value?.ToString());
            else if (property.PropertyType == typeof(int))
                property.SetValue(newObj, value?.ToString().ParseInt());
            else if (property.PropertyType == typeof(int?))
                property.SetValue(newObj, value?.ToString().ParseIntNullable());

            else if (property.PropertyType == typeof(Guid))
                property.SetValue(newObj, value?.ToString().ParseGuid());
            else if (property.PropertyType == typeof(Guid?))
                property.SetValue(newObj, value?.ToString().ParseGuidNullable());

            else if (property.PropertyType == typeof(decimal))
                property.SetValue(newObj, value?.ToString().ParseDecimal());
            else if (property.PropertyType == typeof(decimal?))
                property.SetValue(newObj, value?.ToString().ParseDecimalNullable());

            else if (property.PropertyType == typeof(DateTime))
                property.SetValue(newObj, value?.ToString().ParseDateTime());
            else if (property.PropertyType == typeof(DateTime?))
                property.SetValue(newObj, value?.ToString().ParseDateTimeNullable());

            else if (property.PropertyType == typeof(DateTimeOffset))
                property.SetValue(newObj, value?.ToString().ParseDateTimeOffset());
            else if (property.PropertyType == typeof(DateTimeOffset?))
                property.SetValue(newObj, value?.ToString().ParseDateTimeOffsetNullable());

            else if (property.PropertyType.IsEnum)
                property.SetValue(newObj, Enum.TryParse(property.PropertyType, value?.ToString(), true, out var e) ? e : throw new BusinessWebException($"Invalid [{property.PropertyType.Name}] [{value}]. Cannot be parsed"));
            else if (Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                property.SetValue(newObj, string.IsNullOrWhiteSpace(value?.ToString()) ? null : Enum.TryParse(underlyingType, value.ToString(), true, out var vl) ? vl : throw new BusinessWebException($"Invalid [{underlyingType.Name}] [{value}]. Cannot be parsed"));
            }
                
            else
                throw new BusinessWebException($"Property [{property.Name}] cannot be parsed");
        }

        return newObj;
    }

    public static string ReadableTimeSpan(this TimeSpan timeSpan, Func<string, string> translate = null)
    {
        var days = timeSpan.Days > 0 ? timeSpan.Days + (translate?.Invoke("d") ?? "d") : null;
        var hours = timeSpan.Hours > 0 ? timeSpan.Hours + (translate?.Invoke("hr") ?? "hr") : null;
        var mins = timeSpan.Minutes > 0 ? timeSpan.Minutes + (translate?.Invoke("m") ?? "m") : null;
        var secs = timeSpan.Seconds > 0 ? timeSpan.Seconds + (translate?.Invoke("s") ?? "s") : null;
        return $"{days} {hours} {mins} {secs}";
    }

    public static decimal? Round(this decimal? v, int decimals)
    {
        return v == null ? null : Math.Round(v.Value, decimals);
    }

    public static double? Round(this double? v, int decimals)
    {
        return v == null ? null : Math.Round(v.Value, decimals);
    }

    public static bool IsSuccessStatusCode(this int statusCode)
    {
        return statusCode >= 200 && (statusCode <= 299);
    }

    //Dictionary ops
    public static dynamic ToDynamic<T>(this IDictionary<string, T> source) where T : class
    {
        var expando = new ExpandoObject() as IDictionary<string, object>;
        foreach (var kvp in source)
            expando[kvp.Key] = kvp.Value;
        return expando;
    }

    public static IDictionary<string, object> KeepOnlyKeys(this IDictionary<string, object> hash, params string[] keys)
    {
        hash.Keys
            .Where(c => c != "Id")
            .Where(c => !c.StartsWith("U_"))
            .Where(c => !keys.Contains(c)).ToList().ForEach(c => hash.Remove(c));
        return hash;
    }

    public static IDictionary<string, object> ExcludeKeys(this IDictionary<string, object> hash, params string[] keys)
    {
        hash.Keys.Where(keys.Contains).ToList().ForEach(c => hash.Remove(c));
        return hash;
    }

    public static IDictionary<string, object> SetIfNotExist(this IDictionary<string, object> hash, string key, object value)
    {
        if (hash.ContainsKey(key))
            return hash;
        hash[key] = value;
        return hash;
    }

    public static IDictionary<string, object> SetOrRemoveIfNull(this IDictionary<string, object> hash, string key, object value)
    {
        if(value == null)
            hash.Remove(key);
        else
            hash[key] = value;
        return hash;
    }
}