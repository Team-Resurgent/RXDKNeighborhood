using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDKTestRig
{
    public static class ParamParser
    {
        public static Dictionary<string, string> ParseParams(string input)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(input))
            {
                return result;
            }

            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                string key, value;

                int eqIndex = token.IndexOf('=');
                if (eqIndex == -1)
                {
                    key = token.Trim();
                    value = "";
                }
                else
                {
                    key = token.Substring(0, eqIndex).Trim();
                    value = token.Substring(eqIndex + 1).Trim().Trim('"');

                    // Convert hex (0x...) to decimal
                    if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ulong.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var dec))
                        {
                            value = dec.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(key))
                {
                    result[key] = value;
                }
            }

            return result;
        }
    }
}
