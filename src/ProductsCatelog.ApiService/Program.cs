using ProductsCatelog.ApiService.HostedServices;
using ProductsCatelog.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
// ProductSeeder is registered as scoped because it may resolve scoped deps in real apps
builder.Services.AddScoped<ProductSeeder>();

// Hosted services are registered as singleton; it will create scopes to resolve scoped services.
// Eager initialization happens compare to regular AddSingleton service which is lazy.
builder.Services.AddHostedService<ProductCatalogHostedService>();

// Use the IHostApplicationLifetime based hosted lifecycle service to demonstrate alternative startup/shutdown handling.
// This is generally preferred over the regular IHostedService for more complex scenarios.
// It allows better integration with the host's lifecycle events.
//builder.Services.AddHostedService<ProductCatalogHostedLifecycleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
