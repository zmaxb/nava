using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nava.Core.Utils;

public static class JsonHelper
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = new List<JsonConverter> { new StringEnumConverter() },
        Formatting = Formatting.None
    };

    public static string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, Settings);
    }
}