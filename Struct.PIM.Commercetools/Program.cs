using System.Net.Http.Headers;
using System.Text.Json;
using commercetools.Base.Client;
using commercetools.Sdk.Api;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Client.Endpoints;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Commercetools;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Services.Struct.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Struct PIM Commerce Tools Accelerator", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "ApiKey must appear in header",
        Type = SecuritySchemeType.ApiKey,
        Name = "XApiKey",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    var key = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
    {
        { key, new List<string>() }
    };
    c.AddSecurityRequirement(requirement);
});

builder.Services.UseCommercetoolsApi(builder.Configuration, "Client");

var commerceClientConfiguration = builder.Configuration.GetSection("Client").Get<ClientConfiguration>();
Settings.SetCurrentCommerceProjectKey(commerceClientConfiguration.ProjectKey);

var structApiConfiguration = builder.Configuration.GetSection("Struct").Get<StructConfiguration>();

if (!(string.IsNullOrEmpty(structApiConfiguration.ApiKey) || string.IsNullOrEmpty(structApiConfiguration.BaseUrl)))
{

    // Commerce DI
    builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IImportService, ImportService>();
    builder.Services.AddScoped<IProjectSettingsService, ProjectSettingsService>();
    builder.Services.AddScoped<IProductTypeService, ProductTypeService>();

    // Struct DI
    var pimFacade = new StructPIMApiClient(structApiConfiguration.BaseUrl, structApiConfiguration.ApiKey);
    builder.Services.AddScoped<IStructPimApiClient>(_ => new StructPimApiClient(pimFacade));
    builder.Services.AddScoped<IAttributeEndpoint>(_ => new AttributeEndpoint(pimFacade.RequestHandler));
    builder.Services.AddScoped<ICatalogueEndpoint>(_ => new CatalogueEndpoint(pimFacade.RequestHandler));
    builder.Services.AddScoped<ILanguageEndpoint>(_ => new LanguageEndpoint(pimFacade.RequestHandler));
    builder.Services.AddScoped<IProductEndpoint>(_ => new ProductEndpoint(pimFacade.RequestHandler));
    builder.Services.AddScoped<IProductStructureEndpoint>(_ => new ProductStructureEndpoint(pimFacade.RequestHandler));
    builder.Services.AddScoped<IVariantEndpoint>(_ => new VariantEndpoint(pimFacade.RequestHandler));

    builder.Services.AddScoped<IProductEndpointHandler, ProductEndpointHandler>();
    builder.Services.AddScoped<IVariantEndpointHandler, VariantEndpointHandler>();
    builder.Services.AddHttpClient("StructApiClient", httpClient =>
    {
        httpClient.BaseAddress = new Uri(structApiConfiguration.BaseUrl);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Struct.PIM.Commercetools");
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(structApiConfiguration.ApiKey);
    });

    // Misc
    builder.Services.AddSingleton<IErrorService, ErrorService>();

    var app = builder.Build();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();
    app.UseMiddleware<ApiKeyMiddleware>();
    app.MapControllers();

    app.Run();
}