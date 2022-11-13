# Proyecto Fundamentos de Entity Framework

El proyecto contiene diferentes commits con la explicaciones del ejemplo que se implementa en código, cada commit es un ejemplo diferente para poder comprender el funcionamiento de Entity Framework.

## Temas por commit

1. Mapeo de base de datos y test en memoria del modelo.

    - commit 2e65b9ca17775d81d44581eadc8410fc67e967cb

2. Creación de base de datos en base al modelo creado en código.

    - commit 8c6a20273cc690d0a76c3df762472cfc42c0fa7d

3. Configuración de modelos con Fluent API y verificación de validaciones con Fluent API.

    - commit 06b700a2cc40e08c64db1cbd926940ff9aa43474

4. Migraciones
    - commit e48aba0100d49079e51a1ef5ed98594d9bcac0f1

----

## Mapeo de base de datos y test en memoria del modelo

Contiene ejemplos de como crear modelos y utilizar **data annotation validators** para luego probar el modelo en memoria.

### Fuentes

- [Data Annotations Attributes in EF 6 and EF Core](https://www.entityframeworktutorial.net/code-first/dataannotation-in-code-first.aspx)
- [Validation with the Data Annotation Validators (C#)](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/models-data/validation-with-the-data-annotation-validators-cs)
- [Code First Data Annotations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations?source=recommendations)

### Creación de un modelo con data annotation validators

``` c#
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcApplication1.Models
{
    
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [DisplayName("Price")]
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]
        public decimal UnitPrice { get; set; }
    }
}
```

### Creacion de un context para crear el modelo en base de datos

``` c#
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.EF;

public class TareasContext : DbContext
{
    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Tarea> Tareas { get; set; }

    public TareasContext(DbContextOptions<TareasContext> options) : base(options) { }
}
```

### Verificar el modelo en memoria

```c#
builder.Services.AddDbContext<TareasContext>(p =>
{
    p.UseInMemoryDatabase("TareasDB");
});

//Endpoint
app.MapGet("/dbConexion", async ([FromServices] TareasContext dbContext) =>
{
    dbContext.Database.EnsureCreated();
    return Results.Ok("Base de datos en memoria: " + dbContext.Database.IsInMemory());
});

app.Run();
```

----

## Creación de base de datos en base al modelo creado en código

El siguiente código muestra la construcción del servicio de conexión con la base de datos y la contrucción del endpoint para valudar si la creación de la base de datos fue correcta.

### Agregar el servicio de conexión a base de datos y el endpoint a [Program.cs](/Program.cs)

``` c#
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.EF;

var builder = WebApplication.CreateBuilder(args);
//Agregar servicio de conexión con la base de datos
builder.Services.AddSqlServer<TareasContext>(builder.Configuration.GetConnectionString("SQLServer_PlatziDb"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//Endpoint
app.MapGet("/dbConexion", async ([FromServices] TareasContext dbContext) =>
{
    dbContext.Database.EnsureCreated();
    return Results.Ok("Base de datos en memoria: " + dbContext.Database.IsInMemory());
});

app.Run();
```

### Agregar cadena de conexión a archivo [appsettings.json](/appsettings.json)

``` json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SQLServer_PlatziDb": "Data Source=192.168.248.128;Initial Catalog=PlatziDb;user id=sa;password=WL5ut9QgwQ;TrustServerCertificate=True"
  }
}
```

----

## Configuracion de modelos con Fluent API y verificación de validaciones con Fluent API

Sobreescritura del método [OnModelCreating](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext.onmodelcreating?view=entity-framework-6.2.0#system-data-entity-dbcontext-onmodelcreating(system-data-entity-dbmodelbuilder)) de la clase [DbContext](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext?view=entity-framework-6.2.0)

```c#
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.EF;

public class TareasContext : DbContext
{
    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Tarea> Tareas { get; set; }

    public TareasContext(DbContextOptions<TareasContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //FLUENT API
        modelBuilder.Entity<Categoria>(categoria =>
        {
            categoria.ToTable("Categorias");
            categoria.HasKey(p => p.CategoriaId);
            categoria.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            categoria.Property(p => p.Descripcion);
        });

        modelBuilder.Entity<Tarea>(tarea => 
        {
            tarea.ToTable("Tareas");    //Nombre de la tabla
            tarea.HasKey(p => p.TareaId);   //Primary Key
            tarea.HasOne(p => p.Categoria).WithMany(p => p.Tareas).HasForeignKey(p => p.CategoriaId);   //Foreign Key
            tarea.Property(p => p.Titulo).IsRequired().HasMaxLength(200);
            tarea.Property(p => p.Descripcion);
            tarea.Property(p => p.PrioridadTareas);
            tarea.Property(p => p.FechaCreacion);
            tarea.Ignore(p => p.Resumen);   //ignorar un atributo para no agregarlo en el modelo de base de datos
        });
    }
}
```

----

## Migraciones

>Es una funcionalidad de Entity Framework que nos permite guardar de manera incremental los cambios realizados en la base de datos. Nos permite construir un versionamiento de la base de datos.
>

Para utilizar las migraciones necesitamos agregar una herramienta deltro del CLI de dotnet que nos permitira generar las migraciones de la base de datos desde el CLI

- [Entity Framework Core tools reference - .NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

### Instalar Entity Framework Core tools

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef
```

### Crear los archivos de migración

Este comando genenra una carpeta llamada [Migratios](/Migrations/) con diferemtes clases con la configuración de los modelos que se crearon con Entity Framewoerk.
Si vamos a [TareasContextModelSnapshot](/Migrations/TareasContextModelSnapshot.cs) lo que tendremos será la configuraci[on actial de la base de datos.

El arhivo [20221113042418_InitialCreate](./Migrations/20221113042418_InitialCreate.cs) contendra toda la configuración inicial de nuestra base de datos, cada vez que creemos una migración tendremos dos métodos (Up - Down), el método **Up** contendra todo lo que se va a ejecutar en la base de datos de la migración que se desea ejecutar y el método **Down** nos permitira revertir los cambios realizados al estado anterio de la migración.

```bash
dotnet ef migrations add InitialCreate
```

### Ejecutar una migración

>Es importante recalcar que al trabajar con migraciones es recomendable eliminar la base de datos si ya está creada y primero terminar el modelo base de nuesta base de datos ya que si creamos la base de datos antes y luego utilizamos migraciones, esta no contendra la tabla que lleva el control de las migraciones, debemos utilizar migraciones cada vez que realicemos cambios en la base de datos ya que es muy difícil utilizar migraciones con una base de datos ya en producción con datos y una configuración previa.
>

```bash
dotnet ef database update
```

### Creando una migración

Para crear una nueva migración con las modificaciones o nuevos modelos agregados con Entity Framework, los siguientes comandos nos ayudaran a generar una nueva migración a la base de datos para efectuar los cambios.

```bash
dotnet ef migrations add ColumnPesoCategoria_11_13_2022
```

Este comando generar las clases con los métodos **Up** y **Down** para efectuar los nuevos cambios en la base de datos o regresarla al estado anterior y deshacer la migración.

#### **Archivos generados por la migración**

- [20221113184629_ColumnPesoCategoria_11_13_2022.cs](/Migrations/20221113184629_ColumnPesoCategoria_11_13_2022.cs)
- [20221113184629_ColumnPesoCategoria_11_13_2022.Designer.cs](/Migrations/20221113184629_ColumnPesoCategoria_11_13_2022.Designer.cs)

### Actualizar la base de datos con la nueva migración

```bash
dotnet ef database update
```
