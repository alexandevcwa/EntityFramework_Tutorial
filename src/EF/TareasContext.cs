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
            categoria.Property(p => p.Peso);    //Propiedad nueva para generar una migración
        });
        //FLUENT API
        modelBuilder.Entity<Tarea>(tarea => 
        {
            tarea.ToTable("Tareas");
            tarea.HasKey(p => p.TareaId);
            tarea.HasOne(p => p.Categoria).WithMany(p => p.Tareas).HasForeignKey(p => p.CategoriaId);
            tarea.Property(p => p.Titulo).IsRequired().HasMaxLength(200);
            tarea.Property(p => p.Descripcion);
            tarea.Property(p => p.PrioridadTareas);
            tarea.Property(p => p.FechaCreacion);
            tarea.Ignore(p => p.Resumen);
        });
    }
}