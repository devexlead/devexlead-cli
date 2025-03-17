using Spectre.Console;
using System.IO.Compression;
using System.Xml;

namespace DevExLead.Core.Helpers
{
    public static class FileHelper
    {
        public static void ReplaceFile(string sourceFilePath, string destinationFilePath)
        {
            // Copy the file and overwrite if it already exists
            File.Copy(sourceFilePath, destinationFilePath, true);
            Console.WriteLine($"{destinationFilePath} replaced successfully.");
        }

        public static void ChangeValue(XmlDocument xdoc, string key, string newValue)
        {
            AnsiConsole.MarkupLine($"[red]{key}={newValue}[/]");
            XmlNode xn = xdoc.SelectSingleNode($"//add[@key=\"{key}\"]")!;
            XmlElement el = (XmlElement)xn;
            el.SetAttribute("value", newValue);
        }

        //public static string GetJsonPropertyValue(string jsonKey, string output)
        //{
        //    try
        //    {
        //        //JsonDocumentPath package is required because SelectElement is not natively supported by .NET 6 (https://stackoverflow.com/questions/70678718/how-to-delete-and-update-based-on-a-path-in-system-text-json-net-6)
        //        var jsonDocument = JsonDocument.Parse(output);
        //        JsonElement? jsonElement = jsonDocument.RootElement.SelectElement($"$.{jsonKey}");
        //        if (jsonElement == null)
        //        {
        //            return string.Empty;
        //        }
        //        return jsonElement.Value.ToString();
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}

        public static string ReadFile(string fileLocation)
        {
            return File.ReadAllText(fileLocation);
        }

        public static void SaveFile(string fileLocation, string content)
        {
            File.WriteAllText(fileLocation, content);
            AnsiConsole.MarkupLine($"[green]{fileLocation} saved successfully.[/]");
        }

        public static List<FileInfo> FindAllFiles(string path, string name)
        {
            var response = new List<FileInfo>();
            var files = Directory.GetFiles(path, name, SearchOption.AllDirectories);
            files.ToList().ForEach(f => response.Add(new FileInfo(f)));
            return response;
        }

        public static void ExtractZipFile(string zipFolder, string zipFile)
        {
            if (Directory.Exists(zipFolder))
            {
                Directory.Delete(zipFolder, true);
            }
            Directory.CreateDirectory(zipFolder);
            ZipFile.ExtractToDirectory(zipFile, zipFolder);
        }

     
        public static void CopyAllContent(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDirectory))
            {
                string targetDirectoryPath = Path.Combine(targetDirectory, Path.GetFileName(directory));
                CopyAllContent(directory, targetDirectoryPath);
            }
        }


        public static async Task DownloadFileAsync(string url, string authorization, string filePath)
        {
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            {
                client.DefaultRequestHeaders.Add("Authorization", authorization);

                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                    await StartDownloadWithProgressBarAsync(
                        () => response.Content.ReadAsStreamAsync(),
                        totalBytes,
                        filePath,
                        "[green]Downloading file...[/]"
                    );
                }
            }
        }

        private static async Task StartDownloadWithProgressBarAsync(
                   Func<Task<Stream>> getContentStreamAsync,
                   long totalBytes,
                   string filePath,
                   string taskDescription)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (Stream contentStream = await getContentStreamAsync())
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                var progress = AnsiConsole.Progress()
                    .AutoClear(false)
                    .HideCompleted(false)
                    .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),    // Task description
                        new ProgressBarColumn(),        // Progress bar
                        new PercentageColumn(),         // Percentage
                        new RemainingTimeColumn(),      // Remaining time
                        new SpinnerColumn(),            // Spinner
                    });

                await progress.StartAsync(async ctx =>
                {
                    var task = ctx.AddTask(taskDescription, maxValue: totalBytes);

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        task.Increment(bytesRead);

                        // Update the progress bar
                        task.Value = totalRead;
                    }
                });
            }
        }

    }
}
