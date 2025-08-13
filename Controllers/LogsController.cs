using LoggingModule.Services.LogsInterfaces;
using LoggingModule.Models.DTOs;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace LoggingModule.Controllers
{
    public class LogsController : Controller
    {
        private readonly ILogServices _logServices;

        public LogsController(ILogServices logServices)
        {
            _logServices = logServices;
        }

        // GET: Logs (list logs with optional date filter and pagination)
        public async Task<IActionResult> Index()
        {
            const int pageSize = 10;  // Number of log entries per page
            //var allLogsResult = await _logServices.GetLogs(new() { PageNumber = 1, PageSize = pageSize }, new() { FromDate = DateTime.UtcNow, ToDate = DateTime.UtcNow });
            ViewBag.ActiveMenuTab = "Requests";

            // Prepare view model with logs and pagination info
            var viewModel = new LogIndexViewModel();
            //{
            //  Logs = allLogsResult.Data,
            //FromDate = DateTime.UtcNow,
            //ToDate = DateTime.UtcNow,
            //Pagination = allLogsResult.Pagination
            //};

            return View(viewModel);
        }

        public async Task<IActionResult> Exceptions()
        {
            const int pageSize = 10;  // Number of log entries per page
            //var allLogsResult = await _logServices.GetLogs(new() { PageNumber = 1, PageSize = pageSize }, new() { FromDate = DateTime.UtcNow, ToDate = DateTime.UtcNow });
            ViewBag.ActiveMenuTab = "Exceptions";

            // Prepare view model with logs and pagination info
            var viewModel = new LogIndexViewModel();
            //{
            //  Logs = allLogsResult.Data,
            //FromDate = DateTime.UtcNow,
            //ToDate = DateTime.UtcNow,
            //Pagination = allLogsResult.Pagination
            //};

            return View(viewModel);
        }

        //[HttpGet]
        //public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int page = 1, int size = 10)
        //{
        //    var allLogsResult = await _logServices.GetLogs(new() { PageNumber = page, PageSize = size }, new() { FromDate = fromDate, ToDate = toDate });

        //    // Prepare view model with logs and pagination info
        //    var viewModel = new LogIndexViewModel
        //    {
        //        Logs = allLogsResult.Data,
        //        FromDate = fromDate,
        //        ToDate = toDate,
        //        Pagination = allLogsResult.Pagination
        //    };

        //    return View(viewModel);
        //}

        // GET: Logs/Details/5  (view full details of a single log entry)

        public async Task<IActionResult> Details([FromRoute] int id)
        {
            ViewBag.ActiveTab = "Payload";
            var logResult = await _logServices.GetLogById(id);
            if (logResult == null)
            {
                return NotFound();
            }
            return View(logResult.Data);
        }

        // POST: Logs/Delete/5  (delete a log entry)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _logServices.DeleteLog(id);
            // Optionally, add success/failure feedback via TempData
            return RedirectToAction(nameof(Index));
        }

        // طريقة جديدة لجلب السجلات بتنسيق JSON
        [HttpGet]
        public async Task<JsonResult> GetLogsJson(DateTime? fromDate, DateTime? toDate, int page = 1, int size = 10)
        {
            var allLogsResult = await _logServices.GetLogs(
                new() { PageNumber = page, PageSize = size },
                new() { FromDate = fromDate, ToDate = toDate }
            );

            return Json(new
            {
                data = allLogsResult.Data,
                pagination = new
                {
                    currentPage = allLogsResult.Pagination.CurrentPage,
                    pageSize = allLogsResult.Pagination.PageSize,
                    totalItems = allLogsResult.Pagination.TotalItems,
                    totalPages = allLogsResult.Pagination.TotalPages,
                    hasPreviousPage = allLogsResult.Pagination.HasPreviousPage,
                    hasNextPage = allLogsResult.Pagination.HasNextPage
                }
            });
        }

        // طريقة جديدة لجلب السجلات بتنسيق JSON
        [HttpGet]
        public async Task<JsonResult> GetExceptionLogsJson(DateTime? fromDate, DateTime? toDate, int page = 1, int size = 10)
        {
            var allLogsResult = await _logServices.GetExceptionLogs(
                new() { PageNumber = page, PageSize = size },
                new() { FromDate = fromDate, ToDate = toDate }
            );

            return Json(new
            {
                data = allLogsResult.Data,
                pagination = new
                {
                    currentPage = allLogsResult.Pagination.CurrentPage,
                    pageSize = allLogsResult.Pagination.PageSize,
                    totalItems = allLogsResult.Pagination.TotalItems,
                    totalPages = allLogsResult.Pagination.TotalPages,
                    hasPreviousPage = allLogsResult.Pagination.HasPreviousPage,
                    hasNextPage = allLogsResult.Pagination.HasNextPage
                }
            });
        }
    }

    public class LogIndexViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public PaginationDetails Pagination { get; set; }

        public IEnumerable<HttpRequestLogDto> Logs { get; set; }

    }
}
