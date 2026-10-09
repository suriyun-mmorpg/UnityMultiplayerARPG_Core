using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MultiplayerARPG
{
    public static class DataPatchZipExporter
    {
        public static void Write(Stream output, IReadOnlyList<DataPatchEntry> entries)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (entry == null || entry.data == null || string.IsNullOrEmpty(entry.dataType) || entry.dataType.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                    throw new InvalidOperationException("A patch record has missing data or an invalid type name.");
                if (!paths.Add(EntryPath(entry)))
                    throw new InvalidOperationException("Duplicate ZIP record: " + EntryPath(entry));
            }

            using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                var serializer = JsonSerializer.CreateDefault();
                foreach (var entry in entries)
                {
                    using (var stream = zip.CreateEntry(EntryPath(entry), CompressionLevel.Optimal).Open())
                    using (var text = new StreamWriter(stream, new UTF8Encoding(false)))
                    using (var json = new JsonTextWriter(text)
                    {Formatting = Formatting.Indented})
                        serializer.Serialize(json, entry);
                }
            }
        }

        private static string EntryPath(DataPatchEntry entry) => entry.dataType + "/" + entry.dataId.ToString(CultureInfo.InvariantCulture) + ".json";

        public static string SaveToFolder(string folder, IReadOnlyList<DataPatchEntry> entries)
        {
            string name = "data-patch-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N") + ".zip";
            string destination = Path.Combine(folder, name);
            string temporary = destination + ".tmp";
            bool created = false;
            try
            {
                using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                {
                    created = true;
                    Write(output, entries);
                }

                File.Move(temporary, destination);
                return destination;
            }
            finally
            {
                if (created && File.Exists(temporary))
                    File.Delete(temporary);
            }
        }
    }
}
