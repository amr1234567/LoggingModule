using LoggingModule.Models.LogModels;
using LoggingModule.Models.DTOs;

namespace LoggingModule.Services.LogsInterfaces
{
    public interface ILogServices
    {
        Task<Result<List<HttpRequestLogDto>>> GetLogs(PaginationRequest pagination, DateFilteration dateFilteration);
        Task<Result<HttpRequestLogDto>> GetLogById(int logId);
        Task<Result<bool>> DeleteLog(int logId);
        Task<Result<List<HttpRequestLogDto>>> GetExceptionLogs(PaginationRequest pagination, DateFilteration dateFilteration);
    }
}
