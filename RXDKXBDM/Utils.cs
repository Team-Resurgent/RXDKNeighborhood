using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKXBDM
{
    public static class Utils
    {
        public static Dictionary<string, string> StringToDictionary(string value)
        {
            var parts = value.Split(" ").ToArray();
            var result = new Dictionary<string, string>();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i] ?? string.Empty;
                var keyValue = part.Split('=');
                result[keyValue[0]] = keyValue.Length >= 2 ? keyValue[1] : string.Empty;
            }
            return result;
        }
    }
}
