using _10xVibeTravels.Components;
using _10xVibeTravels.Components.Account;
using _10xVibeTravels.Data;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Services;
using Blazored.Toast;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace _10xVibeTravels
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            builder.Services.AddHttpClient();
            /*builder.Services.AddHttpClient<_10xVibeTravels.Services.OpenRouterService>((sp, client) => // Use fully qualified name if any ambiguity
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["OpenRouter:BaseUrl"]; // Should be "https://openrouter.ai/api/v1"
    var apiKey = configuration["OpenRouter:ApiKey"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        // Consider logging this error as well
        throw new InvalidOperationException("OpenRouter:BaseUrl is not configured in appsettings.json.");
    }
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        // Consider logging this error
        throw new InvalidOperationException("OpenRouter:ApiKey is not configured in appsettings.json.");
    }

    client.BaseAddress = new Uri(baseUrl); // BaseAddress will be "https://openrouter.ai/api/v1"
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
})*/;

// You still need to register the service itself, e.g.:
builder.Services.AddScoped<_10xVibeTravels.Services.OpenRouterService>();

// The Configure<OpenRouterSettings> can remain if you want to use IOptions<OpenRouterSettings> elsewhere,
// but OpenRouterService will not use it directly.
// Ensure the section name matches your appsettings.json ("OpenRouter").
builder.Services.Configure<_10xVibeTravels.Models.OpenRouterSettings>(
    builder.Configuration.GetSection("OpenRouter")
);
            builder.Services.AddBlazoredToast();
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Register custom application services
            builder.Services.AddScoped<IPlanGenerationService, PlanGenerationService>();
            builder.Services.AddScoped<IOpenRouterService, OpenRouterService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            // Register dictionary services
            builder.Services.AddScoped<IIntensityService, IntensityService>();
            builder.Services.AddScoped<IInterestService, InterestService>();
            builder.Services.AddScoped<ITravelStyleService, TravelStyleService>();

            // Register Note specific services
            builder.Services.AddScoped<INoteService, NoteService>();

            // Register Plan specific services (using new locations)
            builder.Services.AddScoped<IPlanService, PlanService>();

            // Needed for accessing HttpContext outside of controllers (e.g., in services)
            builder.Services.AddHttpContextAccessor();

            // Add API Controllers support (needed for Swagger endpoint discovery)
            builder.Services.AddControllers();

            // Configure FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // Add Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "10xVibeTravels API", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "10xVibeTravels API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            // Map API controllers
            app.MapControllers();

            app.Run();
        }
    }
}
