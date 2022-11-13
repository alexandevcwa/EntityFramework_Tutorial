using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.EF;

var builder = WebApplication.CreateBuilder(args);

//BASE DE DATOS EN MEMORIA PARA PROBAR LOS MODELOS
//builder.Services.AddDbContext<TareasContext>(p =>
//{
//    p.UseInMemoryDatabase("TareasDB");
//});

builder.Services.AddSqlServer<TareasContext>(builder.Configuration.GetConnectionString("SQLServer_PlatziDb"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/dbConexion", async ([FromServices] TareasContext dbContext) =>
{
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    if (dbContext.Database.IsInMemory())
    {
        return Results.Ok("Base de datos en memoria");
    }
    else if (dbContext.Database.IsSqlServer())
    {
        return Results.Ok("Base de datos SQL Server");
    }
    return Results.Problem("Sin Base de datos");
});

app.Run();