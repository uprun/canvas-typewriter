using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace lisperanto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            Response.Redirect("canvas-documents.html");
            return View();
        }

        [HttpPost]
        public async Task SaveCustomObject(string hash, string value)
        {
            var directory_info = Directory.CreateDirectory("customObjects");
            string path_with_hash = Path.Combine(directory_info.FullName, $"{hash}.json");
            if(System.IO.File.Exists(path_with_hash))
            {
                return;
            }
            using (var file_stream = System.IO.File.Create(path_with_hash))
            {
                using(StreamWriter stream_writer = new StreamWriter(file_stream, System.Text.Encoding.UTF8))
                {
                    await stream_writer.WriteAsync(value);
                }
            }
            var parsed = System.Text.Json.JsonDocument.Parse(value);
            try
            {
                //Console.WriteLine($"Parsed : {parsed.RootElement.ToString()}" );
                string definition = parsed.RootElement.GetProperty("javascript-function-definition@lisperanto").ToString();
                string name = parsed.RootElement.GetProperty("name@lisperanto").ToString();
                string directory_path = Path.Combine(Directory.GetCurrentDirectory(), "lisperanto", "js");
                var path_to_js = Path.Combine(directory_path, $"{name}.js");
                Console.WriteLine($"Path to file: {path_to_js}");
                definition = $"// Version hash: {hash}\r\n" +
                    "if(typeof(lisperanto) === 'undefined')\r\n{\r\n\tlisperanto = {};\r\n}\r\n\r\n" +
                    $"lisperanto.{name} = {definition};";
                Directory.CreateDirectory(directory_path);
                using(var file_stream_js = System.IO.File.Create(path_to_js))
                {
                    using(StreamWriter stream_writer = new StreamWriter(file_stream_js, System.Text.Encoding.UTF8))
                    {
                        await stream_writer.WriteAsync(definition);
                    }
                }
                //Console.WriteLine(definition);
            }
            catch(KeyNotFoundException _)
            {}
        }

        

        [HttpPost]
        public async Task SaveOperation(string hash, string value)
        {
            var directory_info = Directory.CreateDirectory("operations");
            using(var file_stream = System.IO.File.Create(Path.Combine(directory_info.FullName, $"{hash}.json")))
            {
                using(StreamWriter stream_writer = new StreamWriter(file_stream, System.Text.Encoding.UTF8))
                {
                    await stream_writer.WriteAsync(value);
                }
            }
            
        }

        [HttpGet]
        public async Task<string[]> ListOfOperations()
        {
            string directory_path = Path.Combine(Directory.GetCurrentDirectory(), "operations");
            var files = Directory.GetFiles(directory_path).OrderBy(path => path).Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
            return files;
        }

        public class CustomObjectResult
        {
            public string Hash {get; set;}
            public string Value {get; set;}
        }

        [HttpGet]
        public CustomObjectResult GetCustomObject(string hash)
        {
            string target_path = Path.Combine(Directory.GetCurrentDirectory(), "customObjects", $"{hash}.json");

            using(StreamReader stream_reader = new StreamReader(target_path))
            {
                return new CustomObjectResult
                {
                    Hash = hash,
                    Value = stream_reader.ReadToEnd()
                };
            }
        }
    }
}
