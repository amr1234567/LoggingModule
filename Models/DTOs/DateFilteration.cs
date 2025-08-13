namespace LoggingModule.Models.DTOs;

public record DateFilteration
{
  public DateTime? FromDate { get; set; } = DateTime.UtcNow;
  public DateTime? ToDate { get; set; } = DateTime.UtcNow;
}