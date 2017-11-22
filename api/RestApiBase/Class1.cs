using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace RestApiBase
{
    public interface IDataRepository
    {
        IdentityResult CreateUser(string userName, string password, string email, string roleName);
        ApplicationUser GetUser(string userName);
    }
}
