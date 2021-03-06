﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTracker.API.Data.Repositories;
using TravelTracker.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using TravelTracker.API.Helpers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace TravelTracker.API
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
            services.AddDbContext<TravelTrackerDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddJsonOptions(options => 
            {
                options.SerializerSettings.Formatting =Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling=ReferenceLoopHandling.Ignore;
            });                       
            services.AddCors();
            services.AddAutoMapper(typeof(AutoMapperProfiles));
            services.AddScoped<IAuthenticationRepository,AuthenticationRepository>();
            services.AddScoped<IFlightRepository,FlightRepository>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<IJWTTokenBuilder,JWTTokenBuilder>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer
                (options=>{
                    var signingKey = Configuration.GetSection("Jwt:SigningSecret").Value;
                    options.TokenValidationParameters = new TokenValidationParameters{
                        ValidateIssuer=false,
                        ValidateAudience=false,
                        ValidateIssuerSigningKey=true,
                        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                        
                    };
                });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
