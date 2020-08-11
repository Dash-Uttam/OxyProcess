using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxyProcess.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OxyProcess.Models;
using OxyProcess.Services;
using OxyProcess.Models.SendEmail;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OxyProcess.Interface;
using AutoMapper;
using System.Linq;
using OxyProcess.Helpers;
using System.Collections.Generic;

namespace OxyProcess
{
    public class Startup
    {
       
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

         
            //services.AddAutoMapper();
            //services.AddAutoMapper(new Type[] { typeof(Mappers.AutoMapper) });
            services.AddOptions();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            //var results = Array.FindAll(ConnString, s => s.Contains("Password"))[0].Split("Password=").FirstOrDefault(e=>e != "");

            //string Password =EncryptDecryptHelper.Decrypt(results);

            //var l = Array.FindAll(ConnString, s => s.Contains("Password"));

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(GetConnectionString()));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            //services.TryAddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEmailSender, EmailSender>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(300);

                //options.LoginPath = "/Identity/Account/Login";
                //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            //services.AddMvc()
            //   .AddControllersAsServices();
            //// Add application services.
            //services.AddTransient<IEmailSender, EmailSender>();           
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAutoMapper(typeof(Startup));


        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider src)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {


                routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
            });
            

            app.UseCookiePolicy();
            using (var serviceScope = app.ApplicationServices
          .GetRequiredService<IServiceScopeFactory>()
          .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
            CreateRoles(src);

        }


        private void CreateRoles(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Task<IdentityResult> roleResult;
            //string email = "someone@somewhere.com";

            //Check that there is an Administrator role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync("SuperAdmin");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SuperAdmin" });

                roleResult.Wait();
            }

            hasAdminRole = roleManager.RoleExistsAsync("Admin");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new ApplicationRole { Name = "Admin", NormalizedName = "Admin" });

                roleResult.Wait();
            }


            hasAdminRole = roleManager.RoleExistsAsync("Worker");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new ApplicationRole { Name = "Worker", NormalizedName = "Worker" });

                roleResult.Wait();
            }


            hasAdminRole = roleManager.RoleExistsAsync("Customer");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new ApplicationRole { Name = "Customer", NormalizedName = "Customer" });

                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the Administrator role

            // Task<ApplicationUser> testUser = userManager.FindByEmailAsync(email);
            //testUser.Wait();

            //if (testUser.Result == null)
            //{
            // ApplicationUser administrator = new ApplicationUser();
            // administrator.Email = email;
            // administrator.UserName = email;

            // Task<IdentityResult> newUser = userManager.CreateAsync(administrator, "_AStrongP@ssword!");
            // newUser.Wait();

            // if (newUser.Result.Succeeded)
            // {
            // Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(administrator, "Administrator");
            // newUserRole.Wait();
            // }
            //}

        }

        /// <summary>
        /// Get ConnectionString
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {

            string[] ConnString = Configuration.GetConnectionString("DefaultConnection").Split(";");

            foreach (var d in ConnString.Select((value, i) => (value, i)))
            {
                if (d.value.Contains("Password"))
                {

                    var results = d.value.Split("Password=").FirstOrDefault(e => e != "");
                    string Password = EncryptDecryptHelper.Decrypt(results);
                    ConnString[d.i] = Array.FindAll(ConnString, s => s.Contains("Password"))[0].Split("Password=")[1] = ("Password=" + Password);
                }


            }
            return string.Join(";", ConnString); ;

        }
    }
}