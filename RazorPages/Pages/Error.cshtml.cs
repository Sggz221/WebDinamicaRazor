using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Nuevas propiedades para personalizar el mensaje
    public int ErrorCode { get; set; }
    public string ErrorTitle { get; set; } = "Ha ocurrido un error";
    public string ErrorMessage { get; set; } = "Lo sentimos, ha ocurrido un error inesperado en el servidor.";

    public void OnGet(int? code)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        // Si recibimos un código por la URL (gracias al cambio en Program.cs)
        if (!code.HasValue) return;
        ErrorCode = code.Value;

        switch (ErrorCode)
        {
            case 404:
                ErrorTitle = "Página no encontrada";
                ErrorMessage = "Lo sentimos, el recurso que buscas (Funko o página) no existe.";
                break;
            case 500:
                ErrorTitle = "Error del Servidor";
                ErrorMessage = "Ha ocurrido un problema interno. Por favor, inténtalo más tarde.";
                break;
            default:
                ErrorTitle = $"Error {ErrorCode}";
                ErrorMessage = "Ha ocurrido un problema al procesar tu solicitud.";
                break;
        }
    }
}