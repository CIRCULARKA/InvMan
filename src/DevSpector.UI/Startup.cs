using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using DevSpector.Database;
using DevSpector.Domain.Models;
using DevSpector.Application;
using DevSpector.UI.Filters;
using DevSpector.UI.Validators;

namespace DevSpector.UI
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
            services.AddDbContext<ApplicationDbContext>(
                options =>
                    // options.UseSqlServer(
                    //     Configuration["ConnectionString"]
                    // )
                    options.UseSqlite("Data Source=../DevSpector.Database/DevSpector.Server.db")
            );

            services.AddControllers().AddFluentValidation();

            services.AddIdentity<ClientUser, IdentityRole>().
                AddRoles<IdentityRole>().
                AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddTransient<IValidator<Device>, DeviceValidator>();
            services.AddTransient<AuthorizationFilter>();

            services.AddApplicationServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var context = GetService<ApplicationDbContext>(app);
            var usersManager = GetService<ClientUsersManager>(app);

            app.AddUserGroups(context);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.FillDbWithTemporaryData(context, usersManager);
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private T GetService<T>(IApplicationBuilder builder) =>
            builder.ApplicationServices.CreateScope().
                ServiceProvider.
                    GetService<T>();
    }
}
