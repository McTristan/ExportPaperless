using Microsoft.Extensions.Configuration;

namespace ExportPaperless.Services;

public static class Configuration
{
    public static IConfiguration GetStandardConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", true, true);
        configurationBuilder.AddEnvironmentVariables();
        ReadDotEnvFile(maxTraverseDepth: 3);
        return configurationBuilder.Build();
    }
    
    private static void ReadDotEnvFile(int maxTraverseDepth = 0)
    {
        const string file = ".env";

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, file);

        if (maxTraverseDepth == 0 && !File.Exists(filePath))
        {
            return;
        }

        if (maxTraverseDepth > 0)
        {
            while (!File.Exists(filePath) && maxTraverseDepth-- > 0)
            {
                var parent = Directory.GetParent(directory);
                if (parent == null)
                {
                    filePath = null;
                    break;
                }

                directory = parent.FullName;
                filePath = Path.Combine(directory, file);
            }
        }

        if (filePath == null || !File.Exists(filePath))
        {
            return;
        }

        ReadEnvFile(filePath);
    }

    private static void ReadEnvFile(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(['='], 2);

            if (parts.Length != 2)
            {
                continue;
            }

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}