using Microsoft.EntityFrameworkCore;

namespace App.Pagination;

public interface IPaginatedQuery {
	int? Page { get; set; }
	int? PageSize { get; set; }
}

public record class PaginatedResponse<TDto> {
	public required IEnumerable<TDto> Items { get; set; }
	public required int TotalCount { get; set; }
	public required int Page { get; set; }
	public required int PageSize { get; set; }
	public required int TotalPages { get; set; }
	public required int TotalItems { get; set; }
}

public static class PaginationExtensions {
	public static async Task<PaginatedResponse<TDto>> ToPaginated<TDto>(this IQueryable<ToDto<TDto>> source, IPaginatedQuery query) {
		var page = query.Page ?? 1;
		var pageSize = query.PageSize ?? 10;

		var count = await source.CountAsync();
		var items = await source
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(item => item.AsDto())
			.ToListAsync();


		return new() {
			Items = items,
			TotalCount = count,
			Page = page,
			PageSize = pageSize,
			TotalPages = (int)Math.Ceiling(count / (float)pageSize),
			TotalItems = count
		};
	}
}
