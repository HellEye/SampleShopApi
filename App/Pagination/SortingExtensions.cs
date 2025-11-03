using System.Linq.Expressions;
using System.Reflection;
using App.Exceptions;

namespace App.Pagination;

public enum Order {
	Asc,
	Desc
}
public interface ISortParams {
	string? Sort { get; set; }
	Order? Order { get; set; }
}

public class SortableAttribute : Attribute {
	public string FieldName { get; }

	public SortableAttribute(string fieldName) {
		FieldName = fieldName;
	}

}

public static class SortingExtensions {
	private static readonly MethodInfo OrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);
	private static readonly MethodInfo OrderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

	public static IQueryable<T> WithSorting<T>(this IQueryable<T> source, ISortParams query) where T : class {
		if (query == null || string.IsNullOrEmpty(query.Sort)) {
			return source;
		}

		var entityType = typeof(T);

		// Find the property with the Sortable attribute matching the sort field
		var property = entityType.GetProperties()
			.Where(prop => {
				if (!Attribute.IsDefined(prop, typeof(SortableAttribute))) return false;
				var attr = prop.GetCustomAttributes(typeof(SortableAttribute), false)
				.Cast<SortableAttribute>()
				.First();
				return attr.FieldName.Equals(query.Sort, StringComparison.OrdinalIgnoreCase);
			})
			.FirstOrDefault();

		if (property == null) {
			throw ApiException.BadRequest($"Cannot sort by field '{query.Sort}'.");
		}

		var paramterExpression = Expression.Parameter(typeof(T));
		var orderByProperty = Expression.Property(paramterExpression, property.Name);
		var lambda = Expression.Lambda(orderByProperty, paramterExpression);
		var genericMethod =
		  query.Order == Order.Desc ?
			OrderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type)
		  : OrderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
		var ret = genericMethod.Invoke(null, [source, lambda]);
		return (IQueryable<T>)ret!;
	}
}
