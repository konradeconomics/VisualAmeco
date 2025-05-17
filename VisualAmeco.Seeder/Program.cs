using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

using VisualAmeco.Application.Interfaces;
using VisualAmeco.Application.Services;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Data.Repositories;
using VisualAmeco.Parser.Parsers;
using VisualAmeco.Parser.Services;

namespace VisualAmeco.Seeder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Seeder Main Started (Console Output) ---");
            
            var initialConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(initialConfiguration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("--- Serilog Configured. Building Host (Serilog Output) ---");

                // --- Setup Host Builder ---
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.Sources.Clear();
                        config.SetBasePath(AppContext.BaseDirectory);
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                                              ?? hostingContext.HostingEnvironment.EnvironmentName // This should be available if the lambda signature is (HostBuilderContext hostingContext, ...)
                                              ?? "Production";
                        config.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
                        config.AddEnvironmentVariables();
                    })
                    .UseSerilog()
                    .ConfigureServices((context, services) =>
                    {
                        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                        if (string.IsNullOrEmpty(connectionString))
                        {
                            Log.Error("Database connection string 'DefaultConnection' not found in configuration."); // Use Serilog
                            throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
                        }
                        services.AddDbContext<VisualAmecoDbContext>(options =>
                            options.UseNpgsql(connectionString));

                        services.AddScoped<IChapterRepository, ChapterRepository>();
                        services.AddScoped<ISubchapterRepository, SubchapterRepository>();
                        services.AddScoped<IVariableRepository, VariableRepository>();
                        services.AddScoped<ICountryRepository, CountryRepository>();
                        services.AddScoped<IValueRepository, ValueRepository>();
                        services.AddScoped<IUnitOfWork, UnitOfWork>();

                        services.AddTransient<ICsvFileReader, CsvFileReader>();
                        services.AddTransient<ICsvRowMapper, CsvRowMapper>();
                        services.AddTransient<ICsvHeaderValidator, CsvHeaderValidator>();

                        services.AddTransient<IAmecoEntitySaver, AmecoEntitySaver>();
                        services.AddTransient<IAmecoCsvParser, AmecoCsvParser>();
                    })
                    .Build();

                Console.WriteLine("--- Host Built (Console Output) ---");
                Log.Information("--- Host Built (Serilog Output) ---");

                // --- Run the Seeding Logic ---
                var appLogger = host.Services.GetRequiredService<ILogger<Program>>();
                appLogger.LogInformation("AMECO Seeder application starting (via ILogger)...");

                Console.WriteLine("--- Checking Data Directory (Console Output) ---");
                var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
                appLogger.LogInformation("Looking for AMECO CSV files in: {DataDirectory}", Path.GetFullPath(dataDirectory));

                List<string> filePaths;
                try
                {
                    if (!Directory.Exists(dataDirectory))
                    {
                        Console.WriteLine($"--- Data Directory NOT FOUND at: {Path.GetFullPath(dataDirectory)} (Console Output) ---");
                        appLogger.LogError("Data directory not found: {DataDirectory}. Please ensure the 'Data' folder with CSVs is copied to the output directory.", Path.GetFullPath(dataDirectory));
                        return;
                    }
                    Console.WriteLine("--- Found Data Directory (Console Output) ---");
                    appLogger.LogDebug("Data directory found at: {DataDirectory}", Path.GetFullPath(dataDirectory));


                    filePaths = Directory.EnumerateFiles(dataDirectory, "AMECO*.CSV", SearchOption.TopDirectoryOnly).ToList();

                    if (!filePaths.Any())
                    {
                        Console.WriteLine($"--- No AMECO*.CSV files found in: {Path.GetFullPath(dataDirectory)} (Console Output) ---");
                        appLogger.LogWarning("No AMECO*.CSV files found in {DataDirectory}.", Path.GetFullPath(dataDirectory));
                        return;
                    }
                    Console.WriteLine($"--- Found {filePaths.Count} CSV Files (Console Output) ---");
                    appLogger.LogInformation("Found {Count} AMECO files to process.", filePaths.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- ERROR finding CSV files: {ex.Message} (Console Output) ---");
                    appLogger.LogError(ex, "Error finding CSV files in data directory: {DataDirectory}", Path.GetFullPath(dataDirectory));
                    return;
                }

                Console.WriteLine("--- Entering Service Scope (Console Output) ---");
                appLogger.LogDebug("Entering service scope for parsing.");
                using (var scope = host.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    try
                    {
                        appLogger.LogInformation("Attempting to parse and save data...");
                        Console.WriteLine("--- Resolving and Calling ParseAndSaveAsync (Console Output) ---");
                        var parser = serviceProvider.GetRequiredService<IAmecoCsvParser>();
                        bool success = await parser.ParseAndSaveAsync(filePaths);
                        Console.WriteLine($"--- ParseAndSaveAsync returned: {success} (Console Output) ---");

                        if (success)
                        {
                            appLogger.LogInformation("Seeding process completed successfully.");
                        }
                        else
                        {
                            appLogger.LogError("Seeding process finished with errors (check previous logs for details).");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--- UNHANDLED EXCEPTION during seeding: {ex.Message} (Console Output) ---");
                        appLogger.LogCritical(ex, "An unhandled exception occurred during the seeding process.");
                    }
                }

                Console.WriteLine("--- Seeder Main Finished (Console Output) ---");
                appLogger.LogInformation("AMECO Seeder application finished.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "--- Seeder application terminated unexpectedly (Serilog Output) ---");
                Console.WriteLine($"--- FATAL ERROR: Seeder application terminated unexpectedly: {ex.Message} (Console Output) ---");
            }
            finally
            {
                Log.Information("--- Flushing Serilog and Shutting Down (Serilog Output) ---");
                Console.WriteLine("--- Flushing Logs and Shutting Down (Console Output) ---");
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
