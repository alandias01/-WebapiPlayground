using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebapiPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileReaderController : ControllerBase
    {
        FileProcessor fp;
        
        public FileReaderController(ILogger<FileProcessor> logger)
        {
            fp = new FileProcessor(logger);
        }

        // GET: api/<FileReaderController>
        [HttpGet]
        public async Task<IEnumerable<string>> ReadFile()
        {
            var sw = new Stopwatch();
            sw.Start();
            var lines = await fp.ReadFile();
            sw.Stop();

            if (lines != null)
            {
                lines?.Insert(0,sw.Elapsed.ToString(@"m\:ss\.fffff"));
                return lines;
            }
            return new string[0];            
        }

        [HttpGet("create")]
        public async Task<ActionResult> CreateFile()
        {
            var ok = await fp.CreateFile();
            return ok ? Ok() : BadRequest();
        }
    }

    public class FileProcessor
    {
        ILogger<FileProcessor> _logger;

        public FileProcessor(ILogger<FileProcessor> logger)
        {
            _logger = logger;
        }
        public async Task<bool> CreateFile()
        {
            try
            {
                var builder = new StringBuilder();
                Enumerable.Range(0, 100).ToList().ForEach(
                    x => builder.AppendLine("The cat in the hat went out to get a pail of water so he can put out the fire"));

                await File.WriteAllTextAsync("../data.txt", builder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return false;
            }

            return true;
        }

        public async Task<List<string>?> ReadFile()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync("../data.txt");
                return lines.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                return null;
            }
        }
    }
}
