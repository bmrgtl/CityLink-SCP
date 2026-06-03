using CityLink_SCP.Common;
using CityLink_SCP.Database;
using CityLink_SCP.DbModels;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CityLink_SCP
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            // MVC + Model Binder for query binding, so normal users can't query userId
            builder.Services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new RestrictedQueryModelBinderProvider());
            });

            // Database 
            var dbPath = Path.Join(Directory.GetCurrentDirectory(), "Database", "CityLinks.db");
			builder.Services.AddDbContext<CityLinksContext>(options => options.UseSqlite($"Data Source={dbPath}"));

			// Identity
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				// Password rules
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = false;

				// Lockout
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;

				// User
				options.User.RequireUniqueEmail = true;
			})
			.AddEntityFrameworkStores<CityLinksContext>()
			.AddDefaultTokenProviders();

			// Auth cookie -> redirect staff to /Admin/Login
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Admin/Login";   // default redirect for [Authorize]
				options.AccessDeniedPath = "/Admin/Login";
				options.SlidingExpiration = true;
				options.ExpireTimeSpan = TimeSpan.FromHours(8);
			});

			// App services 
			builder.Services.AddScoped<DatabaseService>();
			builder.Services.AddScoped<XmlConfigService>();

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<CityLinksContext>();
				var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
				var dbService = services.GetRequiredService<DatabaseService>();
				var xmlService = services.GetRequiredService<XmlConfigService>();

				// EnsureCreated creates the schema if it doesn't exist
				context.Database.EnsureCreated();

				await DbInitialiser.InitialiseAsync(
					context, userManager, roleManager, dbService, xmlService);
			}

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
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

            await app.RunAsync();
		}
	}
}
