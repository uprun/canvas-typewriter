using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace lisperanto.Controllers
{
    public class JustDocumentController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public JustDocumentController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            Response.Redirect("canvas-documents.html");
            return View();
        }

        

        [HttpPost]
        public async Task SaveDocument(string path, string[] text)
        {
            using(var file_stream = System.IO.File.Create(path))
            {
                using(StreamWriter stream_writer = new StreamWriter(file_stream, System.Text.Encoding.UTF8))
                {
                    foreach(var line in text)
                    {
                        await stream_writer.WriteLineAsync(line);
                    }
                    
                }
            }
        }

        [HttpGet]
        public async Task<string[]> ListOfDocuments(string? query = "*")
        {
            string directory_path = Path.Combine(Directory.GetCurrentDirectory(), "documents");
            var files = Directory.GetFiles(directory_path, query, SearchOption.AllDirectories).OrderBy(path => path).ToArray();
            return files;
        }

        public class DocumentResult
        {
            public string Path {get; set;}
            public string[] Text {get; set;}
        }

        [HttpGet]
        public async Task<DocumentResult> GetDocument(string path)
        {
            var result = new DocumentResult
                {
                    Path = path
                };

            using(StreamReader stream_reader = new StreamReader(path))
            {
                 
            }
            return result;
        }
    }
}
