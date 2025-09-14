using DevExLead.Core;
using DevExLead.Core.Helpers;
using System.Data;
using System.Text.Json;

namespace DevExLead.Modules.Export.Handlers
{
    public class HttpEndpointsHandler : ICommandHandler
    {

        public async Task ExecuteAsync(Dictionary<string, object> options)
        {
            var path = ParameterHelper.ReadStringParameter(options, "path");
            var format = ParameterHelper.ReadStringParameter(options, "format");

            var rootPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                rootPath = path;
            }

            var httpEndpoints = ScanAllHttpEndpoints(rootPath);

            var fileName = "http-endpoints";
            switch (format)
            {
                case "json":
                    FileHelper.ExportAsJsonFile(httpEndpoints, rootPath, fileName);
                    break;
                case "csv":
                    FileHelper.ExportAsCsvFile(httpEndpoints, rootPath, fileName);
                    break;
                case "xls":
                    await FileHelper.ExportAsExcelFile(httpEndpoints, rootPath, fileName);
                    break;
                default:
                    FileHelper.ExportAsCsvFile(httpEndpoints, rootPath, fileName);
                    break;
            }
        }

        private static DataTable ScanAllHttpEndpoints(string folderPath)
        {
            var dataTable = new DataTable("HttpEndpoints");
            dataTable.Columns.Add("File", typeof(string));
            dataTable.Columns.Add("URL", typeof(string));

            var results = new List<UrlRecord>();
            var jsonFiles = Directory.GetFiles(folderPath, "appsettings.*.json", SearchOption.AllDirectories);

            foreach (var file in jsonFiles)
            {
                string content = File.ReadAllText(file);

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(content);
                    CollectUrls(doc.RootElement, file, results);
                }
                catch (JsonException ex)
                {
                    Console.Error.WriteLine($"❌ Could not parse {file}: {ex.Message}");
                }
            }

            foreach (var result in results)
            {
                dataTable.Rows.Add(result.File, result.Url);
            }

            return dataTable;
        }

        static void CollectUrls(JsonElement element, string file, List<UrlRecord> results)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        CollectUrls(property.Value, file, results);
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        CollectUrls(item, file, results);
                    }
                    break;

                case JsonValueKind.String:
                    string? value = element.GetString();
                    if (IsUrl(value))
                    {
                        results.Add(new UrlRecord(file, value!));
                    }
                    break;
            }
        }

        static bool IsUrl(string? value)
        {
            return value != null && (value.StartsWith("http://") || value.StartsWith("https://"));
        }
    }

    internal class UrlRecord
    {
        public UrlRecord(string file, string url)
        {
            File = file;
            Url = url;
        }

        public string File { get; internal set; }
        public string Url { get; internal set; }
    }
}
