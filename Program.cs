using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Security.Claims;
using System.Threading.RateLimiting;
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
                .RequireRole("Admin", "Staff", "Veterinarian")
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
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async context =>
                        {
                            var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                            if (!int.TryParse(userIdValue, out var userId))
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                                return;
                            }

                            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                            var user = await userService.GetByIdAsync(userId);
                            if (user is null)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            }
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("LoginLimiter", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetRateLimitPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));

                options.AddPolicy("PublicBookingLimiter", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetRateLimitPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 6,
                            Window = TimeSpan.FromMinutes(5),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));

                options.AddPolicy("PublicLookupLimiter", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetRateLimitPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            });


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
            app.UseRateLimiter();
            app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }

        private static string GetRateLimitPartitionKey(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString()
                ?? httpContext.User.Identity?.Name
                ?? "anonymous";
        }
    }
}
