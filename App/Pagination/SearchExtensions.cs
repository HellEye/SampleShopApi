public interface ISearchParams {
	string? Search { get; set; }
}
public static class SearchExtensions {

	public static IQueryable<T> WithSearch<T>(this IQueryable<T> source, ISearchParams query, Func<T, string[]> nameSelectors) {
		if (string.IsNullOrWhiteSpace(query.Search)) {
			return source;
		}

		return source.Where(item => nameSelectors(item).Any(name => name.Contains(query.Search, StringComparison.InvariantCultureIgnoreCase)));
	}
}
