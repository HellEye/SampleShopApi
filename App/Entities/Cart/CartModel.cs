using App.Utils;
using SampleShopApi.App.Entities.Albums;

namespace SampleShopApi.App.Entities.Cart;

public class Cart : ToDto<CartDto> {
	public int Id { get; set; }
	public required List<CartItem> Items { get; set; } = [];
	public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public CartDto AsDto() => new() {
		Id = Id,
		Items = [.. Items.Select(item => item.AsDto())],
	};
}

public class CartItem : ToDto<CartItemDto> {
	public int Id { get; set; }
	public required int Quantity { get; set; }

	public required int AlbumId { get; set; }
	public Album Album { get; set; } = null!;

	public required int CartId { get; set; }
	public Cart Cart { get; set; } = null!;

	public CartItemDto AsDto() => new() {
		Album = Album!.AsDto()!,
		Quantity = Quantity
	};

	public override string ToString() =>
	  $"CartItem(Id={Id}, AlbumId={AlbumId}, Quantity={Quantity})";
}
