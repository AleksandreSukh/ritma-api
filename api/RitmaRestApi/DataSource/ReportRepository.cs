using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using RitmaRestApi.Models;

namespace RitmaRestApi.DataSource
{
    public class ReportRepository : IReportRepository
    {
        private readonly IReportsDbContext _context;

        public ReportRepository(Func<IReportsDbContext> contextProvider)
        {
            this._context = contextProvider.Invoke();
        }
        public ReportRepository(IReportsDbContext context)
        {
            this._context = context;
        }

        public void AddReport(RitmaResult ritmaResult)
        {
            ritmaResult.DateUtc = DateTime.UtcNow;
            _context.Reports.Add(ritmaResult);
            _context.SaveChanges();
        }

        public IQueryable<RitmaResult> GetAllAQueryable()
        {
            return _context.Reports.AsQueryable();
        }

        public LinkedResourceCollection<RitmaResult> GetAll()
        {
            var lrc = new LinkedReportCollection();
            foreach (var rep in _context.Reports)
            {
                lrc.Add(rep);
            }
            return lrc;
        }

        public ApplicationUser GetUser(string userName)
        {
            return _context.UsersQueriable.FirstOrDefault(
            u => u.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
        }


        public bool DeleteReport(RitmaResult rep)
        {
            _context.Reports.Remove(rep);
            return _context.SaveChanges() > 0;
        }

        public IdentityResult CreateUser(string userName, string password, string email, string roleName)
        {
            var res = _context.DbContext.CreateUser(
                userName: userName,
                password: password,
                roleName: roleName,
                email: email);
            if (res.Succeeded)
            {
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            return res;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

}