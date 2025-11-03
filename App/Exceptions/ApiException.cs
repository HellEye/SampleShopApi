// ApiException.cs

using System.Net;
namespace App.Exceptions;

public sealed class ApiException : Exception {
	public int StatusCode { get; }
	public string? Title { get; }

	public ApiException(int statusCode, string message, string? title = null)
		: base(message) {
		StatusCode = statusCode;
		Title = title;
	}

	public static ApiException BadRequest(string message) =>
		new(StatusCodes.Status400BadRequest, message, "Bad Request");

	public static ApiException NotFound(string message) =>
		new(StatusCodes.Status404NotFound, message, "Not Found");

	public static ApiException Unprocessable(string message) =>
		new(StatusCodes.Status422UnprocessableEntity, message, "Unprocessable Entity");
}
