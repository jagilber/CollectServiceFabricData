using CollectSFData;
using CollectSFData.Common;
using CollectSFDataGui.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net.Http.Json;
using System.IO;
using CollectSFDataGui.Server.Services;
using System.Threading.Tasks;

namespace CollectSFDataGui.Server.Controllers
{
    // https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ConfigurationController : ControllerBase
    {
        private static Collector _collector;

        private static ConfigurationOptions _config;
        private static ILogger<ConfigurationController> _logger;
        private static List<LogMessage> _logMessages;
        private IDataService _dataService;

        static ConfigurationController()
        {
            _collector = new Collector(false);
            _logMessages = new List<LogMessage>();
            // to subscribe to log messages
            Log.MessageLogged += Log_MessageLogged;
            _config = _collector.Config;
        }

        public ConfigurationController(ILogger<ConfigurationController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        [HttpGet]
        [Route("/api")]
        public IEnumerable<JsonResult> Get()
        {
            ConfigurationOptions ConfigurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(ConfigurationOptions, JsonHelpers.GetJsonSerializerOptions());

            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");
            return new List<JsonResult>() { new JsonResult(new ConfigurationOptions()) }.AsEnumerable();
        }

        [HttpGet]
        [Route("/api/configurationJson")]
        public ActionResult GetConfiguration()
        {
            ConfigurationOptions ConfigurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(ConfigurationOptions, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = new JsonResult(ConfigurationOptions, JsonHelpers.GetJsonSerializerOptions()){};
            //jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet
            jsonResult.ContentType = "application/json;charset=utf-8";

            return jsonResult;
        }

        [HttpGet]
        [Route("/api/configurationOptions")]
        public IEnumerable<string> GetOptionsConfiguration()
        {
            ConfigurationOptions configurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(configurationOptions, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = new JsonResult(configurationOptions, JsonHelpers.GetJsonSerializerOptions());
            jsonResult.ContentType = "application/json;charset=utf-8";

            //return new List<JsonResult>() { jsonResult }.AsEnumerable();
            return new List<string>() { jsonString }.AsEnumerable();
        }

        [HttpGet]
        [Route("/api/configuration")]
        public IEnumerable<string> GetPropertiesConfiguration()
        {
            ConfigurationProperties configurationProperties = _config.PropertyClone();
            string jsonString = JsonSerializer.Serialize(configurationProperties, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = new JsonResult(configurationProperties, JsonHelpers.GetJsonSerializerOptions());
            jsonResult.ContentType = "application/json;charset=utf-8";

            //return new List<JsonResult>() { jsonResult }.AsEnumerable();
            return new List<string>() { jsonString }.AsEnumerable();
        }

        [HttpPost("/api/configuration/update")]
        public IActionResult ImportConfiguration([FromBody] object properties)
        {
            try
            {
                ConfigurationOptions configurationProperties = JsonSerializer.Deserialize<ConfigurationOptions>(properties.ToString(), JsonHelpers.GetJsonSerializerOptions());
                _config.MergeConfig(configurationProperties);
                bool validated = _config.Validate();
                ConfigurationOptions newConfigurationOptions = _config.Clone();

                JsonResult jsonResult = new JsonResult(newConfigurationOptions, JsonHelpers.GetJsonSerializerOptions());
                jsonResult.ContentType = "application/json;charset=utf-8";
                string jsonString = JsonSerializer.Serialize(newConfigurationOptions, JsonHelpers.GetJsonSerializerOptions());

                if (validated)
                {
                    return Created($"/api/configuration/update", jsonString);
                }
                else
                {
                    //return Created($"/api/configuration/update", jsonString);
                    string jsonErrorString = JsonSerializer.Serialize(_logMessages, JsonHelpers.GetJsonSerializerOptions());
                    _logMessages.Clear();
                    return ValidationProblem($"failed validation:\r\n{jsonErrorString}", this.GetHashCode().ToString(), 400, "/api/configuration/update");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"exception:{e}");
                return BadRequest($"update/{properties} error:{e}");
            }
        }

        [HttpGet]
        [Route("/api/index")]
        public IEnumerable<ConfigurationOptions> Index()
        {
            return new List<ConfigurationOptions>() { _config.Clone() }.AsEnumerable();
        }

        [HttpPost]
        [Route("/api/configuration/save")]
        public async Task<IActionResult> SaveConfiguration([FromBody] ConfigurationProperties data)
        {
            await _dataService.SaveConfiguration(data);
            return Ok();
        }

        [HttpGet]
        [Route("/api/allconfiguration")]
        public async Task<IActionResult> GetAllConfiguration()
        {
            var allConfiguration = await _dataService.GetAllConfiguration();
            if (allConfiguration != null)
            {
                return Ok(allConfiguration);
            }
            else
            {
                return NotFound(allConfiguration);
            }
        }

        [HttpGet]
        [Route("/api/configuration/pull/{kustoTable}")]
        public async Task<IActionResult> GetConfiguration([FromRoute]string kustoTable)
        {
            var configuration = await _dataService.PullConfiguration(kustoTable);
            if (configuration != null)
            {
                return Ok(configuration);
            }
            else
            {
                return NotFound(configuration);
            }
        }

        [HttpDelete]
        [Route("/api/configuration/{kustoTable}")]
        public async Task<IActionResult> DeleteConfiguration(string kustoTable)
        {
            var deleteResult = await _dataService.DeleteConfiguration(kustoTable);
            return Ok(deleteResult);
        }

        private static void Log_MessageLogged(object sender, LogMessage args)
        {
            _logger.LogInformation($"ConfigurationController:CSFDMessage:{args.Message}");
            if (args.IsError)
            {
                _logMessages.Add(args);
            }
        }
    }
}