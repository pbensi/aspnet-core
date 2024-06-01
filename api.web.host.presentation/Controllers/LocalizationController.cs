using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace api.web.host.presentation.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Version}/[controller]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private readonly string _xmlDirectory;
        public LocalizationController()
        {
            _xmlDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        }

        [HttpGet("GetLocalization")]
        public ActionResult<Dictionary<string, string>> GetLocalization(string culture = "english.en")
        {
            try
            {
                var xmlFilePath = Path.Combine(_xmlDirectory, $"{culture}.xml");

                if (!System.IO.File.Exists(xmlFilePath))
                {
                    return new Dictionary<string, string>();
                }

                var xmlDoc = XDocument.Load(xmlFilePath);

                var localizationData = xmlDoc.Root
                    .Element("texts")
                    .Elements("text")
                    .ToDictionary(
                        element => element.Attribute("name")?.Value,
                        element => element.Attribute("value")?.Value ?? "");

                return Ok(localizationData ?? new Dictionary<string, string>());
            }
            catch (FileNotFoundException)
            {
                return NotFound($"Localization data not found for culture: {culture}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetResources")]
        public ActionResult<List<string>> GetResources()
        {
            try
            {
                if (!Directory.Exists(_xmlDirectory))
                {
                    return Ok(new List<string>());
                }

                var xmlFiles = Directory.GetFiles(_xmlDirectory, "*.xml")
                                          .Select(Path.GetFileNameWithoutExtension)
                                          .ToList();

                return Ok(xmlFiles);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
