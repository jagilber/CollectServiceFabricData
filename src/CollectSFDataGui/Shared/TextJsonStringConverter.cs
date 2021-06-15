using CollectSFData.Common;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollectSFDataGui.Shared
{
    public class JsonHelpers
    {
        /// <summary>
        /// Helper method to convert the raw "json response from file upload activity" 
        /// into custom type "ConfigurationProperties".
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        public static async Task<ConfigurationProperties> ToConfigurationProperties(string rawResponse)
        {
            ConfigurationProperties configProperties = null;
            byte[] rawResponseArray = null;
            MemoryStream rawResponseStream = null;

            try
            {
                // If there is no data in raw response, return
                if (string.IsNullOrEmpty(rawResponse))
                {
                    return configProperties;
                }

                // Convert response JSON string into stream
                rawResponseArray = Encoding.UTF8.GetBytes(rawResponse);
                rawResponseStream = new MemoryStream(rawResponseArray);

                // De-serialize for write indented JSON data
                var serializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                configProperties = await JsonSerializer.DeserializeAsync<ConfigurationProperties>(rawResponseStream, serializerOptions);

                return configProperties;
            }
            finally
            {
                // Reset the object references once job done
                if(rawResponseArray != null)
                {
                    rawResponseArray = null;
                }
                if(rawResponseStream != null)
                {
                    rawResponseStream.Close();
                }
                if(configProperties != null)
                {
                    configProperties = null;
                }
            }
        }

        public static JsonSerializerOptions GetJsonSerializerOptions(bool indented = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonHelpers.StringConverter());
            options.AllowTrailingCommas = true;
            options.MaxDepth = 5;
            options.PropertyNameCaseInsensitive = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.WriteIndented = indented;
            return options;
        }

        public class StringConverter : System.Text.Json.Serialization.JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    var stringValue = reader.GetInt32();
                    return stringValue.ToString();
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString();
                }

                throw new System.Text.Json.JsonException();
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
}