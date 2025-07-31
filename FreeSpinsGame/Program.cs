using FreeSpinsGame;
using FreeSpinsGame.Infrastructure.Data;
using FreeSpinsGame.Web.Autoregistration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterApplicationsServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<SpinGameDbContext>();

await dbContext.Database.MigrateAsync();
await DataSeeder.SeedDataAsync(dbContext);

app.UseHttpsRedirection();

app.RegisterApiRoutes();

app.Run();