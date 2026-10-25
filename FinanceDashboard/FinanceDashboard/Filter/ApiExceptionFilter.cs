using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceDashboard.Filters
{
    /// <summary>
    /// Turns exceptions thrown inside API controllers into JSON error responses (RFC 7807 "problem details")
    /// instead of an HTML error page. Registered in Program.cs for controllers only; Blazor pages are not affected.
    ///
    ///   ValidationException  -> 400 with the broken rule
    ///   everything else      -> 500 (details are logged; only shown in the response in Development)
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IHostEnvironment _environment;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            if (exception is ValidationException validationException)
            {
                var member = validationException.ValidationResult.MemberNames.FirstOrDefault() ?? string.Empty;

                var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [member] = new[] { validationException.Message }
                })
                {
                    Status = StatusCodes.Status400BadRequest
                };

                context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception in {Action}", context.ActionDescriptor.DisplayName);

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = _environment.IsDevelopment()
                        ? exception.Message
                        : "The request could not be processed."
                };

                context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
            }

            context.ExceptionHandled = true;
        }
    }
}
