using App.Exceptions;
using SampleShopApi.App.Data;
using SampleShopApi.App.Entities;
using SampleShopApi.App.Entities.Cart;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment()) {
	DotNetEnv.Env.Load();
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShopApiContext>(options => {
	if (builder.Environment.IsDevelopment()) {
		options.EnableSensitiveDataLogging();
	}
});
builder.Services.AddHostedService<CartCleanupService>();
builder.Services.AddEntityServices();
var app = builder.Build();


app.UseExceptionHandler();
app.MapEntityHandlers();


app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();

