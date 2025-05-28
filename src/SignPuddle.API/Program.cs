using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Data;
using SignPuddle.API.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Suppress output formatter buffering to avoid PipeWriter issues in tests
    options.SuppressOutputFormatterBuffering = true;
});
// Don't clear TypeInfoResolverChain here - it breaks deserialization

// Configure JSON options for minimal APIs only (not MVC controllers)
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add health checks
builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<IFormatService, FormatService>();
builder.Services.AddScoped<IRenderService, RenderService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register repositories
builder.Services.AddScoped<ISignRepository, SignRepository>();
builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();
builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add CORS policy for Svelte frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Add global exception handler for production
    app.UseExceptionHandler("/error");
}

// Global exception handler endpoint
app.Map("/error", async (HttpContext context, ILogger<Program> logger) =>
{
    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionFeature?.Error;

    // Log the exception with more details
    logger.LogError(exception, "Unhandled exception occurred. Path: {Path}, Method: {Method}", 
        context.Request.Path, context.Request.Method);

    var response = new
    {
        error = "An error occurred while processing your request.",
        message = app.Environment.IsDevelopment() ? exception?.Message : "Internal server error",
        type = exception?.GetType().Name,
        timestamp = DateTime.UtcNow,
        // Include request path in development for debugging
        path = app.Environment.IsDevelopment() ? context.Request.Path.ToString() : null
    };

    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    context.Response.ContentType = "application/json";

    // Create a fresh JsonSerializerOptions instance to avoid PipeWriter issues
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // Use Stream-based serialization to avoid PipeWriter issues
    await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
});

// Configure health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        // Use Stream-based serialization to avoid PipeWriter issues
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
    }
});

// Only use HTTPS redirection when not in testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }