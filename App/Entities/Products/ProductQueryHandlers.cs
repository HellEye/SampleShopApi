
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Products;

public record class ProductListQuery : IPaginatedQuery, ISortParams, ISearchParams {
	[FromQuery(Name = "page")]
	[DefaultValue(1)]
	[Range(1, int.MaxValue)]
	[Description("Page number")]
	public int? Page { get; set; } = 1;

	[FromQuery(Name = "pageSize")]
	[DefaultValue(10)]
	[Range(1, int.MaxValue)]
	[Description("Number of items per page")]
	public int? PageSize { get; set; } = 10;

	[FromQuery(Name = "search")]
	[Description("Search term for product name")]
	public string? Search { get; set; }

	[FromQuery(Name = "sort")]
	[Description("Field to sort by")]
	[EnumDataType(typeof(ProductSortField))]
	public string? Sort { get; set; }

	[FromQuery(Name = "order")]
	[Description("Sort order")]
	[EnumDataType(typeof(Order))]
	public Order? Order { get; set; } = Queries.Order.Asc;
}

public class GetProductListHandler(ShopApiContext db) {
	public async Task<PaginatedResponse<ProductDto>> HandleAsync(ProductListQuery query) {
		var res = await db.Products
		  .AsNoTracking()
		  .WithSearch(query,
			(p) => p.Name,
			p => p.Description
		  )
		  .WithSorting(query)
		  .ToPaginated(query);

		return res;
	}

}

public class GetProductByIdHandler(ShopApiContext db) {
	public async Task<ProductDto?> HandleAsync(int id) {
		var product = await db.Products
		  .AsNoTracking()
		  .FirstOrDefaultAsync(p => p.Id == id);

		return product?.AsDto();
	}
}
