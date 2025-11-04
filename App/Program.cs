using App.Cart;
using App.Data;
using App.Exceptions;
using App.Products;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

var app = builder.Build();
app.UseExceptionHandler();
app.MapProductEndpoints();
app.MapCartEndpoints();


app.MapOpenApi();
// if (app.Environment.IsDevelopment()) {
// }
// app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

// app.MapGet("/docs", () => {
// 	return Results.File(File.OpenRead("App/Documentation/stoplightio.html"), "text/html");
// })
// .WithName("Documentation")
// .WithDescription("Show API documentation using stoplightio");

app.Run();

