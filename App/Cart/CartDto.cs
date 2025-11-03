using App.Products;
namespace App.Cart;

public class CartDto {
	public required int Id { get; set; }
	public required List<CartItemDto> Items { get; set; }
	public decimal TotalPrice =>
		Items.Sum(item => item.Product.Price * item.Quantity);
}

public class CartItemDto {
	public required ProductDto Product { get; set; }
	public required int Quantity { get; set; }
}

public class AddToCartDto {
	public required int ProductId { get; set; }
	public required int Quantity { get; set; }
}

public class RemoveFromCartDto {
	public required int ProductId { get; set; }
}
