// En el Program.cs de tu proyecto RazorPages
using Backend.Infrastructure; // Namespace donde residen AddApplicationServices y AddDatabaseConfiguration

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar soporte para Razor Pages
builder.Services.AddRazorPages();

// 2. IMPORTANTE: Registrar los servicios del proyecto Backend
// Estos métodos deben estar disponibles si has referenciado el proyecto Backend
builder.Services.AddDatabaseConfiguration(builder.Configuration); // Configura la DB
builder.Services.AddApplicationServices(builder.Configuration); // Registra IFunkoService y repositorios 

var app = builder.Build();

app.InitializeDatabase();
// Configuración estándar de Razor Pages
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();


app.Run();