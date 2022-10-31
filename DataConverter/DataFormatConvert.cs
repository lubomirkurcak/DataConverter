using Newtonsoft.Json;
using System.Xml.Linq;

namespace DataConverter
{
    static class DataFormatConverter
    {
        public static string Convert(string data, string inputContentType, string outputContentType)
        {
            data = inputContentType switch
            {
                "application/json" => data,
                "text/json" => data,
                "application/xml" => FromXml(data),
                "text/xml" => FromXml(data),
                _ => throw new InvalidContentTypeException($"Unsupported input Content-Type: {inputContentType}"),
            };

            data = outputContentType switch
            {
                "application/json" => data,
                "text/json" => data,
                "application/xml" => ToXml(data),
                "text/xml" => ToXml(data),
                _ => throw new InvalidContentTypeException($"Unsupported output Content-Type: {outputContentType}"),
            };

            return data;
        }

        public static string ToXml(string json)
        {
            json = $"{{root:{json}}}";
            return JsonConvert.DeserializeXNode(json)!.ToString();
        }

        public static string FromXml(string xml)
        {
            if (xml.StartsWith("<?xml")) xml = xml[(xml.IndexOf("?>") + 2)..];
            return JsonConvert.SerializeXNode(XDocument.Parse(xml), Formatting.None, true);
        }
    }
}
