using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Play.Common.Settings;

namespace Play.Common.Identity
{
    public class ConfigureJWtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IConfiguration configuration;
        public ConfigureJWtBearerOptions(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void Configure(string name, JwtBearerOptions options)
        {
            if(name == JwtBearerDefaults.AuthenticationScheme){
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                options.Authority = serviceSettings.Authority;
                options.Audience = serviceSettings.ServiceName;
                options.MapInboundClaims = false; //remove legacy claims mapping
                options.TokenValidationParameters = new TokenValidationParameters{
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}