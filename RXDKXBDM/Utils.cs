using RXDKXBDM.Commands;

namespace RXDKXBDM
{
    public static class Utils
    {
        public static string[] SplitBySpaceIgnoringQuotes(string input)
        {
            var result = new List<string>();
            var currentPart = string.Empty;
            bool insideQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"') 
                {
                    insideQuotes = !insideQuotes;
                    continue; 
                }
                if (c == ' ' && !insideQuotes) 
                {
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        result.Add(currentPart);
                        currentPart = string.Empty; 
                    }
                }
                else
                {
                    currentPart += c; 
                }
            }

            if (!string.IsNullOrEmpty(currentPart))
            {
                result.Add(currentPart);
            }

            return result.ToArray();
        }

        public static bool IsSuccess(ResponseCode responseCode)
        {
            return (int)responseCode >= 200 && (int)responseCode <= 299;
        }

        public static IDictionary<string, string> BodyToDictionary(string[] body)
        {
            var result = new Dictionary<string, string>();
            for (int i = 0; i < body.Length; i++)
            {
                var line = body[i];
                var temp = StringToDictionary(line);
                var keys = temp.Keys.ToArray();
                for (var j = 0; j < keys.Length; j++)
                {
                    var key = keys[j];
                    result.Add(key, temp[key]);
                }
            }
            return result;
        }
       
        public static IDictionary<string, string>[] BodyToDictionaryArray(string[] body)
        {
            var result = new List<Dictionary<string, string>>();
            for (int i = 0; i < body.Length; i++)
            {
                var line = body[i];
                result.Add(StringToDictionary(line));
            }
            return result.ToArray();
        }

        public static Dictionary<string, string> StringToDictionary(string line)
        {
            var parts = SplitBySpaceIgnoringQuotes(line);
            var result = new Dictionary<string, string>();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i] ?? string.Empty;
                var keyValues = part.Split('=');
                var key = keyValues[0];
                if (keyValues.Length == 2)
                {
                    var value = keyValues[1];
                    if (value.StartsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    result[key] = value;
                }
                else
                {
                    result[key] = string.Empty;
                }
                
            }
            return result;
        }

        public static string GetDictionaryString(IDictionary<string, string> keyValues, string key)
        {
            var result = keyValues.TryGetValue(key, out string? value) ? value : "";
            return result;
        }

        public static uint GetDictionaryIntFromKey(IDictionary<string, string> keyValues, string key)
        {
            var value = GetDictionaryString(keyValues, key);
            if (value.EndsWith(","))
            {
                value = value.Substring(0, value.Length - 1);
            }
            if (string.IsNullOrEmpty(value) || uint.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var result) == false)
            {
                return 0;
            }
            return result;
        }

        public static ulong GetDictionaryLongFromKeys(IDictionary<string, string> keyValues, string hiKey, string loKey)
        {
            var hiValue = GetDictionaryIntFromKey(keyValues, hiKey);
            var loValue = GetDictionaryIntFromKey(keyValues, loKey);
            var result = ((ulong)hiValue << 32) | loValue;
            return result;
        }

        public static IDictionary<string, string> DateTimeToDictionary(DateTime dateTime)
        {
            DateTime fileTimeStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fileTimeTicks = (ulong)(dateTime.ToUniversalTime() - fileTimeStart).Ticks;
            uint hiValue = (uint)(fileTimeTicks >> 32);
            uint loValue = (uint)(fileTimeTicks & 0xFFFFFFFF);
            var result = new Dictionary<string, string>
            {
                { "hi", "0x" + hiValue.ToString("x8") },
                { "lo", "0x" + loValue.ToString("x8") }
            };
            return result;
        }


    }
}
