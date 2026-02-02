using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Backend.Services.Funkos;
using Backend.Models.Dto;
using Backend.Models.Dto.Funkos;

namespace RazorPages.Pages;

public class IndexModel(IFunkoService service) : PageModel
{
    // Declaramos la propiedad que almacenará la lista de Funkos para mostrarla en la vista
    public IEnumerable<FunkoResponseDto> Funkos { get; set; } = [];

    // Vinculamos esta propiedad con el input del buscador de la vista
    // Usamos SupportsGet = true para que capture el valor desde la URL (ej: ?Nombre=Batman)
    [BindProperty(SupportsGet = true)]
    public string? Nombre { get; set; }

    // Este método se ejecuta automáticamente cuando cargamos la página
    public async Task OnGetAsync()
    {
        // Cambiamos Page: 0 por Page: 1 para alinearnos con la lógica del repositorio
        var filter = new FilterDto(Nombre, null, null, Page: 1); 

        var result = await service.GetAllAsync(filter);

        if (result.IsSuccess)
        {
            Funkos = result.Value.Items;
            // Log de depuración en consola para ver si llegan datos
            Console.WriteLine($"Datos cargados: {Funkos.Count()} funkos encontrados.");
        }
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        //Llamamos al servicio para borrar
        var result = await service.DeleteAsync(id);

        // Comprobamos si fue bien
        if (result.IsSuccess)
        {
            // PATRÓN POST-REDIRECT-GET
            // Si se borró, recargamos la página actual (esto vuelve a ejecutar OnGetAsync)
            return RedirectToPage();
        }
        else
        {
            // Si no se encontró o hubo error, devolvemos 404
            return NotFound();
        }
    }
}