using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

public interface ISearchParams {
	string? Search { get; set; }
}
public static class SearchExtensions {
	private static readonly MethodInfo ILikeMethod =
		typeof(NpgsqlDbFunctionsExtensions)
		  .GetMethod(
			nameof(NpgsqlDbFunctionsExtensions.ILike),
			[typeof(DbFunctions), typeof(string), typeof(string)]
		  )!;
	private static readonly MemberExpression efFunctionsExpr =
		Expression.Property(null, typeof(EF), nameof(EF.Functions));
	public static IQueryable<T> WithSearch<T>(this IQueryable<T> source, ISearchParams query, params Expression<Func<T, string>>[] fields) {
		if (string.IsNullOrWhiteSpace(query.Search)) {
			return source;
		}
		var pattern = $"%{query.Search.Replace("%", "\\%").Replace("_", "\\_")}%";
		var parameter = Expression.Parameter(typeof(T));
		Expression? body = null;

		foreach (var field in fields) {
			var replacedBody = ReplaceParameter(field.Body, field.Parameters[0], parameter);
			var notNull =
				Expression.NotEqual(replacedBody, Expression.Constant(null, typeof(string)));
			var likeCall = Expression.Call(
				ILikeMethod,
				efFunctionsExpr,
				replacedBody,
				Expression.Constant(pattern)
			);
			var fieldPredicate = Expression.AndAlso(notNull, likeCall);
			body = body is null ? fieldPredicate : Expression.OrElse(body, fieldPredicate);
		}

		var lambda = Expression.Lambda<Func<T, bool>>(body!, parameter);
		return source.Where(lambda);
	}


	private static Expression ReplaceParameter(
		Expression expression,
		ParameterExpression from,
		ParameterExpression to
	) => new ReplaceParameterVisitor(from, to).Visit(expression)!;

	private sealed class ReplaceParameterVisitor : ExpressionVisitor {
		private readonly ParameterExpression _from;
		private readonly ParameterExpression _to;

		public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to) {
			_from = from; _to = to;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		  => node == _from ? _to : base.VisitParameter(node);
	}

}
