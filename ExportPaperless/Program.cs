using ExportPaperless.Jobs;
using ExportPaperless.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services
    .AddAuthentication("ApiKeyScheme")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", null);

builder.Services.AddServices();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DeleteOldFilesJob");

    q.AddJob<DeleteOldFilesJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DeleteOldFilesTrigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(6)
            .RepeatForever()
        )
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Learn more about configuring Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Paperless Export API", Version = "v1" });

    var referenceId = "ApiKey";
    c.AddSecurityDefinition(referenceId, new OpenApiSecurityScheme
    {
        Description =
            "API Key needed to access the endpoints. Use header: `x-api-key: {your token from paperless user profile}` or configure environment variable `PAPERLESS__API_TOKEN` on server side.",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(referenceId)
            {
                Reference = new OpenApiReferenceWithDescription
                {
                    Type =  ReferenceType.SecurityScheme,
                    Id = referenceId,
                    Description = "API Key needed to access the endpoints. Use header: `x-api-key: {your token from paperless user profile}` or configure environment variable `PAPERLESS__API_TOKEN` on server side."  
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();