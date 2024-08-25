using Palmalytics;
using Palmalytics.SqlServer;
using Serilog;
using Serilog.Events;

namespace TestSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Palmalytics services
            builder.Services.AddPalmalytics(options =>
            {
                options.DashboardOptions.ShowExceptionDetails = true;

                options.UseSqlServer(new SqlServerOptions
                {
                    ConnectionString = "Server=localhost;Database=Palmalytics;Integrated Security=true;TrustServerCertificate=true"
                });
            });

            // Add file logging with Serilog
            builder.Logging.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Palmalytics", LogEventLevel.Debug)
                .WriteTo.File("Logs.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}")
                .CreateLogger());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Add Palmalytics middleware
            app.UsePalmalytics();

            app.Run();
        }
    }
}