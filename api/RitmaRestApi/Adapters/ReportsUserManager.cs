using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RitmaRestApi.Adapters
{
    public class ReportsUserManager : UserManager<IdentityUser>
    {
        public ReportsUserManager(ReportsUserStore userStore) : base(userStore)
        {
        }
    }
}