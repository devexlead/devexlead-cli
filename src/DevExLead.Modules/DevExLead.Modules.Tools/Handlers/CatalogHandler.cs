using DevExLead.Core;
using DevExLead.Core.Helpers;
using DevExLead.Modules.Export.Model;
using Spectre.Console;
using System.Data;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevExLead.Modules.Export.Handlers
{
    public class CatalogHandler : ICommandHandler
    {
        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            options.TryGetValue("path", out var path);
            options.TryGetValue("format", out var format);

            var rootPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                rootPath = path;
            }
           
            var softwareCatalog = await BuildSoftwareCatalog(rootPath);

            var fileName = "catalog";
            switch (format)
            {
                case "json":
                    FileHelper.ExportAsJsonFile(softwareCatalog, rootPath, fileName);
                    break;
                case "csv":
                    FileHelper.ExportAsCsvFile(softwareCatalog, rootPath, fileName);
                    break;
                default:
                    FileHelper.ExportAsCsvFile(softwareCatalog, rootPath, fileName);
                    break;
            }
        }


        private static async Task<DataTable> BuildSoftwareCatalog(string rootPath)
        {
            var fileMatchesByDirectory = await FindKubernetesAndDatadogFiles(rootPath);

            DataTable table = new DataTable("FileMatches");
            table.Columns.Add("Repository", typeof(string));
            table.Columns.Add("Kubernetes", typeof(string));
            table.Columns.Add("app_name", typeof(string));
            table.Columns.Add("service_prefix", typeof(string));
            table.Columns.Add("Datadog", typeof(string));
            table.Columns.Add("Pipeline URL", typeof(string));

            foreach (var kvp in fileMatchesByDirectory)
            {
                string directory = kvp.Key;
                foreach (var match in kvp.Value)
                {
                    //Extract K8 Fields
                    string json = File.ReadAllText(match.KubernetesFile);
                    KubernetesSettingsFile k8sObject = JsonSerializer.Deserialize<KubernetesSettingsFile>(json);

                    //Extract Datadog Fields
                    var pipelineUrl = string.Empty;
                    if (match.DatadogFile != null)
                    {
                        try
                        {
                            var yaml = File.ReadAllText(match.DatadogFile);
                            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .IgnoreUnmatchedProperties()
                                .Build();
                            var datadogRoot = deserializer.Deserialize<DatadogSettingsFile>(yaml);
                            pipelineUrl = datadogRoot?.Metadata?.Links?
                                                                .FirstOrDefault(l => string.Equals(l.Name, "Pipeline", StringComparison.OrdinalIgnoreCase))
                                                                ?.Url ?? string.Empty;
                        }
                        catch (Exception)
                        {

                        }
                    }

                    table.Rows.Add(
                        directory,
                        Path.GetFileName(match.KubernetesFile) ?? string.Empty,
                        k8sObject?.Default.app_name ?? string.Empty,
                        k8sObject?.Default.service_prefix ?? string.Empty,
                        Path.GetFileName(match.DatadogFile),
                        pipelineUrl
                    );
                }
            }

            // Sort the table by the "Name" column
            DataView view = table.DefaultView;
            view.Sort = "Repository ASC";
            DataTable sortedTable = view.ToTable();
            return sortedTable;
        }

        public static async Task<Dictionary<string, List<(string KubernetesFile, string? DatadogFile)>>> FindKubernetesAndDatadogFiles(string folderPath)
        {
            var kubernetesFiles = Directory.GetFiles(folderPath, "*k8_settings*.json", SearchOption.AllDirectories);
            var datadogFiles = Directory.GetFiles(folderPath, "*datadog*.yaml", SearchOption.AllDirectories);

            // Group files by their directory
            var filesByDirectory = new Dictionary<string, List<(string filePath, string fileType)>>();

            foreach (string file in kubernetesFiles)
            {
                string directory = Path.GetDirectoryName(file);
                if (!filesByDirectory.ContainsKey(directory))
                    filesByDirectory[directory] = new List<(string, string)>();
                filesByDirectory[directory].Add((file, "kubernetes"));
            }

            foreach (string file in datadogFiles)
            {
                string directory = Path.GetDirectoryName(file);
                if (!filesByDirectory.ContainsKey(directory))
                    filesByDirectory[directory] = new List<(string, string)>();
                filesByDirectory[directory].Add((file, "datadog"));
            }

            // Dictionary to store matches per directory
            var fileMatchesByDirectory = new Dictionary<string, List<(string KubernetesFile, string? DatadogFile)>>();

            foreach (var directoryGroup in filesByDirectory)
            {
                string directory = directoryGroup.Key;
                var filesInDirectory = directoryGroup.Value;

                var k8sFiles = filesInDirectory.Where(f => f.fileType == "kubernetes").Select(f => f.filePath).ToList();
                var ddFiles = filesInDirectory.Where(f => f.fileType == "datadog").Select(f => f.filePath).ToList();

                var matches = new List<(string KubernetesFile, string? DatadogFile)>();
                var matchedDd = new HashSet<string>();

                if (k8sFiles.Count > 1 && ddFiles.Count > 1)
                {
                    foreach (var k8sFile in k8sFiles)
                    {
                        var choices = ddFiles
                            .Where(dd => !matchedDd.Contains(dd))
                            .Select(dd => Path.GetFileName(dd))
                            .ToList();
                        choices.Add("No counterpart found");

                        var selected = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title($"Match Kubernetes file [blue]{Path.GetFileName(k8sFile)}[/] with a Datadog file:")
                                .AddChoices(choices)
                        );

                        if (selected != "No counterpart found")
                        {
                            var matchedDdFile = ddFiles.First(dd => Path.GetFileName(dd) == selected);
                            matchedDd.Add(matchedDdFile);
                            matches.Add((k8sFile, matchedDdFile));
                        }
                        else
                        {
                            matches.Add((k8sFile, null));
                        }
                    }

                    // Optionally, handle unmatched Datadog files
                    foreach (var ddFile in ddFiles.Except(matchedDd))
                    {
                        matches.Add((null, ddFile));
                    }
                }
                else
                {
                    // Simple case: just pair up by order, or mark as unmatched
                    foreach (var k8sFile in k8sFiles)
                        matches.Add((k8sFile, ddFiles.FirstOrDefault()));
                    foreach (var ddFile in ddFiles.Skip(k8sFiles.Count))
                        matches.Add((null, ddFile));
                }

                fileMatchesByDirectory[directory] = matches;
            }

            return fileMatchesByDirectory;
        }


    }
}
