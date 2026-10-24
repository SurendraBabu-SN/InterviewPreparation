using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace WebAPIPreparation.Utility
{
    /// <summary>Turns exceptions into clean ProblemDetails JSON with the right HTTP status code.</summary>
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetails)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            var (status, title) = exception switch
            {
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
                // 2627 / 2601 = SQL Server unique or primary key violation (a race between two requests)
                DbUpdateException { InnerException: SqlException { Number: 2627 or 2601 } }
                    => (StatusCodes.Status409Conflict, "A record with the same key already exists."),
                _ => (StatusCodes.Status500InternalServerError, "Something went wrong. Please try again.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled error on {Path}", context.Request.Path);

            context.Response.StatusCode = status;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = new ProblemDetails { Status = status, Title = title }
            });
        }
    }
}