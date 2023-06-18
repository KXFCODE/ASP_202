using System;
using System.Security.Claims;
using ASP_202.Data;
using ASP_202.Data.Entity;
using Microsoft.Extensions.Logging;

namespace ASP_202.Middleware
{
	public class SessionAuthMiddleware
    {
        // формирование цепи осуществляется путём того, что каждое звено вызывает следующее
        // відомості про послідовність формуються у Program.cs, а кожен обʼєкт отримує
        // посилання на наступну ланку _next через конструктор

        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<SessionAuthMiddleware> logger,
            DataContext dataContext)
        {
            String? userId =  context.Session.GetString("authUserId");
            if(userId is not null)
            {
                try
                {
                   User? user = dataContext.Users.Find(Guid.Parse(userId));
                    if(user is not null)
                    {
                        context.Items.Add("authUser", user);
                        //
                        Claim[] claims = new Claim[]
                        {
                            new Claim(ClaimTypes.Sid, userId), // Secure ID
                            new Claim(ClaimTypes.Name, user.RealName),
                            new Claim(ClaimTypes.NameIdentifier, user.Login),
                            new Claim(ClaimTypes.UserData, user.Avatar ?? String.Empty)
                        };
                        // Из набора утвержений строится владелец (Principal)
                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                claims,
                                nameof(SessionAuthMiddleware)));
                        //Вiдомостi...
                        context.User = principal;
                    }
                }
                catch(Exception ex)
                {
                    logger.LogWarning(ex, "SessionAuthMiddleware");
                }

            }
            logger.LogInformation("SessionAuthMiddleware works");

            await _next(context);
        }
            // стара(синхронна) схема
            // public void Invoke(HttpContext context) { _next(context); }
    }

    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }

}

