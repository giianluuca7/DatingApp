using System.Text;
using Api.Entities;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<AppUser>(opt => {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding
                    .UTF8.GetBytes(config["TokenKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization(op => 
            {
                op.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                op.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
    }
}