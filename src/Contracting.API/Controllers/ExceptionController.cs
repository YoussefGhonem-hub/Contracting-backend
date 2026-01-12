using Contracting.Application.Common;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos.HelperDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contracting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExceptionController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ExceptionController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get the latest exceptions with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of exceptions</returns>
        [HttpGet]
        [Route("latest")]
        public async Task<IActionResult> GetLatestExceptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            var totalCount = await _db.ExceptionLogs.CountAsync();

            var exceptions = await _db.ExceptionLogs
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ExceptionLogDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    StackTrace = x.StackTrace,
                    InnerExceptionMessage = x.InnerExceptionMessage,
                    InnerExceptionStackTrace = x.InnerExceptionStackTrace,
                    ExceptionType = x.ExceptionType,
                    HttpMethod = x.HttpMethod,
                    RequestPath = x.RequestPath,
                    QueryString = x.QueryString,
                    UserId = x.UserId,
                    UserAgent = x.UserAgent,
                    IpAddress = x.IpAddress,
                    StatusCode = x.StatusCode,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            var result = new PagedResult<ExceptionLogDto>
            {
                Items = exceptions,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        /// <summary>
        /// Get exception by ID
        /// </summary>
        /// <param name="id">Exception log ID</param>
        /// <returns>Exception details</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetExceptionById(Guid id)
        {
            var exception = await _db.ExceptionLogs
                .Where(x => x.Id == id)
                .Select(x => new ExceptionLogDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    StackTrace = x.StackTrace,
                    InnerExceptionMessage = x.InnerExceptionMessage,
                    InnerExceptionStackTrace = x.InnerExceptionStackTrace,
                    ExceptionType = x.ExceptionType,
                    HttpMethod = x.HttpMethod,
                    RequestPath = x.RequestPath,
                    QueryString = x.QueryString,
                    UserId = x.UserId,
                    UserAgent = x.UserAgent,
                    IpAddress = x.IpAddress,
                    StatusCode = x.StatusCode,
                    CreatedDate = x.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (exception == null)
                return NotFound("Exception not found");

            return Ok(exception);
        }

        /// <summary>
        /// Get exceptions by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of exceptions in the date range</returns>
        [HttpGet]
        [Route("by-date-range")]
        public async Task<IActionResult> GetExceptionsByDateRange([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            if (startDate > endDate)
                return BadRequest("Start date must be before end date");

            var totalCount = await _db.ExceptionLogs
                .Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate)
                .CountAsync();

            var exceptions = await _db.ExceptionLogs
                .Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ExceptionLogDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    StackTrace = x.StackTrace,
                    InnerExceptionMessage = x.InnerExceptionMessage,
                    InnerExceptionStackTrace = x.InnerExceptionStackTrace,
                    ExceptionType = x.ExceptionType,
                    HttpMethod = x.HttpMethod,
                    RequestPath = x.RequestPath,
                    QueryString = x.QueryString,
                    UserId = x.UserId,
                    UserAgent = x.UserAgent,
                    IpAddress = x.IpAddress,
                    StatusCode = x.StatusCode,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            var result = new PagedResult<ExceptionLogDto>
            {
                Items = exceptions,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        /// <summary>
        /// Get exceptions by type
        /// </summary>
        /// <param name="exceptionType">Exception type name</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of exceptions by type</returns>
        [HttpGet]
        [Route("by-type")]
        public async Task<IActionResult> GetExceptionsByType([FromQuery] string exceptionType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(exceptionType))
                return BadRequest("Exception type is required");

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            var totalCount = await _db.ExceptionLogs
                .Where(x => x.ExceptionType == exceptionType)
                .CountAsync();

            var exceptions = await _db.ExceptionLogs
                .Where(x => x.ExceptionType == exceptionType)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ExceptionLogDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    StackTrace = x.StackTrace,
                    InnerExceptionMessage = x.InnerExceptionMessage,
                    InnerExceptionStackTrace = x.InnerExceptionStackTrace,
                    ExceptionType = x.ExceptionType,
                    HttpMethod = x.HttpMethod,
                    RequestPath = x.RequestPath,
                    QueryString = x.QueryString,
                    UserId = x.UserId,
                    UserAgent = x.UserAgent,
                    IpAddress = x.IpAddress,
                    StatusCode = x.StatusCode,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            var result = new PagedResult<ExceptionLogDto>
            {
                Items = exceptions,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }
    }
}
