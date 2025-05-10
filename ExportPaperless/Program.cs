using ExportPaperless;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services
    .AddAuthentication("ApiKeyScheme")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", null);

builder.Services.AddHttpContextAccessor();

builder.Services.AddServices();

// Learn more about configuring Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Paperless Export API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description =
            "API Key needed to access the endpoints. Use header: `x-api-key: {your token from paperless user profile}` or configure environment variable `PAPERLESS__API_TOKEN` on server side.",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().WithOpenApi();
app.Run();