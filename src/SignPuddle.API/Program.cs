using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
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
        // ReferenceHandler removed to avoid $id/$values serialization
    };

    options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(jsonOptions));
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressMapClientErrors = true; // avoids using ProblemDetails for 404/etc.
});

// Configure CosmosDB
string CosmosAccountEndpoint() => builder.Configuration["CosmosDb:AccountEndpoint"] ?? "";
string CosmosAccountKey() => builder.Configuration["CosmosDb:AccountKey"] ?? "";
string DatabaseName() => builder.Configuration["CosmosDb:DatabaseName"] ?? "SignPuddle";

if (!builder.Environment.IsEnvironment("Testing"))
{
    

    // Configure DbContext for CosmosDB
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseCosmos(
            accountEndpoint: CosmosAccountEndpoint(),
            accountKey: CosmosAccountKey(),
            databaseName: DatabaseName(),
            cosmosOptionsAction: options =>
            {
                options.ConnectionMode(ConnectionMode.Direct);
                options.MaxRequestsPerTcpConnection(20);
                options.MaxTcpConnectionsPerEndpoint(32);
            }
        ));

    // Add health checks for CosmosDB
    builder.Services.AddHealthChecks()
        .AddCheck("cosmosdb-check", () =>
        {
            try
            {
                HealthCheckResult? result = null;
                Task.Run(async () =>
                {
                    var client = new CosmosClient(CosmosAccountEndpoint(), CosmosAccountKey());
                    // Use async/await to avoid deadlocks and improve performance
                    await client.ReadAccountAsync();
                    result = HealthCheckResult.Healthy("CosmosDB connection is healthy");
                }).Wait();

                if (result != null)
                {
                    return (HealthCheckResult)result;
                }
                else
                {
                    return HealthCheckResult.Unhealthy("CosmosDB health check did not complete successfully");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("CosmosDB connection failed", ex);
            }
        }, new[] { "cosmosdb", "database" });
}
else
{
    // Add a basic health check for the testing environment to satisfy MapHealthChecks
    builder.Services.AddHealthChecks();
}

builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<IFormatService, FormatService>();
builder.Services.AddScoped<IRenderService, RenderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISpmlImportService, SpmlImportService>();
builder.Services.AddScoped<ISpmlPersistenceService, SpmlPersistenceService>();

// Register repositories
builder.Services.AddScoped<ISignRepository, SignRepository>();
builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();
builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpmlRepository, SpmlRepository>();

// Add CORS policy for Svelte frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error;

    // Ensure only specific, safe properties are serialized
    var errorResponse = new
    {
        Message = exception?.Message,
        Type = exception?.GetType().ToString(), // Use ToString() for type name
        StackTraceString = exception?.StackTrace // StackTrace is already a string
    };
    // It's good practice to return a proper status code for errors.
    // Results.Json by default might return 200 if not specified.
    return Results.Json(errorResponse, statusCode: Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError);
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