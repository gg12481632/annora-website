using Annora.Application.Images;
using Annora.Infrastructure.Images;using Annora.Application.Listings;
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

builder.Services.AddSingleton<
    IListingImageStorage,
    BlobListingImageStorage>();

builder.Services.AddSingleton<
    IImageUploadStorage,
    ImageUploadStorage>();

builder.Services.AddScoped<CreateListingHandler>();
builder.Services.AddScoped<GetListingsHandler>();
builder.Services.AddScoped<GetListingByIdHandler>();
builder.Services.AddScoped<CreateImageUploadHandler>();

builder.Build().Run();
