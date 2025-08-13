using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LoggingModule.Models.LogModels;

public class HttpRequestLog
{
    public HttpRequestLog()
    {
    }

    [Key]
    public int Id { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Message { get; set; }
    public string? Level { get; set; }
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
    public string? IpAddress { get; set; }
    public double? MemoryUsage { get; set; }
    public string? UserAgent { get; set; }
}

