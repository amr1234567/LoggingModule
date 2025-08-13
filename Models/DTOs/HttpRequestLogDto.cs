using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text;
using System.Web;

namespace LoggingModule.Models.DTOs
{
    public class HttpRequestLogDto
    {
        public HttpRequestLogDto(HttpRequestLog log)
        {
            Id = log.Id;
            Timestamp = log.Timestamp;
            Elapsed = log.Elapsed;
            Method = log.Method;
            ExceptionDetails = log.ExceptionDetails;
            Path = log.Path;
            QueryString = log.QueryString;
            RequestHeaders = log.RequestHeaders;
            RequestBody = log.RequestBody;
            ResponseStatusCode = log.ResponseStatusCode;
            ResponseHeaders = log.ResponseHeaders;
            ResponseBody = log.ResponseBody;
            Hostname = log.Hostname;
            IPAddress = log.IpAddress;
            ControllerAction = log.ControllerAction;
            MemoryUsage = log.MemoryUsage.Value;
            UserAgent = log.UserAgent;
        }

        public HttpRequestLogDto()
        {

        }
        public int Id { get; set; }
        public DateTime? Timestamp { get; set; }
        public double? Elapsed { get; set; }
        public string? Method { get; set; }
        public string? ExceptionDetails { get; set; }
        public string? Path { get; set; }
        public string? QueryString { get; set; }
        public string? RequestHeaders { get; set; }
        public string? RequestBody { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? ResponseHeaders { get; set; }
        public string? ResponseBody { get; set; }
        public string? Hostname { get; set; }
        public string? ControllerAction { get; set; }
        public string? IPAddress { get; set; }
        public double? MemoryUsage { get; set; }
        public string? UserAgent { get; set; }

        public string GetFormattedPayload()
        {
            return FormatJson(RequestBody);
        }

        public string GetFormattedHeaders()
        {
            return FormatJson(RequestHeaders);
        }

        public string GetFormattedResponse()
        {
            return FormatJson(ResponseBody);
        }

        private string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "{}";
            }

            try
            {
                var obj = JToken.Parse(json);
                var formatted = obj.ToString(Formatting.Indented);

                // Escape HTML to prevent XSS
                var escaped = HttpUtility.HtmlEncode(formatted);

                // Process the JSON line by line to add property containers
                var lines = escaped.Split('\n');
                var result = new StringBuilder();
                var propertyId = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var trimmedLine = line.TrimStart();

                    // Check if this line contains a property key
                    var isProperty = trimmedLine.Contains("&quot;") && trimmedLine.Contains(":");

                    if (isProperty)
                    {
                        // This is a key-value pair, wrap it in a hoverable container
                        propertyId++;

                        // Split the line at the first colon to separate key and value
                        var colonIndex = line.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            var keyPart = line.Substring(0, colonIndex + 1); // Include the colon
                            var valuePart = line.Substring(colonIndex + 1);

                            // Replace &quot; with actual quote character for display
                            // But wrap in spans for styling
                            keyPart = keyPart.Replace("&quot;", "<span class=\"json-quote\">\"</span>");

                            // Apply syntax highlighting to the key
                            keyPart = System.Text.RegularExpressions.Regex.Replace(
                                keyPart,
                                "<span class=\"json-quote\">\"</span>(.*?)<span class=\"json-quote\">\"</span>",
                                "<span class=\"json-key\"><span class=\"json-quote\">\"</span>$1<span class=\"json-quote\">\"</span></span>"
                            );

                            // Apply syntax highlighting to the value based on its type
                            if (valuePart.Contains("&quot;"))
                            {
                                // String value - replace &quot; with actual quote character
                                valuePart = valuePart.Replace("&quot;", "<span class=\"json-quote\">\"</span>");

                                // Wrap the string value (including quotes) in a span
                                valuePart = System.Text.RegularExpressions.Regex.Replace(
                                    valuePart,
                                    "<span class=\"json-quote\">\"</span>(.*?)<span class=\"json-quote\">\"</span>",
                                    "<span class=\"json-string\"><span class=\"json-quote\">\"</span>$1<span class=\"json-quote\">\"</span></span>"
                                );
                            }
                            else if (System.Text.RegularExpressions.Regex.IsMatch(valuePart, @"\s+\d+"))
                            {
                                // Number value
                                valuePart = System.Text.RegularExpressions.Regex.Replace(
                                    valuePart,
                                    @"(\s+\d+)",
                                    "<span class=\"json-number\">$1</span>"
                                );
                            }
                            else if (valuePart.Contains("true") || valuePart.Contains("false"))
                            {
                                // Boolean value
                                valuePart = System.Text.RegularExpressions.Regex.Replace(
                                    valuePart,
                                    "(true|false)",
                                    "<span class=\"json-boolean\">$1</span>"
                                );
                            }
                            else if (valuePart.Contains("null"))
                            {
                                // Null value
                                valuePart = System.Text.RegularExpressions.Regex.Replace(
                                    valuePart,
                                    "(null)",
                                    "<span class=\"json-null\">$1</span>"
                                );
                            }

                            // Wrap the entire property in a hoverable container
                            result.AppendLine($"<div class=\"json-property\" data-property-id=\"{propertyId}\">{keyPart}{valuePart}</div>");
                        }
                        else
                        {
                            // Fallback if we can't find the colon
                            result.AppendLine(line);
                        }
                    }
                    else
                    {
                        // This is not a property (e.g., brackets, commas), output as is
                        result.AppendLine(line);
                    }
                }

                return result.ToString();
            }
            catch
            {
                return HttpUtility.HtmlEncode(json);
            }
        }
    }
}
