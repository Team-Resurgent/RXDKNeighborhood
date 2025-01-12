using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBDMTest
{
    public static class Utils
    {
        public static bool IsSuccess(ResultCode hr)
        {
            return ((int)hr >= 200 && (int)hr <= 299);
        }

        public static string FGetSzParam(string line, string key)
        {
            return line.Contains(key) ? line.Substring(line.IndexOf(key) + key.Length).Trim() : string.Empty;
        }

        public static string? GetParam(string command, string key, bool needValue)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(key))
            {
                return null;
            }

            int index = 0;

            // Skip the command part
            while (index < command.Length && !char.IsWhiteSpace(command[index]))
            {
                index++;
            }

            while (index < command.Length)
            {
                // Skip leading spaces
                while (index < command.Length && char.IsWhiteSpace(command[index]))
                {
                    index++;
                }
                if (index >= command.Length)
                {
                    return null;
                }

                // Identify token
                int tokenStart = index;
                bool foundEquals = false;
                int tokenLength = 0;
                for (; index < command.Length; index++, tokenLength++)
                {
                    if (command[index] == '=')
                    {
                        foundEquals = true;
                        break;
                    }
                    if (char.IsWhiteSpace(command[index]))
                    {
                        break;
                    }
                }

                string token = command.Substring(tokenStart, tokenLength);

                // Check if token matches the key
                if (string.Equals(key, token, StringComparison.OrdinalIgnoreCase))
                {
                    if (foundEquals)
                    {
                        // Return value after '='
                        return command.Substring(index + 1);
                    }
                    if (!needValue)
                    {
                        // Return the key itself
                        return token;
                    }
                }

                // Skip past the value (handle quotes if present)
                bool inQuotes = false;
                while (index < command.Length && (!char.IsWhiteSpace(command[index]) || inQuotes))
                {
                    if (command[index] == '"')
                    {
                        inQuotes = !inQuotes;
                    }
                    index++;
                }
            }

            return null;
        }

        public static string GetParam(string szLine)
        {
            bool fQuote = false;

            var result = string.Empty;

            int i = 0; // Index for szLine
            while (i < szLine.Length && (!char.IsWhiteSpace(szLine[i]) || fQuote))
            {
                if (szLine[i] == '"')
                {
                    if (fQuote && i + 1 < szLine.Length && szLine[i + 1] == '"')
                    {
                        result += '"';
                        i += 2;
                    }
                    else
                    {
                        fQuote = !fQuote;
                        i++;
                    }
                }
                else
                {
                    result += szLine[i++];
                }
            }

            return result;
        }

        public static bool FGetSzParam(string szLine, string szKey, out string value)
        {
            value = string.Empty;

            var pch = GetParam(szLine, szKey, true);
            if (pch == null)
            {
                return false;
            }
            value =  GetParam(pch);
            return true;
        }

        public static bool FGetQwordParam(string line, string key, out uint lowPart, out uint highPart)
        {
            lowPart = 0;
            highPart = 0;

            var param = FGetSzParam(line, key);
            if (string.IsNullOrEmpty(param))
            {
                return false;
            }

            if (param.Length < 2 || param.StartsWith("0q") == false)
            {
                return false;
            }

            var paddedParam = param.Substring(2).PadLeft(16, '0');
            var lowPartHex = paddedParam.Substring(8, 8);
            var highPartHex = paddedParam.Substring(0, 8);

            if (uint.TryParse(lowPartHex, System.Globalization.NumberStyles.HexNumber, null, out lowPart) == false)
            {
                return false;
            }

            if (uint.TryParse(highPartHex, System.Globalization.NumberStyles.HexNumber, null, out highPart) == false)
            {
                return false;
            }

            return true;
        }

        public static bool FGetQwordParam(string line, string key, out ulong value)
        {
            var result = FGetQwordParam(line, key, out var lowPart, out var highPart);
            value = (highPart << 32) | lowPart;
            return result;
        }
    }
}
