using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Backend.Services.Funkos;
using Backend.Models.Dto.Funkos;

namespace RazorPages.Pages;

public class DetalleModel(IFunkoService service) : PageModel
{
    // Propiedad donde guardaremos los datos para la vista
    public FunkoResponseDto Funko { get; set; } = null!;

    // Recibimos el ID desde la URL
    public async Task<IActionResult> OnGetAsync(long id)
    {
        var result = await service.GetByIdAsync(id);

        // Si el resultado falla (no encontrado), devolvemos error 404
        if (result.IsFailure)
        {
            return NotFound();
        }

        Funko = result.Value;
        return Page();
    }
}