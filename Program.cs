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

try
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .CreateLogger();

    Log.Information("Serilog is starting ...");
    Log.Information($"Environment is {env}");

    var builder = WebApplication.CreateBuilder(args);

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
    Environment.SetEnvironmentVariable("BASEDIR", basedir);

    // TODO: Fix needed for Traefik
    // Configure ASP.NET Core to work with proxy servers and load balancers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
    });

    builder.Services.AddQuickGridEntityFrameworkAdapter();
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Add services to the container.
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
    // TODO: This is not working with Docker container.
    var httpBaseUriAccessor = new HttpBaseUrlAccessor()
    {
        // SiteUrlString = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey).Trim()
        SiteUrlString = Env.GetString("BASE_ADDRESS")
    };

    // var baseAddress = httpBaseUriAccessor.GetHttpsUrl() ?? httpBaseUriAccessor.GetHttpUrl();
    var baseAddress = Env.GetString("BASE_ADDRESS");
    Log.Information($"ASPNETCORE_ENVIRONMENT: {Env.GetString("ASPNETCORE_ENVIRONMENT")}");
    // Log.Information($"ASPNETCORE_URLS: {Env.GetString("ASPNETCORE_URLS")}");
    // Log.Information($"ASPNETCORE_HTTP_PORTS: {Env.GetString("ASPNETCORE_HTTP_PORTS")}");
    // Log.Information($"ASPNETCORE_HTTPS_PORTS: {Env.GetString("ASPNETCORE_HTTPS_PORTS")}");

    // TODO: BaseAddress is null ???
    Log.Information($"BUILD_PLATFORM: {Env.GetString("BUILD_PLATFORM")}");
    Log.Information($"BASE_ADDRESS: {Env.GetString("BASE_ADDRESS")}");
    Log.Information($"BaseAddress hello: {baseAddress}");
    Log.Information($"BASEDIR: {Env.GetString("BASEDIR")}");
    Log.Information($"LOG_LEVEL: {Env.GetString("LOG_LEVEL")}");

    // Register named HttpClient for Identity API
    var httpClientBuilder = builder.Services
        .AddHttpClient("IdentityController", client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "BlazorShortUrl-IdentityController");
            client.DefaultRequestHeaders.Add("X-API-Key", Env.GetString("API_KEY"));
        })
    .AddStandardResilienceHandler();

    // TODO: Need to refactor IHttpClientFactory
    // Create the HttpClient as a singleton instance
    // builder.Services.AddSingleton(sp => sp.GetRequiredService<IHttpClientFactory>()
    //     .CreateClient("IdentityController"));


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

    // Need to remove quotes when using .env file ??
    // appDbContext = appDbContext.Replace("\"", string.Empty).Trim();

    Environment.SetEnvironmentVariable("AppDbContext", appDbContext);
    Environment.SetEnvironmentVariable("DataContext", dataContext);

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

    builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;  // Disable email confirmation
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            // options.Lockout.MaxFailedAccessAttempts = 10;    // Increase to prevent lockout during testing
            // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Adjust lockout time
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


    // Configure Swagger middleware
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;  // .NET 10
    });
    builder.Services.AddControllers();

    // ==========

    // Configure web application
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi("/openapi/v1.json");

        // Configure Swagger middleware
        app.UseSwaggerUi(options =>
        {
            options.DocumentTitle = "BlazorShortUrlApi";
            options.Path = "/swagger";
            options.DocumentPath = "/openapi/v1.json";
            options.DocExpansion = "list";
        });

        app.UseMigrationsEndPoint();
        // Migrate Data
        Task.Run(async () =>
        {
            await MigrateDataAsync(app);
        }).Wait();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);

        // NOTE: HSTS does not seem to work with Traefik
        // HTTP Strict Transport Security Protocol (HSTS)
        // The browser forces all communication over HTTPS.
        // The default HSTS value is 30 days.
        // You may want to change this for production scenarios (https://aka.ms/aspnetcore-hsts).
        // app.UseHsts();
    }

    //
    // Configure request pipeline (middleware).
    //
    // Keep Lightweight Middleware Early.
    // Middleware registered earlier executes for every request.
    // Each middleware has a specific responsibility and changing the order
    // can affect both functionality and performance.

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseForwardedHeaders();
    // app.UseHttpsRedirection();
    app.UseAntiforgery();

    // Write streamlined request completion events rather than verbose from the framework.
    // app.UseSerilogRequestLogging();

    // The ordering is important here
    app.UseAuthentication();
    app.UseAuthorization();
    // app.UseRateLimiter();

    app.MapControllers();
    app.MapStaticAssets();

    app.MapRazorComponents<BlazorShortUrl.Components.App>()
        .AddInteractiveServerRenderMode()
        .AllowAnonymous();

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();

    // Add Identity endpoints
    app.MapGroup("/api/account")
        .MapIdentityApi<ApplicationUser>();

    app.Run();

    Log.Information("Application stopped cleanly");
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
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
