using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models;

public class Categoria
{
    //[Key]
    public Guid CategoriaId { get; set; }

    //[Required]
    //[MaxLength(150)]
    public String Nombre { get; set; }

    public String Descripcion { get; set; }

    public int Peso { get; set; }   //Propiedad nueva para generar una migración

    public virtual ICollection<Tarea> Tareas { get; set; }

}