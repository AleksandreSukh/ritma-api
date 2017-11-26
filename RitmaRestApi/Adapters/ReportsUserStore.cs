using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RitmaRestApi.Adapters
{
    public class ReportsUserStore : UserStore<IdentityUser>
    {
        public ReportsUserStore(DbContext context) : base(context)
        {
        }
    }

}
