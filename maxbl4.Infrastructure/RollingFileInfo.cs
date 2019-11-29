using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;

namespace maxbl4.Infrastructure
{
    public class RollingFileInfo
    {
        public int NumberOfDigits { get; }
        public string BaseFile { get; }

        public RollingFileInfo(string baseFile, int numberOfDigits = 3)
        {
            NumberOfDigits = numberOfDigits;
            BaseFile = baseFile;
        }

        public bool BaseExists => File.Exists(BaseFile);
        public bool Exist => Scan(BaseFile, NumberOfDigits, true).Any(File.Exists);

        public void Delete()
        {
            foreach (var f in Scan(BaseFile, NumberOfDigits, true))
            {
                File.Delete(f);
            }
        }

        public IEnumerable<string> AllCurrentFiles => Scan(BaseFile, NumberOfDigits, false); 
        
        private IEnumerable<string> Scan(string originalFilename, int numberOfDigits, bool returnNext)
        {
            var existingFiles = new List<FileIndex>();
            if (File.Exists(originalFilename))
                existingFiles.Add(new FileIndex{Name = originalFilename, Index = 0});
            
            var dir = Path.GetDirectoryName(originalFilename) ?? "";
            var ext = Path.GetExtension(originalFilename) ?? "";
            var name = Path.GetFileNameWithoutExtension(originalFilename) ?? "";
            var pattern = $"{Regex.Escape(name)}_(\\d+)";
            
            var matched = Directory.GetFiles(string.IsNullOrEmpty(dir) ? Environment.CurrentDirectory : dir,  name + "*")
                .Select(Path.GetFileNameWithoutExtension)
                .Select(x => new {File = x, Match = Regex.Match(x, pattern)})
                .Where(x => x.Match.Success)
                .Select(x => new FileIndex{Name = Path.Combine(dir, x.File) + ext, Index = int.Parse(x.Match.Groups[1].Value)})
                .OrderBy(x => x.Index);
            existingFiles.AddRange(matched);
            if (existingFiles.Any())
            {
                if (returnNext)
                {
                    var index = existingFiles.Select(x => x.Index).LastOrDefault() + 1;
                    var num = index.ToString(new string('0', numberOfDigits));
                    existingFiles.Add(new FileIndex {Name = Path.Combine(dir, $"{name}_{num}{ext}"), Index = index});
                }
            }
            else
            {
                existingFiles.Add(new FileIndex{Name = originalFilename, Index = 0});
            }
            
            return existingFiles.Select(x => x.Name);
        }
        
        public string NextFile => Scan(BaseFile, NumberOfDigits, true).Last();

        public string CurrentFile => Scan(BaseFile, NumberOfDigits, false).Last();

        class FileIndex
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }
    }
}