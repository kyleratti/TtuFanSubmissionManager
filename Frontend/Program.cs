using AdminData.Hubs;
using AdminData.Services;
using Blazored.LocalStorage;
using Blazored.Modal;
using Core.AppSettings;
using Core.DbConnection;
using Core.Interfaces;
using Frontend.Data;
using Frontend.Util;
using Microsoft.AspNetCore.ResponseCompression;
using Twilio.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapControllers();

app.UseRouting();

app.MapFallbackToPage("/_Host");

app.UseEndpoints(ConfigureHubs);

app.Run();

void ConfigureServices(IServiceCollection services)
{
    ConfigureAppSettings<RawDbSettings, DbSettings>(services, "DbSettings");
    ConfigureAppSettings<RawTwilioSettings, TwilioSettings>(services, "TwilioSettings");

    services.AddBlazoredModal();
    services.AddBlazoredLocalStorage();

    services.AddServerSideBlazor();
    services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    });

    services.AddScoped<DbConnection>();
    services.AddScoped<ISubmissionQueue, SubmissionQueue>();
    services.AddHttpClient<ITwilioRestClient, TtuTwilioClient>();
    services.AddScoped<TwilioService>();
    services.AddHttpClient<HttpHelperClient>();
    services.AddScoped<ValidateTwilioRequestFilter>();
    services.AddScoped<HubHelper>();
    services.AddScoped<ClientSettingsProvider>();

    services.AddSingleton<MetadataCache>();
}

void ConfigureHubs(IEndpointRouteBuilder webApp)
{
    webApp.MapBlazorHub();
    webApp.MapHub<SubmissionHub>("/hubs/submissions");
}

void ConfigureAppSettings<TRaw, TService>(IServiceCollection services, string sectionName)
    where TRaw : class
    where TService : class
{
    services.AddOptions<TRaw>()
        .Bind(builder.Configuration.GetSection(sectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddScoped<TService>();
}

