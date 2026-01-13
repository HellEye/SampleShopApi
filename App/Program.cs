using App.Exceptions;
using SampleShopApi.App.Data;
using SampleShopApi.App.Data.Populator;
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
builder.Services.AddScoped<DiscogsClient>();
builder.Services.AddScoped<DatabasePopulator>();
builder.Services.AddCors(options => {
	options.AddPolicy("AllowLocalhost",
		policy => policy
			.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3000/", "https://localhost:3000/")
			.AllowAnyHeader()
			.AllowAnyMethod());
});
var app = builder.Build();

app.UseCors("AllowLocalhost");
app.UseExceptionHandler();
app.MapEntityHandlers();


app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


if (args.Length > 0 && args[0].Equals("populate-db", StringComparison.OrdinalIgnoreCase)) {
	using var scope = app.Services.CreateScope();
	var populator = scope.ServiceProvider.GetRequiredService<DatabasePopulator>();
	await populator.PopulateAsync();
	return;
}

app.Run();

