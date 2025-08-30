using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public class Utils
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string SerializeToStringJson(object obj, Formatting formatting = Formatting.None, bool throwError = true)
    {
        try
        {
            if (obj == null)
                return null;
            return JsonConvert.SerializeObject(obj, formatting, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
        catch
        {
            if (throwError)
                throw;
            return null;
        }
    }

    public static T DeserializeFromJson<T>(string str, string root = null, bool throwError = true, T defaultVal = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (throwError)
                    throw new BusinessWebException("Cannot deserialize empty string");
                return defaultVal;
            }

            var reader = new JsonTextReader(new StringReader(str));
            var serializer = new JsonSerializer { DateParseHandling = DateParseHandling.DateTimeOffset };
            var des = serializer.Deserialize(reader);
            if (des is not JToken jToken)
                return (T)des;
            if (root != null)
                jToken = jToken.SelectToken(root);
            return (T)jToken?.ToObject(typeof(T));
        }
        catch
        {
            if (throwError)
                throw;
            return defaultVal;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static byte[] ReadBinaryFile(string fileName)
    {
        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var buffer = new byte[(int)fileStream.Length];
        fileStream.Read(buffer, 0, buffer.Length);
        fileStream.Close();
        return buffer;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string ReadTextFile(string fileName, bool throwException = true, string defaultValue = default)
    {
        try
        {
            using var reader = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            return reader.ReadToEnd();
        }
        catch
        {
            if (throwException)
                throw;
            return defaultValue;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void WriteBinaryFile(string fileName, byte[] data)
    {
        FileInfo file = new FileInfo(fileName);
        if (!file.Directory.Exists)
            file.Directory.Create();
        var stream = new FileStream(fileName, FileMode.Create);
        stream.Write(data, 0, data.Length);
        stream.Close();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void WriteTextFile(string fileName, string data)
    {
        var file = new FileInfo(fileName);
        if (file.Directory != null && !file.Directory.Exists)
            file.Directory.Create();
        using var writer = new StreamWriter(fileName);
        writer.WriteLine(data);
        writer.Flush();
        writer.Close();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void AppendTextFile(string fileName, string data, bool appendLine = true)
    {
        var file = new FileInfo(fileName);
        if (file.Directory != null && !file.Directory.Exists)
            file.Directory.Create();
        using var writer = File.AppendText(file.FullName);
        if (appendLine)
            writer.WriteLine(data);
        else
            writer.Write(data);
        writer.Flush();
        writer.Close();
    }

    public static string GetMd5Hash(string source)
    {
        return GetMd5Hash(Encoding.UTF8.GetBytes(source));
    }

    public static string GetMd5Hash(byte[] source)
    {
        using var md5 = MD5.Create();
        return Convert.ToBase64String(md5.ComputeHash(source));
    }

    private static string _processName;
    public static string ProcessName => _processName ??= Process.GetCurrentProcess().ProcessName;
    public static bool IsDebug => ProcessName == "iisexpress" || ProcessName == "Tester.exe";
}