using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using RitmaRestApi.Models;

namespace RitmaRestApi.DataSource
{
    public interface IReportRepository : IDisposable
    {
        void AddReport(RitmaResult ritmaResult);
        IdentityResult CreateUser(string userName, string password, string email, string roleName);
        bool DeleteReport(RitmaResult rep);
        LinkedResourceCollection<RitmaResult> GetAll();
        IQueryable<RitmaResult> GetAllAQueryable();
        ApplicationUser GetUser(string userName);
    }
}