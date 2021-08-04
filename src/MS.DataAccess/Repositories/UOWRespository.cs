using System.Threading;
using System.Threading.Tasks;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Repositories
{
    public class UOWRespository<T> : GenericRepository<T> where T : Entity
    {
        public UOWRespository(IDataContext context) : base(context)
        {
        }

        protected override Task<int> SaveChangesAsync(CancellationToken? cancellationChangeToken)
        {
            // do nothing, UOW is responsible for saving 
            return Task.FromResult(default(int));
        }

        protected override int SaveChanges()
        {
            // do nothing, UOW is responsible for saving
            return default;
        }
    }
}