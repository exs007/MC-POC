using MS.DataAccess.Context;

namespace MS.DataAccess.Setup
{
    public abstract class Entity
    {
    }

    public abstract class Entity<T> : Entity where T : IDataContext
    {
    }
}