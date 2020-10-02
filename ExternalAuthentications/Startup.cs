using AspNet.Security.OAuth.GitHub;
using ExternalAuthentications.DataAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ExternalAuthentications
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            //services.AddRazorPages();

            services.AddDbContext<CurrentDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Database"));
            }, ServiceLifetime.Transient);

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<CurrentDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddJwtBearer(options => {
                    options.Configuration.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.Configuration.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.Configuration.UserInfoEndpoint = "https://api.github.com/user";
                    options.Configuration.ScopesSupported.Add("code");
                    options.SaveToken = true;
                    options.Configuration.
                })
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                    options.CallbackPath = new PathString("/account/externallogincallback");
                    options.TokenEndpoint = "https://cors-anywhere.herokuapp.com/https://github.com/login/oauth/access_token";
                    //options.Events = new OAuthEvents
                    //{
                    //    OnCreatingTicket = async context =>
                    //    {
                    //        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    //        request.Headers.Add("Access-Control-Allow-Origin", "https://localhost:5000"); 
                    //        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    //        response.EnsureSuccessStatusCode();
                    //        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    //        context.RunClaimActions(json.RootElement);
                    //    }
                    //};
                });

            services.AddMvc();
            //services.AddCors(opts =>
            //{
            //    opts.AddDefaultPolicy(policy =>
            //    {
            //        policy.AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader();
            //    });
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            //app.UseCors();

            app.UseRouting();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}