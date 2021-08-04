using Microsoft.Extensions.Options;
using MS.Core.Options;
using Serilog;

namespace MS.DataAccess.Setup
{
    /// <summary>
    /// Net Core environment checker which is based on app settings
    /// </summary>
    public class CoreEnvironmentResolver : BaseEnvironmentResolver
    {
        public CoreEnvironmentResolver(IOptions<CommonOptions> commonOptions, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(commonOptions.Value?.Environment))
            {
                Environment = commonOptions.Value.Environment;
                return;
            }

            logger.Error(
                "No 'Environment' setting in configs. Please Add 'Common.Environment' setting in configs!");
        }
    }
}