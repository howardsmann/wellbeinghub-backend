using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using AutoMapper;
using WellbeingHub.Profiles;
using WellbeingHub.Validators;

namespace WellbeingHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddFluentValidation();
            services.AddAutoMapper(typeof(UserProfile));

            services.AddTransient<IValidator<Controllers.UserDto>, UserDtoValidator>();
            services.AddTransient<IValidator<Controllers.LoginRequest>, LoginRequestValidator>();
            services.AddTransient<IValidator<Controllers.GroupDto>, GroupDtoValidator>();
            services.AddTransient<IValidator<Controllers.MarketplaceItemDto>, MarketplaceItemDtoValidator>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            var cosmosEndpoint = Configuration["CosmosDb:Account"] ?? Configuration["CosmosDb__Account"];
            var key = Configuration["CosmosDb:Key"] ?? Configuration["CosmosDb__Key"];

            if (string.IsNullOrWhiteSpace(cosmosEndpoint))
                throw new System.Exception("CosmosDb:Account not configured. Set CosmosDb__Account env var.");

            CosmosClient cosmosClient;
            if (!string.IsNullOrEmpty(key) && Environment.IsDevelopment())
            {
                cosmosClient = new CosmosClient(cosmosEndpoint, key);
            }
            else
            {
                var credential = new DefaultAzureCredential();
                cosmosClient = new CosmosClient(cosmosEndpoint, credential);
            }

            services.AddSingleton(cosmosClient);
            services.AddSingleton<DataStore>();

            var jwtKey = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"] ?? Configuration["Jwt__Key"] ?? "ChangeThisDevOnlyKey");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WellbeingHub API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WellbeingHub API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
