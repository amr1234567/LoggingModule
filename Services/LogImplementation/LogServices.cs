using LoggingModule.Helpers;
using LoggingModule.Services.LogsInterfaces;
using LoggingModule.Models.DTOs;

using Microsoft.EntityFrameworkCore;

namespace LoggingModule.Services.LogImplementation;

public class LogServices : ILogServices
{
    private readonly ApplicationDbContext _context;

    public LogServices(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<List<HttpRequestLogDto>>> GetLogs(PaginationRequest pagination, DateFilteration dateFilteration)
    {
        var logsQuery = _context.HttpRequestLogs
            //.Where(l => !l.Timestamp.HasValue || ((!dateFilteration.FromDate.HasValue || dateFilteration.FromDate.Value.Date <= l.Timestamp.Value.Date)
            //        && (!dateFilteration.ToDate.HasValue || dateFilteration.ToDate.Value.Date >= l.Timestamp.Value.Date)))
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new HttpRequestLogDto
            {
                Id = l.Id,
                Method = l.Method,
                Path = l.Path,
                QueryString = l.QueryString,
                RequestBody = l.RequestBody,
                RequestHeaders = l.RequestHeaders,
                ResponseStatusCode = l.ResponseStatusCode,
                Elapsed = l.Elapsed,
                Timestamp = l.Timestamp,
                ExceptionDetails = l.ExceptionDetails,
                ResponseBody = l.ResponseBody,
                ResponseHeaders = l.ResponseHeaders,
                ControllerAction = l.ControllerAction,
                Hostname = l.Hostname,
                IPAddress = l.IpAddress,
                MemoryUsage = l.MemoryUsage ?? 0,
                UserAgent = l.UserAgent
            });
        var logs = await logsQuery
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var count = await logsQuery.CountAsync();
        var paginationDetails = new PaginationDetails()
        {
            CurrentPage = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = count
        };
        return Result<List<HttpRequestLogDto>>.Success(logs, pagination: paginationDetails);
    }

    public async Task<Result<List<HttpRequestLogDto>>> GetExceptionLogs(PaginationRequest pagination, DateFilteration dateFilteration)
    {
        var logsQuery = _context.HttpRequestLogs
            //.Where(l => !l.Timestamp.HasValue || ((!dateFilteration.FromDate.HasValue || dateFilteration.FromDate.Value.Date <= l.Timestamp.Value.Date)
            //        && (!dateFilteration.ToDate.HasValue || dateFilteration.ToDate.Value.Date >= l.Timestamp.Value.Date)))
            .Where(l => !string.IsNullOrEmpty(l.ExceptionDetails))
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new HttpRequestLogDto
            {
                Id = l.Id,
                Method = l.Method,
                Path = l.Path,
                QueryString = l.QueryString,
                RequestBody = l.RequestBody,
                RequestHeaders = l.RequestHeaders,
                ResponseStatusCode = l.ResponseStatusCode,
                Elapsed = l.Elapsed,
                Timestamp = l.Timestamp,
                ExceptionDetails = l.ExceptionDetails,
                ResponseBody = l.ResponseBody,
                ResponseHeaders = l.ResponseHeaders,
                ControllerAction = l.ControllerAction,
                Hostname = l.Hostname,
                IPAddress = l.IpAddress,
                MemoryUsage = l.MemoryUsage ?? 0,
                UserAgent = l.UserAgent
            });
        var logs = await logsQuery
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var count = await logsQuery.CountAsync();
        var paginationDetails = new PaginationDetails()
        {
            CurrentPage = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = count
        };
        return Result<List<HttpRequestLogDto>>.Success(logs, pagination: paginationDetails);
    }


    public async Task<Result<HttpRequestLogDto>> GetLogById(int logId)
    {
        var log = await _context.HttpRequestLogs
            .Where(l => l.Id == logId)
            .Select(l => new HttpRequestLogDto
            {
                Id = l.Id,
                Method = l.Method,
                Path = l.Path,
                QueryString = l.QueryString,
                RequestBody = l.RequestBody,
                RequestHeaders = l.RequestHeaders,
                ResponseStatusCode = l.ResponseStatusCode,
                Elapsed = l.Elapsed,
                Timestamp = l.Timestamp,
                ExceptionDetails = l.ExceptionDetails,
                ResponseBody = l.ResponseBody,
                ResponseHeaders = l.ResponseHeaders,
                ControllerAction = l.ControllerAction,
                Hostname = l.Hostname,
                IPAddress = l.IpAddress,
                MemoryUsage = l.MemoryUsage ?? 0,
                UserAgent = l.UserAgent
            })
            .FirstOrDefaultAsync();
        return Result<HttpRequestLogDto>.Success(log);
    }

    public async Task<Result<bool>> DeleteLog(int logId)
    {
        var result = await _context.HttpRequestLogs.Where(x => x.Id == logId).ExecuteDeleteAsync();
        return result > 0 ?
            Result<bool>.Success(result > 0) :
            Result<bool>.Fail("Log not found", 404);
    }
}
