using System.Net;

namespace SampleShopApi.App.Utils;

public class Result<T> {
	public bool IsSuccess { get; private set; }
	public T? Value { get; private set; }
	public string? Title { get; private set; }
	public Dictionary<string, string[]>? Errors { get; private set; }
	public HttpStatusCode StatusCode { get; private set; }

	public static Result<T> Success(T? value, HttpStatusCode statusCode = HttpStatusCode.OK) => new() {
		IsSuccess = true,
		Value = value,
		StatusCode = statusCode
	};

	public static Result<T> Success(HttpStatusCode statusCode = HttpStatusCode.OK) => new() {
		IsSuccess = true,
		StatusCode = statusCode
	};

	public static Result<T> Failure(
		string title,
		HttpStatusCode statusCode = HttpStatusCode.BadRequest,
		Dictionary<string, string[]>? errors = null) =>
		new() {
			IsSuccess = false,
			Title = title,
			Errors = errors,
			StatusCode = statusCode
		};

	public Result<T> WithStatusCode(HttpStatusCode statusCode) {
		StatusCode = statusCode;
		return this;
	}

	public IResult ToHttpResult() => ToHttpResult<T>(null);
	public IResult ToHttpResult<U>(Func<T, U>? mapResult) {
		if (IsSuccess) {
			return Results.Json(mapResult is not null ? mapResult(Value!) : Value, statusCode: (int)StatusCode);
		}

		var problem = new HttpValidationProblemDetails(Errors ?? []) {
			Title = Title,
			Status = (int)StatusCode,
			Type = "https://example.com/probs/general-error"
		};

		return Results.Json(problem, statusCode: (int)StatusCode);
	}

	// Convenience helpers
	public static Result<T> BadRequest(
		string title,
		Dictionary<string, string[]>? errors = null) =>
		Failure(title, HttpStatusCode.BadRequest, errors);

	public static Result<T> NotFound(
		string title,
		Dictionary<string, string[]>? errors = null) =>
		Failure(title, HttpStatusCode.NotFound, errors);

	public static Result<T> NoContent() => Success(HttpStatusCode.NoContent);
}
