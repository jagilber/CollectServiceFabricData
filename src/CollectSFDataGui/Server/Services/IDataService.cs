using CollectSFData.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollectSFDataGui.Server.Services
{
    public interface IDataService
    {
        /// <summary>
        /// Saves configuration properties details.
        /// Currently: Put the configuration in local temp directory. It works for localhost access.
        /// Next: If app be available in cloud, we need to have the app AAD mapped. 
        /// For that authenticated user, crate a container in blob storage and store the config list created by that user.
        /// </summary>
        /// <param name="dataOption"></param>
        /// <returns></returns>
        Task SaveConfiguration(ConfigurationProperties dataOption);

        /// <summary>
        /// Get all the configuration list created by user earlier.
        /// </summary>
        /// <returns></returns>
        Task<IList<ConfigurationProperties>> GetAllConfiguration();

        /// <summary>
        /// Get the configuration properties details for specific kusto table.
        /// </summary>
        /// <param name="kustoTable"></param>
        /// <returns></returns>
        Task<ConfigurationProperties> PullConfiguration(string kustoTable);

        /// <summary>
        /// Delete the configuration properties for a specified kusto table.
        /// </summary>
        /// <param name="kustoTable"></param>
        /// <returns></returns>
        Task<bool> DeleteConfiguration(string kustoTable);
    }
}
