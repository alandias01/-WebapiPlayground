using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebapiPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileReaderController : ControllerBase
    {
        FileProcessor fp;
        string file = "jsondata.json";
        string file2 = "jsondata2.json";

        public FileReaderController(ILogger<FileProcessor> logger)
        {
            fp = new FileProcessor(logger);
        }

        // GET: api/<FileReaderController>
        [HttpGet("read1")]
        public async Task<IEnumerable<string>> ReadFile()
        {
            var sw = new Stopwatch();
            sw.Start();
            var lines = await fp.ReadAllLinesAsync(file);
            sw.Stop();

            if (lines != null)
            {
                lines?.Insert(0,sw.Elapsed.ToString(@"m\:ss\.fffff"));
                return lines;
            }
            return new string[0];            
        }
                
        [HttpGet("read2")]
        public IEnumerable<string> ReadFile2()
        {
            var sw = new Stopwatch();
            sw.Start();
            var lines = fp.ReadFileSync(file2);
            sw.Stop();

            if (lines != null)
            {
                lines?.Insert(0, sw.Elapsed.ToString(@"m\:ss\.fffff"));
                return lines;
            }
            return new string[0];
        }

        [HttpGet("read3")]
        public IEnumerable<string> ReadFile3()
        {
            var sw = new Stopwatch();
            sw.Start();
            var lines = fp.ReadFileAsync(file2);
            sw.Stop();

            if (lines != null)
            {
                lines?.Insert(0, sw.Elapsed.ToString(@"m\:ss\.fffff"));
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

        public async Task<List<string>?> ReadAllLinesAsync(string file)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync($"../stuff/{file}");
                return lines.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                return null;
            }
        }

        public IList<string> ReadFileSync(string file)
        {
            var lines = new List<string>();
            using (FileStream fs = File.Open($"../stuff/{file}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var opt = new JsonSerializerSettings { Formatting = Formatting.None };
                            lines.Add(JsonConvert.DeserializeObject(line, opt).ToString());
                        }
                    }
                }
            }
            return lines;
        }

        public IList<string> ReadFileAsync(string file)
        {
            var lines = new List<string>();
            using (FileStream fs = File.Open($"../stuff/{file}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var opt = new JsonSerializerSettings { Formatting = Formatting.None };
                        lines.Add(JsonConvert.DeserializeObject(line, opt).ToString());
                    }
                }

            }
            return lines;
        }

        public async Task<bool> CreateFile()
        {
            try
            {
                var builder = new StringBuilder();
                Enumerable.Range(0, 100).ToList().ForEach(
                    x => builder.AppendLine("The cat in the hat went out to get a pail of water so he can put out the fire"));

                await File.WriteAllTextAsync("../stuff/data.txt", builder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return false;
            }

            return true;
        }
    }
}
