using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using DotNetEnv;
using DotNetEnv.Configuration;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

using BlazorShortUrl.Components.Account;
using BlazorShortUrl.Data;
using BlazorShortUrl.Helpers;
using BlazorShortUrl.Middleware;
using BlazorShortUrl.Services;


// Load environment variables from JSON and .env
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var appsettings = $"appsettings.{env}.json";
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile(appsettings, true)
    .AddDotNetEnv(".env", LoadOptions.TraversePath())
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

Log.Information("Serilog is starting");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Map some appsettings to environment variables.
    // DotNetEnv cannot access configuration provider for appsettings.json
    var appUrl = builder.Configuration.GetValue<string>("AppUrl");
    Environment.SetEnvironmentVariable("APP_URL", appUrl);

    var baseAddr = builder.Configuration.GetValue<string>("BaseAddress");
    Environment.SetEnvironmentVariable("BASE_ADDRESS", baseAddr);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(config)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new ExpressionTemplate(
            // Include trace and span ids when present.
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Code)));

    Log.Information("Serilog is running");

    string basedir = AppContext.BaseDirectory;
    Log.Information($"BASEDIR: {basedir}");
    Environment.SetEnvironmentVariable("BASEDIR", basedir);

    // TODO: Fix needed for Traefik
    // Configure ASP.NET Core to work with proxy servers and load balancers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;

        // DEBUG: Troubleshoot ForwardedHeaders.
        // options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
        // options.ForwardLimit = 3;
        // options.ForwardedForHeaderName = "X-Forwarded-For-BlazorShortUrl";
        // options.KnownProxies.Add(IPAddress.Parse(Env.GetString("KNOWN_PROXY_IP1")));
        // options.KnownProxies.Add(IPAddress.Parse(Env.GetString("KNOWN_PROXY_IP2")));
        // options.KnownProxies.Add(IPAddress.Parse(Env.GetString("KNOWN_PROXY_IP3")));
    });

    builder.Services.AddQuickGridEntityFrameworkAdapter();

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Add services to the container
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    builder.Services.AddHttpContextAccessor();


    // Add HttpClient for local API calls to IdentityController.
    var httpBaseUriAccessor = new HttpBaseUrlAccessor()
    {
        SiteUrlString = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey)
    };
    var baseAddress = httpBaseUriAccessor.GetHttpsUrl() ?? httpBaseUriAccessor.GetHttpUrl();
    baseAddress = Env.GetString("BASE_ADDRESS");
    Log.Information($"BaseAddress: {baseAddress}");

    // Register named HttpClient
    builder.Services
        .AddHttpClient("IdentityController", client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "BlazorShortUrl-IdentityController");
            client.DefaultRequestHeaders.Add("X-API-Key", Env.GetString("API_KEY"));
        })
        .AddStandardResilienceHandler();


    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("IdentityController"));


    // Add API Key middleware
    builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
    builder.Services.AddScoped<ApiKeyAuthFilter>();
    var apiKey = Env.GetString("API_KEY");
    Environment.SetEnvironmentVariable("API_KEY", apiKey);

    // Add required services
    builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
    builder.Services.AddScoped<IShortUrlService, ShortUrlService>();
    builder.Services.AddScoped<IHttpClientHelper, HttpClientHelper>();

    // Get connection strings
    var appDbContext = Env.GetString("AppDbContext");
    var dataContext = Env.GetString("DataContext");

    // Need to remove quotes when using .env file
    // appDbContext = appDbContext.Replace("\"", string.Empty).Trim();
    Environment.SetEnvironmentVariable("AppDbContext", appDbContext);
    Environment.SetEnvironmentVariable("DataContext", dataContext);
    // Log.Information($"init main AppDbContext: {appDbContext}");

    // Create LoggerFactory
    var logger = LoggerFactory.Create(logging =>
    {
        logging.AddSerilog(Log.Logger);
    });

    // Add application database
    builder.Services.AddDbContext<DataContext>();

    // Add Identity database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(appDbContext));

    builder.Services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddSingleton<IEmailSender<AppUser>, IdentityNoOpEmailSender>();

    // Configure Swagger middleware
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAuthorization(o =>
    {
        o.AddPolicy("ApiAdminPolicy", b => b.RequireRole("Admin"));
    });
    builder.Services.AddOpenApi();

    builder.Services.AddControllers();

    // ==========

    // Configure web application
    var app = builder.Build();

    // TODO: Fix needed for Traefik
    app.UseForwardedHeaders();

    // app.UseCertificateForwarding();
    // app.UseHttpLogging();

    // DEBUG: Troubleshoot HTTP request logging.
    // app.Use(async (context, next) =>
    // {
    //     // Connection: RemoteIp
    //     app.Logger.LogInformation("Request RemoteIp: {RemoteIpAddress}",
    //         context.Connection.RemoteIpAddress);
    //
    //     await next(context);
    // });

    app.MapOpenApi("/openapi/v1.json")
        .RequireAuthorization("ApiAdminPolicy");

    // Configure Swagger middleware
    app.UseSwaggerUi(options =>
    {
        options.DocumentTitle = "BlazorShortUrlApi";
        options.Path = "/openapi";
        options.DocumentPath = "/openapi/v1.json";
        options.DocExpansion = "list";
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);

        // TODO: Does not work with current Traefik configuration?
        // HTTP Strict Transport Security Protocol (HSTS)
        // The browser forces all communication over HTTPS.
        // The default HSTS value is 30 days.
        // You may want to change this for production scenarios (https://aka.ms/aspnetcore-hsts).
        // app.UseHsts();
    }

    // Write streamlined request completion events rather than verbose from the framework.
    // Remove this line and set the "Microsoft" level in appsettings.json to "Information"
    // to use the default framework request logging.
    app.UseSerilogRequestLogging();

    // Migrate Data
    // Task.Run(async () =>
    // {
    //     await MigrateDataAsync(app);
    // }).Wait();

    app.UseHttpsRedirection();
    app.UseAntiforgery();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllers();

    app.MapRazorComponents<BlazorShortUrl.Components.App>()
        .AddInteractiveServerRenderMode()
        .AllowAnonymous();

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();

    // Add Identity endpoints
    app.MapGroup("/api/account")
        .MapIdentityApi<AppUser>();

    app.Run();

    Log.Information("Application stopped cleanly");
}
catch (Exception ex)
{
    // Catch setup errors
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit.
    await Log.CloseAndFlushAsync();
}

// Migrate any database changes on startup (includes initial db creation)
static async Task MigrateDataAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        db.Database.Migrate();
    }

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        // var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        // Migrate ApplicationDb

        // Seed Identity database
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        try
        {
            await SeedData.InitializeAsync(services);
        }
        catch (Exception ex)
        {
            // var logger = loggerFactory.CreateLogger<Program>();
            Log.Error(ex, "An error occurred seeding the ApplicationDb.");
        }
    }
}