using ExportPaperless.Domain;
using ExportPaperless.Mcp;
using ExportPaperless.McpServer.Tools;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose() // Capture all log levels
    .WriteTo.File(Path.Combine("/tmp", "logs", "ExportPaperless_McpServer_SSE_.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();
var configuration = Configuration.GetStandardConfiguration();
builder.Services.AddSingleton(configuration);
builder.Services.AddMcp(configuration);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(ExportFromPaperlessTools).Assembly);

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();

var app = builder.Build();

app.MapMcp();

app.Run();