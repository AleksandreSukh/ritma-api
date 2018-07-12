using Microsoft.AspNet.Identity;

namespace RestApiBase
{
    public interface IDataRepository
    {
        IdentityResult CreateUser(string userName, string password, string email, string roleName);
        ApplicationUser GetUser(string userName);
    }
}
