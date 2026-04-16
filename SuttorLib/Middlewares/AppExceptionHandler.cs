using Microsoft.AspNetCore.Diagnostics;
using SuttorLibrary.Helpers;
using System.Net;
using System.Text.Json;

namespace SuttorLibrary.Middlewares
{
    public class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, exception.Message);
            var res = httpContext.Response;
            res.ContentType = "application/json";
            ErrorResponse errorResponse = new ErrorResponse();
            switch (exception)
            {
                case System.ComponentModel.DataAnnotations.ValidationException ve:
                    res.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = ve.Message;
                    errorResponse.ErrorMessage = ve.ValidationResult?.ErrorMessage ?? string.Empty;
                    break;

                case ArgumentNullException ane:
                case ArgumentOutOfRangeException aoore:
                case ArgumentException ae:
                    res.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;

                case UnauthorizedAccessException uae:
                    res.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = uae.Message;
                    break;

                case System.Security.SecurityException se:
                    res.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = se.Message;
                    break;

                case KeyNotFoundException knf:
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = knf.Message;
                    break;

                case InvalidOperationException ioe:
                    // Conflict often indicates a logical resource state problem
                    res.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = ioe.Message;
                    break;

                case NotImplementedException nie:
                    res.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = nie.Message;
                    break;

                case NotSupportedException nse:
                    res.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = nse.Message;
                    break;

                case System.Data.Common.DbException dbEx:
                case Microsoft.EntityFrameworkCore.DbUpdateException dbuEx:
                    // Database related errors -> 500
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = "A database error occurred.";
                    errorResponse.ErrorMessage = exception.Message;
                    break;

                case FormatException fe:
                    res.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = fe.Message;
                    break;

                case TimeoutException te:
                    res.StatusCode = 504; // Gateway Timeout
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = te.Message;
                    break;

                case OperationCanceledException oce:
                    res.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = oce.Message;
                    break;

                default:
                    // Unknown exceptions -> Internal Server Error
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.StatusCode = res.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
            }
            errorResponse.Timestamp = DateTime.UtcNow;
            errorResponse.Path = httpContext.Request.Path;

            var result = JsonSerializer.Serialize(
                errorResponse,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
