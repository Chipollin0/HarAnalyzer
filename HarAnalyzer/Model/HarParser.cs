using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HarAnalyzer.Model
{
    public class HarParser
    {
        public static async Task<HarLog> Parse(string json)
        {
            var harLog = new HarLog();
            harLog.Entries = new List<HarEntry>();

            var jsonUtf8Stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var jsonDeserialized = await JsonSerializer.DeserializeAsync<object>(jsonUtf8Stream);
            if (jsonDeserialized == null)
                return harLog;

            var jsonElement = (JsonElement)jsonDeserialized;
            var jsonLogObject = jsonElement.GetProperty("log");
            var jsonEntries = jsonLogObject.GetProperty("entries").EnumerateArray();
            
            foreach (var jsonEntry in jsonEntries)
            {
                var priority = GetString(jsonEntry, "_priority");
                var resourceType = GetString(jsonEntry, "_resourceType");
                var pageRef = GetString(jsonEntry, "pageref");
                var connection = GetString(jsonEntry, "connection");
                var serverIpAddress = GetString(jsonEntry, "serverIPAddress");
                var startedDateTime = GetString(jsonEntry, "startedDateTime");
                var time = GetDouble(jsonEntry, "time");
                var timings = ""; // todo

                var harCache = new HarCache();
                var cache = GetProperty(jsonEntry, "cache");

                var harInitiator = new HarInitiator();
                var initiator = GetProperty(jsonEntry, "initiator");

                var harRequest = new HarRequest();
                var request = jsonEntry.GetProperty("request");
                harRequest.Method = GetString(request, "method");
                harRequest.Url = GetString(request, "url");
                harRequest.HttpVersion = GetString(request, "httpVersion");
                harRequest.Headers = GetHeaders(request, "headers");
                harRequest.Query = GetQueryPairs(request, "queryString");
                harRequest.Cookies = GetCookies(request, "cookies");
                harRequest.RedirectUrl = GetString(request, "redirectURL");
                harRequest.BodySize = GetInt(request, "bodySize");
                harRequest.HeadersSize = GetInt(request, "headersSize");

                var harResponse = new HarResponse();
                var response = jsonEntry.GetProperty("response");
                harResponse.Status = GetInt(response, "status");
                harResponse.StatusText = GetString(response, "statusText");
                harResponse.HttpVersion = GetString(response, "httpVersion");
                harResponse.Headers = GetHeaders(response, "headers");
                harResponse.Cookies = GetCookies(response, "cookies");
                harResponse.Content = ParseText(response, "content");
                harResponse.BodySize = GetInt(response, "bodySize");
                harResponse.HeadersSize = GetInt(response, "headersSize");
                harResponse.TransferSize = GetInt(response, "_transferSize");
                harResponse.Error = GetString(response, "_error");

                var harEntry = new HarEntry();
                harEntry.Priority = priority;
                harEntry.ResourceType = resourceType;
                harEntry.Cache = harCache;
                harEntry.PageRef = pageRef;
                harEntry.Connection = connection;
                harEntry.ServerIpAddress = serverIpAddress;
                harEntry.StartedDateTime = startedDateTime;
                harEntry.Time = time;
                harEntry.Timings = timings;
                harEntry.Initiator = harInitiator;
                harEntry.Request = harRequest;
                harEntry.Response = harResponse;
                harLog.Entries.Add(harEntry);
            }

            return harLog;
        }

        private static List<HarCookies>? GetCookies(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;

            var harCookiesList = new List<HarCookies>();
            var propertyArray = propertyElement.Value.EnumerateArray();
            foreach (var propertyArrayElement in propertyArray)
            {
                var harCookies = new HarCookies();
                harCookies.Name = GetString(propertyArrayElement, "name");
                harCookies.Value = GetString(propertyArrayElement, "value");
                harCookies.Path = GetString(propertyArrayElement, "path");
                harCookies.Domain = GetString(propertyArrayElement, "domain");
                harCookies.Expires = GetString(propertyArrayElement, "expires");
                harCookies.HttpOnly = GetBool(propertyArrayElement, "httpOnly");
                harCookies.Secure = GetBool(propertyArrayElement, "secure");
                harCookiesList.Add(harCookies);
            }

            return harCookiesList;
        }

        private static List<HarHeader>? GetHeaders(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;

            var harHeadersList = new List<HarHeader>();
            var propertyArray = propertyElement.Value.EnumerateArray();
            foreach (var propertyArrayElement in propertyArray)
            {
                var harHeaders = new HarHeader();
                harHeaders.Name = GetString(propertyArrayElement, "name");
                harHeaders.Value = GetString(propertyArrayElement, "value");
                harHeadersList.Add(harHeaders);
            }

            return harHeadersList;
        }

        private static List<HarQueryPair>? GetQueryPairs(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;

            var harQueryPairList = new List<HarQueryPair>();
            var propertyArray = propertyElement.Value.EnumerateArray();
            foreach (var propertyArrayElement in propertyArray)
            {
                var harQueryPair = new HarQueryPair();
                harQueryPair.Name = GetString(propertyArrayElement, "name");
                harQueryPair.Value = GetString(propertyArrayElement, "value");
                harQueryPairList.Add(harQueryPair);
            }

            return harQueryPairList;
        }

        private static HarText? ParseText(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;

            var harText = new HarText();
            harText.MimeType = GetString(propertyElement.Value, "mimeType");
            harText.Size = GetInt(propertyElement.Value, "size");
            harText.Encoding = GetString(propertyElement.Value, "encoding");
            harText.Text = GetString(propertyElement.Value, "text");
            return harText;
        }

        private static int? GetInt(string? value)
        {
            int parsed;
            if (int.TryParse(value, out parsed))
                return parsed;
            return null;
        }

        private static bool? GetBool(string? value)
        {
            bool parsed;
            if (bool.TryParse(value, out parsed))
                return parsed;
            return null;
        }

        private static JsonElement? GetProperty(JsonElement element, string property)
        {
            JsonElement propertyElement;
            if (element.TryGetProperty(property, out propertyElement))
                return propertyElement;
            return null;
        }

        private static bool? GetBool(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;
            var propertyBool = propertyElement.Value.GetBoolean();
            return propertyBool;
        }

        private static double? GetDouble(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;
            var propertyDouble = propertyElement.Value.GetDouble();
            return propertyDouble;
        }

        private static int? GetInt(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;
            var propertyInt = propertyElement.Value.GetInt32();
            return propertyInt;
        }

        private static string? GetString(JsonElement element, string property)
        {
            var propertyElement = GetProperty(element, property);
            if (propertyElement == null)
                return null;
            var propertyString = propertyElement.Value.GetString();
            return propertyString;
        }
    }
}
