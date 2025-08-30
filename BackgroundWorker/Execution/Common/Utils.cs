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

    public static string GetExceptionMessage(Exception ex)
    {
        if (ex is AggregateException agr)
            return string.Join(Environment.NewLine, agr.InnerExceptions.Select(c => c.Message)); 
        return ex.Message;
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

    public static (string key, string[] replace) PrepareForTranslation(string key)
    {
        var chars = key.ToCharArray();
        var result = new StringBuilder(1000);
        var localInBracket = new StringBuilder(1000);
        var elementsToFill = new List<string>(50);

        var inBracket = false;

        for (var i = 0; i < key.Length; i++)
        {
            if (chars[i] == ']' && inBracket)
            {
                result.Insert(result.Length, $"{{{elementsToFill.Count}}}");
                elementsToFill.Add(localInBracket.ToString());
                localInBracket.Clear();
                inBracket = false;
            }

            if (!inBracket)
                result.Append(chars[i]);

            if (inBracket)
                localInBracket.Append(chars[i]);

            if (chars[i] == '[' && !inBracket)
                inBracket = true;
        }

        if (inBracket)
            result.Append(localInBracket);

        return (result.ToString(), elementsToFill.ToArray());
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

    public static string CalculateHash(string payload, string apiKey)
    {
        using var hash = new HMACSHA256(Convert.FromBase64String(apiKey));
        return Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(payload)));
    }

    public const string Numbers = "0123456789";
    public const string SpecialCharacters = "!@#$%^&*()_=-=*";
    public const string LowerCaseLetters = "qwertyuiopasdfghjklzxcvbnm";
    public const string UpperCaseLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";

    public static string GenerateRandomString(int characters = 12, bool includeNumbers = true, bool includeUpperCase = true, bool includeSpecials = true)
    {
        var allowedCharacters = LowerCaseLetters;
        if (includeUpperCase)
            allowedCharacters += UpperCaseLetters;
        if (includeNumbers)
            allowedCharacters += Numbers;
        if (includeSpecials)
            allowedCharacters += SpecialCharacters;

        var charArray = new char[characters];

        var rnd = new Random((int)DateTime.UtcNow.Ticks);
        while (!IsPasswordLegit(charArray, includeNumbers, includeUpperCase, includeSpecials))
            for (var i = 0; i < characters; i++)
                charArray[i] = allowedCharacters[rnd.Next(0, allowedCharacters.Length - 1)];

        return new(charArray);
    }

    public static bool IsPasswordLegit(char[] charArray, bool includeNumbers = true, bool includeUpperCase = true, bool includeSpecials = true)
    {
        return (!includeNumbers || charArray.Any(c => Numbers.Contains(c)))
               && (!includeUpperCase || charArray.Any(c => UpperCaseLetters.Contains(c)))
               && (!includeSpecials || charArray.Any(c => SpecialCharacters.Contains(c)));
    }

    public static string EntryPath
    {
        get
        {
            var codeBase = Assembly.GetEntryAssembly()?.CodeBase;
            if (codeBase == null)
                return null;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }

    public static string CommonAssemblyPath
    {
        get
        {
            var codeBase = typeof(Utils).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }

    public static string GetBigTextFromNumber(int number)
    {
        if (number < Alphabet.Length)
            return Alphabet[number].ToString();
        return GetBigTextFromNumber(number / Alphabet.Length - 1) + Alphabet[number % Alphabet.Length];
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

    public static string GenerateBolWithCheckDigit(string input)
    {
        if (input.Length != 16)
            return input;

        var isOdd = true;
        var oddSum = 0;
        var evenSum = 0;
        var result = input;

        try
        {
            foreach (var digitAsStr in input)
            {
                var digit = (int)char.GetNumericValue(digitAsStr);
                if (isOdd)
                    oddSum += digit;
                else
                    evenSum += digit;
                isOdd = !isOdd;
            }

            var lastDigit = (evenSum * 3 + oddSum) % 10;
            result += lastDigit == 0 ? "0" : (10 - lastDigit).ToString();
        }
        catch
        {
            return result;
        }

        return result;
    }

    public static string GenerateApiKey()
    {
        return $"{Convert.ToBase64String(new HMACSHA256().Key)}";
    }

    public static int GetMod10CheckDigit(string val)
    {
        var characters = val.ToCharArray();
        var sum = 0;
        for (var i = characters.Length; i > 0; i--)
        {
            var digit = Convert.ToInt32(characters[i - 1]);
            var multiplier = 3;
            if (i % 2 == 0)
                multiplier = 1;
            sum += digit * multiplier;
        }

        var checkDigit = (10 - sum % 10) % 10;
        return checkDigit;
    }

    public static char CalculateCheckDigit(string code)
    {
        // https://www.gs1.org/services/how-calculate-check-digit-manually
        var reversed = code.Reverse().Skip(1);

        var sum = 0;
        var multiplier = 3;
        foreach (var c in reversed)
        {
            if (multiplier == 3)
            {
                sum += (c - 0x30) * multiplier;
                multiplier = 1;
            }
            else
            {
                sum += (c - 0x30) * multiplier;
                multiplier = 3;
            }
        }

        int closestTenth = (sum + 9) / 10 * 10;

        return (char)(closestTenth - sum + 0x30);
    }

    public static List<string> ExtractQueryParameters(string qry)
    {
        var result = new List<string>();
        var remainingStr = qry;

        while (true)
        {
            var firstIndex = remainingStr.IndexOf("@{", StringComparison.Ordinal);
            if (firstIndex == -1)
                break;
            remainingStr = remainingStr.Substring(firstIndex+2);
            var secondIndex = remainingStr.IndexOf("}", StringComparison.Ordinal);
            if (secondIndex == -1)
                break;

            var str = remainingStr[..secondIndex];
            if(!result.Contains(str))
                result.Add(str);
            remainingStr = remainingStr[(secondIndex+1)..];
        }

        return result;
    }

    public static byte[] ReadStream(Stream sourceStream)
    {
        using var memoryStream = new MemoryStream();
        sourceStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static string SpaceCamel(string input, bool nonCaps = false)
    {
        var result = string.IsNullOrWhiteSpace(input) ? input : Regex.Replace(input, "(\\B[A-Z]+?(?=[A-Z][^A-Z])|\\B[A-Z]+?(?=[^A-Z]))", " $1");
        if (nonCaps && !string.IsNullOrWhiteSpace(result))
        {
            var split = result.Split(' ');
            for (var i = 1; i < split.Length; i++)
                split[i] = split[i].ToLower();
            result = string.Join(' ', split);
        }
        return result;
    }

    public static byte[] FromBase64String(string base64Str)
    {
        var str = base64Str?.Trim('"');
        if (str?.StartsWith("data:image/png;base64,") == true)
            str = str.Substring("data:image/png;base64,".Length);
        return str == null ? null : Convert.FromBase64String(str);
    }
    
    public static string FirstOrDefault(params string[] vals)
    {
        return vals.FirstOrDefault(val => !string.IsNullOrWhiteSpace(val));
    }

    public static Dictionary<UnitOfMeasure, Dictionary<UnitOfMeasure, double>> ConversionTable = new()
    {
        //{
        //    UnitOfMeasure.Ton,
        //    new Dictionary<UnitOfMeasure, double>
        //    {
        //        { UnitOfMeasure.Ton, 1 },
        //        { UnitOfMeasure.Kg, 1000 },
        //        { UnitOfMeasure.Gr, 1000000 },
        //        { UnitOfMeasure.Lb, 2204.623 },
        //        { UnitOfMeasure.Oz, 35273.96 }
        //    }
        //},
        {
            UnitOfMeasure.Kg,
            new()
            {
                //{ UnitOfMeasure.Ton, 0.001 },
                { UnitOfMeasure.Kg, 1 },
                { UnitOfMeasure.Gr, 1000 },
                { UnitOfMeasure.Lb, 2.204623 },
                { UnitOfMeasure.Oz, 35.27396 }
            }
        },
        {
            UnitOfMeasure.Gr,
            new()
            {
                //{ UnitOfMeasure.Ton, 0.000001 },
                { UnitOfMeasure.Kg, 0.001 },
                { UnitOfMeasure.Gr, 1 },
                { UnitOfMeasure.Lb, 0.002204623 },
                { UnitOfMeasure.Oz, 0.03527396 }
            }
        },
        {
            UnitOfMeasure.Lb,
            new()
            {
                //{ UnitOfMeasure.Ton, 0.004535924 },
                { UnitOfMeasure.Kg, 0.4535924 },
                { UnitOfMeasure.Gr, 453.5924 },
                { UnitOfMeasure.Lb, 1 },
                { UnitOfMeasure.Oz, 16 }
            }
        },
        {
            UnitOfMeasure.Oz,
            new()
            {
                //{ UnitOfMeasure.Ton, 0.0002834952 },
                { UnitOfMeasure.Kg, 0.02834952 },
                { UnitOfMeasure.Gr, 28.34952 },
                { UnitOfMeasure.Lb, 0.0625 },
                { UnitOfMeasure.Oz, 1 }
            }
        },
        {
            UnitOfMeasure.M,
            new()
            {
                { UnitOfMeasure.M, 1 },
                { UnitOfMeasure.Cm, 100 },
                { UnitOfMeasure.Mm, 1000 },
                { UnitOfMeasure.Ft, 3.28084 },
                { UnitOfMeasure.In, 39.37008 }
            }
        },
        {
            UnitOfMeasure.Cm,
            new()
            {
                { UnitOfMeasure.M, 0.01 },
                { UnitOfMeasure.Cm, 1 },
                { UnitOfMeasure.Mm, 10 },
                { UnitOfMeasure.Ft, 0.0328084 },
                { UnitOfMeasure.In, 0.3937008 }
            }
        },
        {
            UnitOfMeasure.Mm,
            new()
            {
                { UnitOfMeasure.M, 0.001 },
                { UnitOfMeasure.Cm, 0.1 },
                { UnitOfMeasure.Mm, 1 },
                { UnitOfMeasure.Ft, 0.00328084 },
                { UnitOfMeasure.In, 0.03937008 }
            }
        },
        {
            UnitOfMeasure.Ft,
            new()
            {
                { UnitOfMeasure.M, 0.3048 },
                { UnitOfMeasure.Cm, 30.48 },
                { UnitOfMeasure.Mm, 304.8 },
                { UnitOfMeasure.Ft, 1 },
                { UnitOfMeasure.In, 12 }
            }
        },
        {
            UnitOfMeasure.In,
            new()
            {
                { UnitOfMeasure.M, 0.0254 },
                { UnitOfMeasure.Cm, 2.54 },
                { UnitOfMeasure.Mm, 25.4 },
                { UnitOfMeasure.Ft, 1 / 12d },
                { UnitOfMeasure.In, 1 }
            }
        }
    };

    public static decimal? ConvertUoM(UnitOfMeasure? from, UnitOfMeasure? to, decimal? val, bool cube = false)
    {
        if (val == null || from == null || to == null)
            return null;
        if (!ConversionTable.TryGetValue(from.Value, out var table))
            return null;
        if (!table.TryGetValue(to.Value, out var coef))
            return null;

        if (cube)
            coef = coef * coef * coef;

        return val * (decimal)coef;
    }

    public static DateTime Max(DateTime v1, DateTime v2)
    {
        return v1 > v2 ? v1 : v2;
    }

    public static DateTimeOffset Max(DateTimeOffset v1, DateTimeOffset v2)
    {
        return v1 > v2 ? v1 : v2;
    }

    public static bool IsEqual(decimal v1, decimal v2, decimal tolerance = 0.00001m)
    {
        return Math.Abs(v1 - v2) < tolerance;
    }

    private static string _processName;
    public static string ProcessName => _processName ??= Process.GetCurrentProcess().ProcessName;
    public static bool IsDebug => ProcessName == "iisexpress" || ProcessName == "Tester.exe";

    public static List<IDictionary<string, object>> NormalizeGridData(IEnumerable<dynamic> data, TimeSpan? timeOffset = null)
    {
        var truncatedResult = data.Cast<IDictionary<string, object>>().ToList();
        if (timeOffset == null)
            return truncatedResult;

        //Normalize dates to Tenant time
        foreach (var row in truncatedResult)
        foreach (var column in row)
            if (column.Value is DateTime date)
                row[column.Key] = date.AddHours(-timeOffset.Value.TotalHours);

        return truncatedResult;
    }

    public static string VerifyTenantName(string name)
    {
        if ((name?.Length??0) <= 3)
            throw new BusinessWebException($"Alias [{name}] is too short, must be at least 4 characters long!");
        var alias = new Regex("[^a-zA-Z0-9]").Replace(name, "").ToLower().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
        var notAllowedAliases = new[] { "master", "docs", "version", "bin", "hh", "signalr", "api", "app", "odata", "data", "resource", "testData", "Binaries", "Resources", "UI", "Scripts" }
            .Select(c => c.ToLower().Trim());
        if (notAllowedAliases.Contains(alias))
            throw new BusinessWebException($"Alias [{alias}] cannot be used!");
        return alias;
    }
}

public enum UnitOfMeasure
{
    Gr,
    Kg,
    //Ton,

    Oz,
    Lb,

    Ml,
    L,

    Pt,
    Gal,

    Mm,
    Cm,
    M,

    In,
    Ft
}