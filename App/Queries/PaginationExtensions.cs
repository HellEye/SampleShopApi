using System.Diagnostics.CodeAnalysis;
using App.Utils;
using Microsoft.EntityFrameworkCore;

namespace SampleShopApi.App.Queries;

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

	[SetsRequiredMembers]
	public PaginatedResponse(IEnumerable<TDto> items, int totalCount, IPaginatedQuery query) {
		Items = items;
		TotalCount = totalCount;
		Page = query.Page ?? 1;
		PageSize = query.PageSize ?? 10;
		TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
		TotalItems = totalCount;
	}
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


		return new(items, count, query);
	}
}
