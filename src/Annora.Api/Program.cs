using Annora.Application.Listings;
using Annora.Infrastructure.Listings;
using Annora.Infrastructure.Storage;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(
        StorageOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.ConnectionString),
        "Storage connection string is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<
    IListingRepository,
    TableListingRepository>();

builder.Services.AddScoped<CreateListingHandler>();

builder.Build().Run();
/*using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
*/