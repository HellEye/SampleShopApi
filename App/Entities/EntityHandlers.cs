using SampleShopApi.App.Entities.Cart;
using SampleShopApi.App.Entities.Products;
namespace SampleShopApi.App.Entities;

public static class EntityHandlersExtensions {
	public static WebApplication MapEntityHandlers(this WebApplication app) {
		app.MapProductEndpoints();
		app.MapCartEndpoints();
		return app;
	}

}
