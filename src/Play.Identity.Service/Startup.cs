using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GreenPipes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exceptions;
using Play.Identity.Service.HostedServices;
using Play.Identity.Service.Settings;

namespace Play.Identity.Service
{
    public class Startup
    {
        private const string AllowedOriginSetting = "AllowedOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
            var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            var identityServiceSettings = Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>(); 

            services.Configure<IdentitySettings>(Configuration.GetSection(nameof(IdentitySettings)))
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddMongoDbStores<ApplicationUser,ApplicationRole, Guid>
                (
                    mongoDbSettings.ConnectionString,
                    serviceSettings.ServiceName
                );

            services.AddMassTransitWithRabbitMq(retryConfigurator => 
            {
                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                retryConfigurator.Ignore(typeof(InsufficientMemoryException));
                retryConfigurator.Ignore(typeof(UnknownUserException));
            });

            services.AddIdentityServer(options => {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.KeyManagement.KeyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryApiScopes(identityServiceSettings.ApiScopes)
                .AddInMemoryApiResources(identityServiceSettings.ApiResources)
                .AddInMemoryClients(identityServiceSettings.Clients)
                .AddInMemoryIdentityResources(identityServiceSettings.IdentityResources);
            services.AddLocalApiAuthentication();
            services.AddControllers();

            services.AddHostedService<IdentitySeedHostedService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Identity.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Identity.Service v1"));

                app.UseCors(builder => {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseCookiePolicy(new CookiePolicyOptions{
                MinimumSameSitePolicy = SameSiteMode.Lax 
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
