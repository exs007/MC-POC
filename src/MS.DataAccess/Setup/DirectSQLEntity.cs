using MS.DataAccess.Context;

namespace MS.DataAccess.Setup
{
    public abstract class DirectSQLEntity<T> : Entity<T> where T : IDataContext
    {
    }
}