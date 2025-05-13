using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Play.Common.Settings;

namespace Play.Common.Identity
{
    public class ConfigureJWtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly string AccessTokenParameter = "access_token";
        private readonly string MessageHubPath = "/messageHub";

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

            options.Events = new JwtBearerEvents{
                OnMessageReceived = context => {
                    var accessToken = context.Request.Query[AccessTokenParameter];
                    var path = context.HttpContext.Request.Path;

                    if(path.StartsWithSegments(MessageHubPath) && !string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;   
                }
            };
        }

        public void Configure(JwtBearerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}