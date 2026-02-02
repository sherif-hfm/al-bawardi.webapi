using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace janaez.webapi
{
    public class BasicAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        
        
        public BasicAuthAttribute() {
            
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var settings = context.HttpContext.RequestServices.GetService<IOptions<BasicAuthSettings>>().Value;

            var request = context.HttpContext.Request;

            if (!request.Headers.ContainsKey("Authorization"))
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            var authHeader = request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            string decoded;
            try
            {
                var bytes = Convert.FromBase64String(encodedCredentials);
                decoded = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            var parts = decoded.Split(':', 2);
            if (parts.Length != 2)
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }

            if (parts[0] != settings.Username || parts[1] != settings.Password)
            {
                context.Result = new UnauthorizedResult();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
