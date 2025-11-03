// GlobalExceptionHandler.cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace App.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler {
	private readonly IProblemDetailsService _problemDetails;

	public GlobalExceptionHandler(IProblemDetailsService problemDetails) {
		_problemDetails = problemDetails;
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken) {
		var (status, title, detail) = exception switch {
			ApiException api => (api.StatusCode, api.Title ?? "Error", api.Message),
			ArgumentException arg => (StatusCodes.Status400BadRequest, "Bad Request", arg.Message),
			KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
			_ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
		};

		httpContext.Response.StatusCode = status;

		var problem = new ProblemDetails {
			Status = status,
			Title = title,
			Detail = detail,
			Instance = httpContext.Request.Path
		};

		// This writes RFC7807 JSON and can include traceId if configured
		var written = await _problemDetails.TryWriteAsync(new ProblemDetailsContext {
			HttpContext = httpContext,
			ProblemDetails = problem,
			Exception = exception
		});

		return written || true;
	}
}
