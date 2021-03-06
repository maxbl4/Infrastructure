﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
        public bool Exist => AllCurrentFiles.Any(File.Exists);

        public int Delete()
        {
            var count = 0;
            foreach (var f in AllCurrentFiles)
            {
                try
                {
                    File.Delete(f);
                    count++;
                }
                catch {}
            }

            return count;
        }

        public int Index => Scan(BaseFile, NumberOfDigits, false).Select(x => x.Index).LastOrDefault();

        public IEnumerable<string> AllCurrentFiles => Scan(BaseFile, NumberOfDigits, false).Select(x => x.Name); 
        
        private IEnumerable<FileIndex> Scan(string originalFilename, int numberOfDigits, bool returnNext)
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
            
            return existingFiles;
        }
        
        public string NextFile => Scan(BaseFile, NumberOfDigits, true).Select(x => x.Name).Last();

        public string CurrentFile => AllCurrentFiles.Last();

        class FileIndex
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }
    }
}