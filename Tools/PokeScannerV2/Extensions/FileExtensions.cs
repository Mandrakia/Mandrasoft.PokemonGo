using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeScannerV2.Extensions
{
    static public class FileExtensions
    {
        static public async Task WriteAllTextAsync(string path, string content)
        {
            using (var fs = File.CreateText(path))
                await fs.WriteAsync(content);
        }
    }
}
