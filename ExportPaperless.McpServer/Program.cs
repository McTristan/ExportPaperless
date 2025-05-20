using ExportPaperless.Domain;
using ExportPaperless.Mcp;
using ExportPaperless.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose() // Capture all log levels
    .WriteTo.File(Path.Combine("/tmp", "logs", "ExportPaperless_McpServer_.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose)
    .CreateLogger();

try
{
    Log.Information("Starting server...");

    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog();
    var configuration = Configuration.GetStandardConfiguration();
    builder.Services.AddSingleton(configuration);
    builder.Services.AddMcp(configuration);
    
    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(ExportFromPaperlessTools).Assembly);

    var app = builder.Build();
    
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}