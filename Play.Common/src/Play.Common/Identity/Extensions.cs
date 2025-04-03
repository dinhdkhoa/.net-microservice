using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Play.Common.Identity
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services){
            return services.ConfigureOptions<ConfigureJWtBearerOptions>()
                            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer();
        }
    }
}