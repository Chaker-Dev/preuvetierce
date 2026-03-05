using Google.Cloud.Firestore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services;
using PreuveTierce.Web.Services.Interfaces;
using QuestPDF.Drawing;
using Serilog;
using System.Globalization;

namespace PreuveTierce.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Information("Démarrage de PreuveTierce...");

                var builder = WebApplication.CreateBuilder(args);

                var supportedCultures = new[] { new CultureInfo("fr-FR") };

                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("fr-FR");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                string logPath;

                if (builder.Environment.IsProduction())
                {
                    logPath = "/var/log/preuvetierce/log-.txt";
                }
                else
                {
                    logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "log-.txt");
                }
                // Logging config
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug() 
                    .WriteTo.Console()
                    .WriteTo.File(logPath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                    .CreateBootstrapLogger();

                // QuestPDF config
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                // Firebase Conf
                string firebasePath = Path.Combine(builder.Environment.ContentRootPath, "firebase-auth.json");
                if (!string.IsNullOrEmpty(firebasePath))
                {
                    if (File.Exists(firebasePath))
                    {
                        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
                    }
                }
                    builder.Services.AddSingleton<FirestoreDb>(s =>
                {
                    return FirestoreDb.Create("preuvetierce");
                });

                // Add services to the container.
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredUniqueChars = 1;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<FrenchIdentityErrorDescriber>();

                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.LoginPath = "/Identity/Account/Login";
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                });

                builder.Host.UseSerilog((context, services, configuration) => configuration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext());



                builder.Services.AddControllersWithViews();

                // Others services
                builder.Services.AddScoped<ICertificationService, CertificationService>();
                builder.Services.AddTransient<IQrCodeService, QrCodeService>();
                builder.Services.AddTransient<IPdfGeneratorService, PdfGeneratorService>();
                builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
                builder.Services.AddScoped<IFileHasherService, FileHasherService>();
                builder.Services.AddScoped<IAuditService, AuditService>();
                builder.Services.AddHttpClient<ITimestampService, Rfc3161TimestampService>();
                var app = builder.Build();
                
               
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                    var fontPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "fonts",
                        "DejaVuSans.ttf"
                    );
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                    var fontPath = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";

                    FontManager.RegisterFont(
                        File.OpenRead(fontPath)
                    );
                }
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseSerilogRequestLogging();
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseRequestLocalization();
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Le serveur a rencontré une erreur fatale au démarrage");
            }
            finally
            {
                Log.Information("Arrêt du serveur...");
                Log.CloseAndFlush(); 
            }
        }
    }
}
