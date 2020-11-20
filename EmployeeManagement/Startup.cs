using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement
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
            services.AddDbContextPool<AppDbContext>(
                       options => options.UseSqlServer(Configuration.GetConnectionString("EmployeeDBConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
             {
                 options.Password.RequiredLength = 6;
                 options.Password.RequiredUniqueChars = 1;
             }).AddEntityFrameworkStores<AppDbContext>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
              options.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role")
                                    .RequireClaim("Create Role")
                                    );
                options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                      context.User.IsInRole("Admin") &&
                      context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                      context.User.IsInRole("Super Admin")
                                    ));
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                    {
                        options.ClientId = "715438474958-d564e63puo8onp5t80epsoh6q3cl36rb.apps.googleusercontent.com";
                        options.ClientSecret = "6b2H-DMAfhPt4rxjj8HcJv6X";
                    })
              .AddFacebook(options =>
                    {
                        options.AppId = "669302937217226";
                        options.AppSecret = "a531f47c2663cb1e25425990dfaba287";
                    });
            

            // services.AddMvc().AddXmlSerializerFormatters();
            services.AddScoped <IEmployeeRepository, SqlEmployeeRepository>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseExceptionHandler("/Error");
                //app.UseHsts();

            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync(Configuration["MyKey"]);
            //});
            //  app.UseMvcWithDefaultRoute();
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{Id?}");
            //});
            app.UseMvc();
        }
    }
}
