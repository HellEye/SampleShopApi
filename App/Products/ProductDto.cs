using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using App.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace App.Products;

public record class ProductDto {
	public int Id { get; set; }
	public string Name { get; set; }
	public decimal Price { get; set; }
	public string Description { get; set; }


}

public enum ProductSortField {
	name,
	price
}
public record class ProductListQueryDto : IPaginatedQuery, ISortParams, ISearchParams {
	[FromQuery(Name = "page")]
	[Range(1, int.MaxValue)]
	[Description("Page number")]
	public int? Page { get; set; } = 1;

	[FromQuery(Name = "pageSize")]
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
	public Order? Order { get; set; } = Pagination.Order.Asc;
}

public record class ProductCreateDto {
	[DefaultValue("New Product")]
	public required string Name { get; set; }

	[DefaultValue(typeof(decimal), "9.99")]
	public required decimal Price { get; set; }

	[DefaultValue("A brand new product")]
	public string Description { get; set; } = string.Empty;
}
