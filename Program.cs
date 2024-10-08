using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using IPassM.Components;
using IPassM.Entities;
using IPassM.Services;

namespace IPassM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    options.SlidingExpiration = true;
                    options.AccessDeniedPath = "/Forbidden/";
                }); ;
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<UserService>();
            //builder.Services.AddScoped<CsvService>();

            var app = builder.Build();

            SeedDefaultUser();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            });

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        private static void SeedDefaultUser()
        {
            string directoryPath = "Credentials";
            string filePath = Path.Combine(directoryPath, "UserCredential.json");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                List<UserCredentials> defaultUsers =
                [
                    new() {
                        Id = Guid.NewGuid(),
                        Username = "superadmin@gmail.com",
                        Password = "superadmin",
                        Name = "Superadmin"
                    }
                ];
                string json = JsonSerializer.Serialize(defaultUsers, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
        }
    }
}
