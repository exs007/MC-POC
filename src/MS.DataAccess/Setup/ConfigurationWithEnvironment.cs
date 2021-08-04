using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MS.DataAccess.Setup
{
    public abstract class ConfigurationWithEnvironment<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        public ConfigurationWithEnvironment(string environment)
        {
            Environment = environment;
        }

        protected string Environment { get; }

        public abstract void Configure(EntityTypeBuilder<TEntity> builder);
    }
}