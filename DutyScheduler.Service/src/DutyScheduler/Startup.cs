﻿using System.Net;
using System.Threading.Tasks;
using DutyScheduler.Helpers;
using DutyScheduler.Models;
using DutyScheduler.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using DutyScheduler.Middlewares;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;

namespace DutyScheduler
{
    public class Startup
    {
        
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets();

            Configuration = builder.Build();

            // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
            // builder.AddUserSecrets();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var pathToDoc = Configuration["Swagger:Path"];

			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy", builder => builder
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials());
			});

			services.AddMvc();
			services.Configure<MvcOptions>(options => options.Filters.Add(new CorsAuthorizationFilterFactory("CorsPolicy")));

            // Use a PostgreSQL database
            var sqlConnectionString = Configuration.GetConnectionString("Postgres");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    sqlConnectionString,
                    b => b.MigrationsAssembly("DutyScheduler")
                )
            );

            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Duty Scheduler API",
                    Description = "Duty Scheduler API",
                    TermsOfService = "None"
                });
                options.IncludeXmlComments(pathToDoc);
                options.DescribeAllEnumsAsStrings();
            });


            services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 4;
                o.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") &&
                            ctx.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvcCore();

            services.AddOptions();

            services.Configure<SmtpClientConfiguration>(Configuration.GetSection("SMTP"));

            //services.AddTransient<IEmailSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

			app.UseCors("CorsPolicy");

			DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);
            app.UseStaticFiles();

            app.UseIdentity().UseCookieAuthentication();
            //app.UseMiddleware<FillResponseBodyMiddleware>();
            app.UseWebSockets();
            app.UseWebSocketHandler();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=home}/{action=index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUi();

            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                         .Database.Migrate();

                    var userManager = app.ApplicationServices.GetService<UserManager<User>>();
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>().EnsureSeedData(userManager, DefaultAdmin(), Configuration["Admin:Password"]);
                }
            }
            catch { }
        }

        private RegisterViewModel DefaultAdmin()
        {
            return new RegisterViewModel()
            {
                UserName = Configuration["Admin:Username"],
                Name = Configuration["Admin:Username"],
                LastName = Configuration["Admin:LastName"],
                Password = Configuration["Admin:Password"],
                Email = Configuration["Admin:Email"],
                Phone= Configuration["Admin:Phone"],
                Office = Configuration["Admin:Office"]
        };
        }
    }
}