using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Data;
using SignPuddle.API.Services;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();

    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(jsonOptions));
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressMapClientErrors = true; // avoids using ProblemDetails for 404/etc.
});

// Add health checks
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
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
app.UseExceptionHandler("/error");

app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Json(new
    {
        error = exception?.Message,
        type = exception?.GetType().Name,
        stack = exception?.StackTrace
    });
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