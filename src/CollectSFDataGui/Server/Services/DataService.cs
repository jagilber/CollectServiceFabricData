using CollectSFData.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollectSFDataGui.Server.Services
{
    public class DataService : IDataService
    {
        private ILogger<DataService> _logger;

        public DataService(ILogger<DataService> logger)
        {
            _logger = logger;
        }

        public async Task SaveConfiguration(ConfigurationProperties dataOption)
        {
            try
            {
                FileStream createStream = null;

                // Create a folder in local TEMP folder path
                string path = $"{Path.GetTempPath()}\\CollectSFData";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Configuration file
                var fileName = $"{path}\\{dataOption.KustoTable}.json";

                // Create a file to write the configuration option data
                // Update the file with configuration data if it exists
                if (File.Exists(fileName))
                {
                    // Remove the existing file
                    File.Delete(fileName);
                }

                // Create file stream
                createStream = File.Create(fileName);

                // Serialize the data to write as a stream
                var serializerOption = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                await JsonSerializer.SerializeAsync(createStream, dataOption, serializerOption);

                // Dispose the stream
                await createStream.DisposeAsync();

                _logger.LogInformation("Save configuration operation completed @ {0}.", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Save configuration operation failed - {0}", ex);
                throw;
            }
        }

        public async Task<IList<ConfigurationProperties>> GetAllConfiguration()
        {
            IList<ConfigurationProperties> allOptions = new List<ConfigurationProperties>();

            try
            {
                string path = $"{Path.GetTempPath()}\\CollectSFData";
                if (Directory.Exists(path))
                {
                    // Write all JSON files in a collection
                    var configFiles = Directory.EnumerateFiles(path, "*.json");

                    // Enumerate the JSON files
                    // De-serialize each of the JSON file and append results to collection
                    foreach (var fileName in configFiles)
                    {
                        using FileStream openStream = File.OpenRead(fileName);
                        var configOption = await JsonSerializer.DeserializeAsync<ConfigurationOptions>(openStream);
                        allOptions.Add(configOption);

                        // Dispose the stream
                        await openStream.DisposeAsync();
                    }

                    _logger.LogInformation("Get All configuration operation completed for {0}.", path);
                }
                else
                {
                    _logger.LogInformation("Get All configuration operation completed with NO record for {0}.", path);
                }

                return allOptions;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get All configuration operation failed - {0}", ex);
                throw;
            }
            finally
            {
                // Dispose of collection objects reference
                if(allOptions != null)
                {
                    allOptions = null;
                }
            }
            
        }

        public async Task<ConfigurationProperties> PullConfiguration(string kustoTable)
        {
            ConfigurationProperties configOption = null;

            try
            {
                string path = $"{Path.GetTempPath()}\\CollectSFData";
                if (Directory.Exists(path))
                {
                    // Write all JSON files in a collection
                    // Ideally, there should be a single file with that name
                    var configFiles = Directory.EnumerateFiles(path, $"{kustoTable}.json");

                    // Read the file details from config files collection and assume it is first
                    var fileDetail = configFiles.FirstOrDefault();

                    if (!string.IsNullOrEmpty(fileDetail))
                    {
                        // Read the JSON file
                        // De-serialize the JSON file and return the result
                        using FileStream openStream = File.OpenRead(fileDetail);
                        configOption = await JsonSerializer.DeserializeAsync<ConfigurationOptions>(openStream);

                        // Dispose the stream
                        await openStream.DisposeAsync();
                    }

                    _logger.LogInformation("Pull configuration operation completed for {0}\\{1}.json.", path, kustoTable);
                }
                else
                {
                    _logger.LogInformation("Pull configuration operation completed with NO record for {0}\\{1}.json.", path, kustoTable);
                }

                return configOption;
            }
            catch (Exception ex)
            {
                _logger.LogError("Pull configuration operation failed - {0}", ex);
                throw;
            }
        }

        public Task<bool> DeleteConfiguration(string kustoTable)
        {
            try
            {
                bool deleteFlag = false;

                // Check if directory exists
                string path = $"{Path.GetTempPath()}\\CollectSFData";
                if (Directory.Exists(path))
                {
                    // Configuration file
                    var fileName = $"{path}\\{kustoTable}.json";

                    // Delete the file if exists
                    if (File.Exists(fileName))
                    {
                        // Remove the existing file
                        File.Delete(fileName);

                        // Set the flag indicating delete operation completed
                        deleteFlag = true;                        
                    }

                    _logger.LogInformation("Delete configuration operation completed with {0} for {1}.", deleteFlag, fileName);
                }                

                return Task.FromResult(deleteFlag);
            }
            catch (Exception ex)
            {
                _logger.LogError("Delete configuration operation failed - {0}", ex);
                throw;
            }
        }
    }
}
