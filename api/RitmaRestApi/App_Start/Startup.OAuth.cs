using System;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using RitmaRestApi.Adapters;
using RitmaRestApi.DataSource;
using RitmaRestApi.Helpers;
using TextLoggerNet.Interfaces;
using Thinktecture.IdentityModel.Tokens;

namespace RitmaRestApi
{
    //see http://www.developerhandbook.com/c-sharp/create-restful-api-authentication-using-web-api-jwt/
    // OAuth secrets
    public partial class Startup
    {
        public void ConfigureOAuth(IAppBuilder app, bool debugMode, ApiConfig config)
        {
            var issuer = config.Issuer;
            var secret = TextEncodings.Base64Url.Decode(config.Secret);

            //Uses JWT bearer tokens
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = config.AllowInsecureHttp,
                TokenEndpointPath = new PathString(config.TokenEndpointPath),
                AccessTokenExpireTimeSpan = debugMode ? TimeSpan.FromDays(1) : TimeSpan.FromMinutes(config.AccessTokenExpireTimeMinutes),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(issuer)
            });

            //Consume the JWT bearer tokens
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { "Any" },
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                {
                    //Todo:ბოლოს ესეც ვნახო რა მოხელეა
                    new SymmetricKeyIssuerSecurityTokenProvider(issuer, secret)
                }
            });

        }

    }

    static class Ext
    {
        public static IReportsDbContext EnsureInitialDataExists(this IReportsDbContext context)
        {


            if (Debugger.IsAttached)
            {
                try
                {

                    var userName = "admin";
                    var roleName = "Admin";
                    var email = "admin@admin.com";


                    if (context.UsersQueriable.Any() && context.UsersQueriable.Any(u => u.UserName == userName)) return context;
                    //Console.WriteLine("Enter Password for admin account");
                    //var password = SecureStringPassword.GetPasswordCheckReenter(p => p.Length > 8).ToBasicString();
                    //throw new NotImplementedException();
                    var password = "asddqwee1233";

                    var usrCreationResult = context.DbContext.CreateUser(userName, roleName, email, password);
                    if (usrCreationResult.Succeeded)
                        context.SaveChanges();
                    else
                    {
                        Console.WriteLine(string.Join(Environment.NewLine, usrCreationResult.Errors));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return context;
        }
    }

    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        private ILogger logger = DependencyRepository.Instance.Logger;
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            logger.WriteLine($"Oauth token request: UserName:{context.UserName};Password:{context.Password};ClientId:{context.ClientId}");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            var owinContext = context.OwinContext.Get<IReportsDbContext>();
            var users = owinContext.UsersQueriable;
            var user = users.FirstOrDefault(u => u.UserName == context.UserName);
            if (!context.OwinContext.Get<ReportsUserManager>().CheckPassword(user, context.Password))
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            var ticket = new AuthenticationTicket(SetClaimsIdentity(context, user), new AuthenticationProperties());
            context.Validated(ticket);

            return Task.FromResult<object>(null);
        }
        /*
         As we’re not checking the audience, when ValidateClientAuthentication is called we can just validate the request. When the request has a grant_type of password, which all our requests to the OAuth endpoint will have, the above GrantResourceOwnerCredentials method is executed. This method authenticates the user and creates the claims to be added to the JWT.
         */
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        private static ClaimsIdentity SetClaimsIdentity(OAuthGrantResourceOwnerCredentialsContext context, IdentityUser user)
        {
            var identity = new ClaimsIdentity("JWT");
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));

            var userRoles = context.OwinContext.Get<ReportsUserManager>().GetRoles(user.Id);
            foreach (var role in userRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return identity;
        }
    }

    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private static readonly byte[] _secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["secret"]);
        private readonly string _issuer;

        public CustomJwtFormat(string issuer)
        {
            _issuer = issuer;
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var signingKey = new HmacSigningCredentials(_secret);
            var issued = data.Properties.IssuedUtc;
            var expires = data.Properties.ExpiresUtc;
            //TODO:ეს Any გავარკვიო
            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_issuer, "Any", data.Identity.Claims, issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingKey));
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}