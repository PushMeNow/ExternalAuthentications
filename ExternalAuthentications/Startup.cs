using ExternalAuthentications.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                //.AddOAuth("GitHub", "GitHub", configs =>
                //{
                //    configs.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                //    configs.TokenEndpoint = "https://github.com/login/oauth/access_token";
                //    configs.UserInformationEndpoint = "https://api.github.com/user";
                //    configs.SaveTokens = true;
                //    configs.ClientId = Configuration["Authentication:GitHub:ClientId"];
                //    configs.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                //    configs.CallbackPath = new PathString("/account/externallogincallback");
                //    configs.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

                //    configs.Scope.Add("user");
                //});
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                    options.Scope.Add("user");
                    //options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    //options.CallbackPath = new PathString("/account/externallogincallback");
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
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}