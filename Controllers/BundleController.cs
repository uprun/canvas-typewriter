using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StringExtensionTools;

namespace lisperanto.Controllers
{
    public class BundleController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public BundleController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task GenerateBundle(string path)
        {
            Console.WriteLine($"{nameof(GenerateBundle)} for \"{path}\"");
            string input_path = Path.Combine(Directory.GetCurrentDirectory(), path);
            var without_extension = Path.GetFileNameWithoutExtension(input_path);
            var memory_stream = await helper_generate_bundle(input_path, add_script_tags: true);
            var hash = await generate_hash(memory_stream);
            var output_name = $"{without_extension}--{hash}{Path.GetExtension(input_path)}";
            string output_path = Path.Combine(Directory.GetCurrentDirectory(), "bundles", output_name);
            var directory = Path.GetDirectoryName(output_path);
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            using(StreamWriter actual_writer = new StreamWriter(output_path))
            {
                memory_stream.WriteTo(actual_writer.BaseStream);
                actual_writer.Flush();
            }
            Response.ContentType = "text/html";
            var result = new MemoryStream();
            using(StreamWriter html_stream_writer = new StreamWriter(result))
            {
                string path2 = Path.Combine("bundles", output_name);
                string some = Request.IsHttps ? "https://" : "http://";
                html_stream_writer.WriteLine($"<a href=\"{some}{Request.Host}/{path2}\" target=\"blank\" >{hash}</a>");
                html_stream_writer.Flush();
                result.Seek(0, SeekOrigin.Begin);
                await result.CopyToAsync(Response.Body);
            }


        }

        public async Task<string> generate_hash(MemoryStream in_memory_stream)
        {
            string string_hash = "";
            in_memory_stream.Seek(0, SeekOrigin.Begin);
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var hash = await sha256Hash.ComputeHashAsync(in_memory_stream);
                string_hash = String.Join("", hash.Select(h => h.ToString("x2")));
            }
            in_memory_stream.Seek(0, SeekOrigin.Begin);
            return string_hash;

        }

        private static async Task<MemoryStream> helper_generate_bundle(string input_path, bool add_script_tags = true)
        {
            var just_copy = new MemoryStream();
            MemoryStream in_memory_stream = new MemoryStream();
            using (StreamReader input_source = new StreamReader(input_path))
            using (StreamWriter output_writer = new StreamWriter(in_memory_stream))
            {
                while (input_source.EndOfStream == false)
                {
                    var line = await input_source.ReadLineAsync();
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("<script ") && trimmed.Contains("src=\"") && trimmed.IsAbsent("lisperanto-skip-bundle=\"true\""))
                    {
                        embed_script_content(add_script_tags, output_writer, trimmed);
                        continue;

                    }

                    if (trimmed.StartsWith("<link rel=\"stylesheet\"") && trimmed.Contains("href=\"") && trimmed.IsAbsent("lisperanto-skip-bundle=\"true\""))
                    {
                        embed_style_content(output_writer, trimmed);

                        continue;

                    }

                    //Console.WriteLine(line);
                    output_writer.WriteLine(line);
                    

                }
                
                in_memory_stream.WriteTo(just_copy);
                just_copy.Seek(0, SeekOrigin.Begin);
            }
            return just_copy;
        }

        private static void embed_style_content(StreamWriter output_writer, string trimmed)
        {
            var splitted = trimmed.Split(new[] { " ", "</link>", "/>", "<link" }, StringSplitOptions.RemoveEmptyEntries);
            string src_from_script = splitted.First(a => a.StartsWith("href="));
            var actual_path = src_from_script.Substring("href=\"".Length, src_from_script.Length - "href=\"\"".Length);

            output_writer.WriteLine("<style>");

            var path_to_src_file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", actual_path);
            Console.WriteLine($"Processing \"{path_to_src_file}\"");
            output_writer.WriteLine($"/* Begin of \"{actual_path}\" */");
            using (var extra_readed = new StreamReader(path_to_src_file))
            {
                var all_content = extra_readed.ReadToEnd();
                output_writer.Write(all_content);

            }
            output_writer.WriteLine("");
            output_writer.WriteLine($"/* End of \"{actual_path}\" */");
            output_writer.WriteLine("</style>");
        }

        private static void embed_script_content(bool add_script_tags, StreamWriter output_writer, string trimmed)
        {
            var splitted = trimmed.Split(new[] { " ", "</script>", ">", "<script" }, StringSplitOptions.RemoveEmptyEntries);
            string src_from_script = splitted.First(a => a.StartsWith("src="));
            var actual_path = src_from_script.Substring("src=\"".Length, src_from_script.Length - "src=\"\"".Length);

            if (add_script_tags)
            {
                output_writer.WriteLine("<script>");
            }

            var path_to_src_file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", actual_path);
            Console.WriteLine($"Processing \"{path_to_src_file}\"");
            output_writer.WriteLine($"// Begin of \"{actual_path}\"");
            using (var extra_readed = new StreamReader(path_to_src_file))
            {
                var all_content = extra_readed.ReadToEnd();
                output_writer.Write(all_content);

            }
            output_writer.WriteLine("");
            output_writer.WriteLine($"// End of \"{actual_path}\"");
            if (add_script_tags)
            {
                output_writer.WriteLine("</script>");
            }
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
    }
}
