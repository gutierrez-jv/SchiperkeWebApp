using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var staffOnlyPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin", "Staff")
                .Build();

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter(staffOnlyPolicy));
            });

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();


            builder.Services.AddDbContext<SchiperkeDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SchiperkeConnection")));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPetRepository, PetRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IConsultationRecordRepository, ConsultationRecordRepository>();
            builder.Services.AddScoped<IVaccinationRecordRepository, VaccinationRecordRepository>();
            builder.Services.AddScoped<IWellnessRecordRepository, WellnessRecordRepository>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPetService, PetService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IConsultationRecordService, ConsultationRecordService>();
            builder.Services.AddScoped<IVaccinationRecordService, VaccinationRecordService>();
            builder.Services.AddScoped<IWellnessRecordService, WellnessRecordService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
