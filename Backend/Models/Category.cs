using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public record Category(string Nombre)
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string Nombre { get; set; } = Nombre;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } =  DateTime.UtcNow;
}