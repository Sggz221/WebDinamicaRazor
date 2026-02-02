using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Backend.Services.Funkos;
using Backend.Repositories.Categorias;
using Backend.Models.Dto.Funkos;

namespace RazorPages.Pages;

public class EditModel(
    IFunkoService funkoService,
    ICategoryRepository categoryRepository,
    IWebHostEnvironment webHostEnvironment) : PageModel
{
    [BindProperty]
    public FunkoRequestDto Funko { get; set; } = default!;

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    // 1. Cargar datos al entrar en la página
    public async Task<IActionResult> OnGetAsync(long id)
    {
        var result = await funkoService.GetByIdAsync(id);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var funkoResponse = result.Value;

        // Mapeamos los datos existentes al DTO del formulario para que aparezcan rellenos
        Funko = new FunkoRequestDto(
            funkoResponse.Nombre,
            funkoResponse.Categoria, 
            funkoResponse.Precio,
            funkoResponse.Image
        );

        await LoadCategories();
        return Page();
    }

    // 2. Procesar la actualización
    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategories();
            return Page();
        }

        // Gestión de imagen: 
        // Si suben una nueva, la guardamos y actualizamos el nombre.
        // Si NO suben nada, el valor de 'Funko.Image' vendrá del input hidden (imagen antigua), así que no hacemos nada.
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            var uploadPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            Funko.Image = fileName; // Actualizamos a la nueva imagen
        }

        var result = await funkoService.UpdateAsync(id, Funko);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Mensaje);
            await LoadCategories();
            return Page();
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadCategories()
    {
        var categoriesList = await categoryRepository.GetAllAsync();
        Categories = categoriesList.Select(c => new SelectListItem
        {
            Value = c.Nombre,
            Text = c.Nombre
        }).ToList();
    }
}