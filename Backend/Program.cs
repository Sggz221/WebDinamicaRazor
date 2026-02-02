using Backend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores (REST)
builder.Services.AddControllers();

// Configuraciones modulares
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsConfiguration();

var app = builder.Build();

// Middleware de manejo global de errores
app.UseGlobalErrorHandling();

// Inicialización de Base de Datos
app.InitializeDatabase();

// Pipeline de middleware de la aplicación
app.UseApplicationMiddleware();

app.Run();