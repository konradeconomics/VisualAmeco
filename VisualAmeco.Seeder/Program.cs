using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            Console.WriteLine("--- Seeder Main Started ---"); // DEBUG LINE

            // --- Setup Host Builder ---
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.Sources.Clear();
                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug); // Keep this at Debug for now
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        // This exception should have been caught if appsettings was missing/invalid
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

             Console.WriteLine("--- Host Built ---"); // DEBUG LINE

            // --- Run the Seeding Logic ---
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            // Use logger from here on, but keep Console writes for flow verification
            logger.LogInformation("AMECO Seeder application starting...");

            Console.WriteLine("--- Checking Data Directory ---"); // DEBUG LINE
            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            logger.LogInformation("Looking for AMECO CSV files in: {DataDirectory}", Path.GetFullPath(dataDirectory));

            List<string> filePaths;
            try
            {
                if (!Directory.Exists(dataDirectory))
                {
                    Console.WriteLine($"--- Data Directory NOT FOUND at: {Path.GetFullPath(dataDirectory)} ---"); // DEBUG LINE
                    logger.LogError("Data directory not found: {DataDirectory}. Please ensure the 'Data' folder with CSVs is copied to the output directory.", Path.GetFullPath(dataDirectory));
                    return; // Exit
                }
                Console.WriteLine("--- Found Data Directory ---"); // DEBUG LINE

                // *** CHANGE HERE: Use uppercase .CSV extension in pattern ***
                filePaths = Directory.EnumerateFiles(dataDirectory, "AMECO*.CSV", SearchOption.TopDirectoryOnly).ToList();

                if (!filePaths.Any())
                {
                     Console.WriteLine($"--- No AMECO*.CSV files found in: {Path.GetFullPath(dataDirectory)} ---"); // DEBUG LINE (Updated pattern)
                    logger.LogWarning("No AMECO*.CSV files found in {DataDirectory}.", Path.GetFullPath(dataDirectory));
                    return; // Exit
                }
                 Console.WriteLine($"--- Found {filePaths.Count} CSV Files ---"); // DEBUG LINE
                logger.LogInformation("Found {Count} AMECO files to process.", filePaths.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- ERROR finding CSV files: {ex.Message} ---"); // DEBUG LINE
                logger.LogError(ex, "Error finding CSV files in data directory: {DataDirectory}", Path.GetFullPath(dataDirectory));
                return; // Exit
            }

            Console.WriteLine("--- Entering Service Scope ---"); // DEBUG LINE
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    logger.LogInformation("Attempting to parse and save data...");
                    Console.WriteLine("--- Resolving and Calling ParseAndSaveAsync ---"); // DEBUG LINE
                    var parser = serviceProvider.GetRequiredService<IAmecoCsvParser>();
                    bool success = await parser.ParseAndSaveAsync(filePaths);
                     Console.WriteLine($"--- ParseAndSaveAsync returned: {success} ---"); // DEBUG LINE

                    if (success)
                    {
                        logger.LogInformation("Seeding process completed successfully.");
                    }
                    else
                    {
                        logger.LogError("Seeding process finished with errors (check previous logs).");
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine($"--- UNHANDLED EXCEPTION during seeding: {ex.Message} ---"); // DEBUG LINE
                    logger.LogCritical(ex, "An unhandled exception occurred during the seeding process.");
                }
            }

            Console.WriteLine("--- Seeder Main Finished ---"); // DEBUG LINE
            logger.LogInformation("AMECO Seeder application finished.");
        }
    }
} 
