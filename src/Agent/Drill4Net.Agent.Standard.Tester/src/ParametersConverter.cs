using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Standard.Tester
{
    internal static class ParametersConverter
    {
        internal static object[] ConvertToRealParameters(List<string> parVals, string parTypes)
        {
            if (string.IsNullOrWhiteSpace(parTypes))
                return null;
            object[] res = new object[parVals.Count];
            var types = parTypes.Split(',');
            for (var i = 0; i < parVals.Count; i++)
            {
                object obj = null;
                var val = parVals[i];
                if (val != "null")
                {
                    var type = types[i];
                    var nullableToken = "System.Nullable`1<";
                    if (type.StartsWith(nullableToken))
                    {
                        type = type.Replace(nullableToken, null);
                        type = type[0..^1];
                    }
                    switch (type)
                    {
                        case "System.Boolean": obj = bool.Parse(val); break;
                        case "System.Byte": obj = byte.Parse(val); break;
                        case "System.UInt16": obj = ushort.Parse(val); break;
                        case "System.Int16": obj = short.Parse(val); break;
                        case "System.UInt32": obj = uint.Parse(val); break;
                        case "System.Int32": obj = int.Parse(val); break;
                        case "System.UInt64": obj = ulong.Parse(val); break;
                        case "System.Int64": obj = long.Parse(val); break;
                        case "System.Single": obj = float.Parse(val); break;
                        case "System.Double": obj = double.Parse(val); break;
                        case "System.Object":
                        case "System.String": obj = val; break;
                        default:
                            OutputInfoHelper.WriteMessage($"Unknown type: [{type}] for data [{val}]", ConsoleColor.DarkYellow);
                            break;
                    }
                }
                res[i] = obj;
            }
            return res;
        }
    }
}
