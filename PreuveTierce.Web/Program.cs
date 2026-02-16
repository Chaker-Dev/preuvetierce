using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services;
using PreuveTierce.Web.Services.Interfaces;
using Serilog;

namespace PreuveTierce.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Serilog Config
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.Seq("http://localhost:5341")
                        .CreateBootstrapLogger();

            try
            {
                Log.Information("Démarrage de PreuveTierce...");

                var builder = WebApplication.CreateBuilder(args);

                // QuestPDF config
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                // Firebase Conf
                string path = Path.Combine(builder.Environment.ContentRootPath, "firebase-auth.json");
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

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

                builder.Host.UseSerilog((context, services, configuration) => configuration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext());

                builder.Services.AddControllersWithViews();

                // Others services
                builder.Services.AddScoped<ICertificationService, CertificationService>();
                builder.Services.AddTransient<IQrCodeService, QrCodeService>();
                builder.Services.AddTransient<IPdfGeneratorService, PdfGeneratorService>();
                builder.Services.AddTransient<IEmailSender, BrevoEmailSender>();
                builder.Services.AddScoped<IFileHasherService, FileHasherService>();
                builder.Services.AddScoped<IAuditService, AuditService>();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseSerilogRequestLogging();
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

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
