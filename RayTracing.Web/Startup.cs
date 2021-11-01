using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RayTracing.CalculationModel.Calculation;
using RayTracing.Web.Models;
using RayTracing.Web.Models.Problems;
using RayTracing.Web.Models.Validators;

namespace RayTracing.Web
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
            services.AddRazorPages()
                .AddSessionStateTempDataProvider()
                .AddNewtonsoftJson();

            services.AddSession();

            services.AddTransient<IRayTracingCalculationService, RayTracingCalculationService>();
            services.AddTransient<IValidator<AcousticProblemDescription>, AcousticProblemDescriptionValidator>();
            services.AddSingleton<CommonProblems>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
