using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Backend.Services.Funkos;
using Backend.Repositories.Categorias;
using Backend.Models.Dto.Funkos;

namespace RazorPages.Pages;

public class CreateModel : PageModel
{
    private readonly IFunkoService _funkoService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _webHostEnvironment; // Para saber dónde guardar la imagen

    public CreateModel(
        IFunkoService funkoService, 
        ICategoryRepository categoryRepository, 
        IWebHostEnvironment webHostEnvironment)
    {
        _funkoService = funkoService;
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    [BindProperty]
    public FunkoRequestDto Funko { get; set; } = default!;

    // Propiedad auxiliar para subir el fichero (no va a la BD directamente)
    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    // Lista para el desplegable de categorías
    public List<SelectListItem> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCategories();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Validamos el modelo (anotaciones del DTO)
        if (!ModelState.IsValid)
        {
            await LoadCategories(); // Recargamos categorías si hay error para que no salga vacío el select
            return Page();
        }

        // 2. Gestión de la Imagen
        if (ImageFile != null && ImageFile.Length > 0)
        {
            // Generamos un nombre único para evitar colisiones
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            // Creamos la carpeta si no existe
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Guardamos el fichero
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            // Asignamos el nombre del archivo al DTO
            Funko.Image = fileName;
        }
        else
        {
            // Imagen por defecto si no suben nada
            Funko.Image = "default.png";
        }

        // 3. Guardar en Base de Datos
        var result = await _funkoService.SaveAsync(Funko);

        if (!result.IsFailure) return RedirectToPage("./Index");
        
        ModelState.AddModelError(string.Empty, result.Error.Mensaje);
        await LoadCategories();
        return Page();

        // 4. Redirigir al listado
    }

    private async Task LoadCategories()
    {
        // Obtenemos todas las categorías (asumiendo que tu repo tiene GetAllAsync)
        // Nota: Ajusta esto si tu repo devuelve otra estructura, aquí asumo una lista simple.
        var categoriesList = await _categoryRepository.GetAllAsync();
        
        Categories = categoriesList.Select(c => new SelectListItem
        {
            Value = c.Nombre, // Tu DTO espera el Nombre de la categoría, no el ID
            Text = c.Nombre
        }).ToList();
    }
}