using MCMV.Logical;
using MCMV.Data;

namespace MCMV
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<MCMV.Data.Database>();
            builder.Services.AddScoped<MCMV.Logical.LoginService>();
            builder.Services.AddScoped<MCMV.Logical.RegisterService>();
            builder.Services.AddScoped<MCMV.Logical.DonationService>();

            var app = builder.Build();

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
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}