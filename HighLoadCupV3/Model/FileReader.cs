using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using HighLoadCupV3.Model.Dto;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model
{
    public class FileReader
    {
        public int ReadAccountFilesCount(string path)
        {
            using (var zipArchive = File.OpenRead(path))
            {
                var zip = new ZipArchive(zipArchive);
                return zip.Entries.Count;
            }
        }

        public int ReadTimeStamp(string path)
        {
            var line = File.ReadLines(path).First();

            return int.Parse(line);
        }

        public IEnumerable<AccountDto> ReadDto(string path, string extractionPath)
        {
            if (!Directory.Exists(extractionPath))
            {
                ZipFile.ExtractToDirectory(path, extractionPath);
            }

            var serializer = new JsonSerializer();

            foreach (var file in Directory.EnumerateFiles(extractionPath))
            {
                using (var fileStream = File.OpenRead(file))
                {
                    using (var sr = new StreamReader(fileStream))
                    {
                        using (var jsonReader = new JsonTextReader(sr))
                        {
                            var accountsDto = serializer.Deserialize<AccountsDto>(jsonReader);
                            foreach (var dto in accountsDto.Accounts)
                            {
                                yield return dto;
                            }
                        }
                    }
                }
            }
        }
    }
}
