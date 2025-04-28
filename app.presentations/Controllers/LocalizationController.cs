using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace app.presentations.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Name}/[controller]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private readonly string _localizationDirectory;
        private readonly ILogger<LocalizationController> _logger;
        private readonly IMemoryCache _cache;

        public LocalizationController(ILogger<LocalizationController> logger, IMemoryCache cache)
        {
            _localizationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..", "app.shared", "Localizations");
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("GetXmlLocalization")]
        public async Task<ActionResult<Dictionary<string, string>>> GetXmlLocalization(string culture = "english.en")
        {
            try
            {
                var cacheKey = $"XmlLocalization_{culture}";
                if (!_cache.TryGetValue(cacheKey, out Dictionary<string, string> localizationData))
                {
                    var xmlFilePath = Path.Combine(_localizationDirectory, "xml", $"{culture}.xml");

                    if (!System.IO.File.Exists(xmlFilePath))
                    {
                        return NotFound(new { message = "Localization XML file not found." });
                    }

                    var xmlDoc = await Task.Run(() => XDocument.Load(xmlFilePath));

                    localizationData = xmlDoc.Root?
                        .Element("texts")?
                        .Elements("text")
                        .Where(e => e.Attribute("name") != null)
                        .ToDictionary(
                            e => e.Attribute("name")!.Value,
                            e => e.Attribute("value")?.Value ?? string.Empty
                        ) ?? new Dictionary<string, string>();

                    _cache.Set(cacheKey, localizationData, TimeSpan.FromMinutes(10));
                }

                return Ok(localizationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading XML localization for culture '{culture}': {ex.Message}");
                return StatusCode(500, new { message = "Error loading XML localization." });
            }
        }

        [HttpGet("GetJsonLocalization")]
        public async Task<ActionResult<Dictionary<string, string>>> GetJsonLocalization(string culture = "english.en")
        {
            try
            {
                var cacheKey = $"JsonLocalization_{culture}";
                if (!_cache.TryGetValue(cacheKey, out Dictionary<string, string> localizationData))
                {
                    var jsonFilePath = Path.Combine(_localizationDirectory, "json", $"{culture}.json");

                    if (!System.IO.File.Exists(jsonFilePath))
                    {
                        return NotFound(new { message = "Localization JSON file not found." });
                    }

                    var jsonData = await System.IO.File.ReadAllTextAsync(jsonFilePath);
                    localizationData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData)
                                      ?? new Dictionary<string, string>();

                    _cache.Set(cacheKey, localizationData, TimeSpan.FromMinutes(10));
                }

                return Ok(localizationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading JSON localization for culture '{culture}': {ex.Message}");
                return StatusCode(500, new { message = "Error loading JSON localization." });
            }
        }

        [HttpGet("GetResources")]
        public ActionResult<HashSet<string>> GetResources(string extension = "xml")
        {
            try
            {
                if (!Directory.Exists(_localizationDirectory))
                {
                    return Ok(new HashSet<string>());
                }

                string specificDirectory = extension.ToLower() switch
                {
                    "xml" => Path.Combine(_localizationDirectory, "xml"),
                    "json" => Path.Combine(_localizationDirectory, "json"),
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(specificDirectory) || !Directory.Exists(specificDirectory))
                {
                    return BadRequest(new { message = "Invalid or unsupported extension specified." });
                }

                var files = Directory.GetFiles(specificDirectory, $"*.{extension}")
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .Where(name => !string.IsNullOrEmpty(name))
                                     .ToHashSet();

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving localization resources: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving localization resources." });
            }
        }
    }
}
