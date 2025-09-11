using DevExLead.Core;
using DevExLead.Core.Helpers;
using Spectre.Console;
using System.Data;
using System.Text.Json;

namespace DevExLead.Modules.Export.Handlers
{
    public class NpmHandler : ICommandHandler
    {
        public record PackageInfo(string Name, string Version, string Kind, string FilePath);

        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            options.TryGetValue("path", out var path);
            options.TryGetValue("format", out var format);

            var rootPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                rootPath = path;
            }

            var npmPackages = ScanAllNpmPackages(rootPath);

            var fileName = "npm";
            switch (format)
            {
                case "json":
                    FileHelper.ExportAsJsonFile(npmPackages, rootPath, fileName);
                    break;
                case "csv":
                    FileHelper.ExportAsCsvFile(npmPackages, rootPath, fileName);
                    break;
                default:
                    FileHelper.ExportAsCsvFile(npmPackages, rootPath, fileName);
                    break;
            }
            //PrintSpectreTable(packages, rootDirectory, showSummary: true);
        }

        private static DataTable ScanAllNpmPackages(string rootDirectory)
        {
            var dataTable = new DataTable("NpmPackages");

            // Add columns to match PackageInfo properties
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Version", typeof(string));
            dataTable.Columns.Add("Kind", typeof(string));
            dataTable.Columns.Add("FilePath", typeof(string));

            foreach (var file in Directory.EnumerateFiles(rootDirectory, "package.json", SearchOption.AllDirectories))
            {
                try
                {
                    var packages = ReadPackagesFromFile(file);
                    foreach (var package in packages)
                    {
                        dataTable.Rows.Add(package.Name, package.Version, package.Kind, package.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to parse[/] {Path.GetRelativePath(rootDirectory, file)}");
                }
            }

            // Sort the DataTable
            DataView view = dataTable.DefaultView;
            view.Sort = "Kind ASC, Name ASC, FilePath ASC";
            DataTable sortedTable = view.ToTable();

            return sortedTable;
        }

        private static IEnumerable<PackageInfo> ReadPackagesFromFile(string packageJsonPath)
        {
            using var stream = File.OpenRead(packageJsonPath);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            var results = new List<PackageInfo>();
            AddMap(root, "dependencies", "Dependency", packageJsonPath, results);
            AddMap(root, "devDependencies", "DevDependency", packageJsonPath, results);
            // Uncomment if needed:
            // AddMap(root, "peerDependencies", "PeerDependency", packageJsonPath, results);
            // AddMap(root, "optionalDependencies", "OptionalDependency", packageJsonPath, results);

            return results;
        }

        private static void AddMap(JsonElement root, string propertyName, string kind, string filePath, List<PackageInfo> sink)
        {
            if (root.TryGetProperty(propertyName, out var map) && map.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in map.EnumerateObject())
                {
                    sink.Add(new PackageInfo(prop.Name, prop.Value.GetString() ?? string.Empty, kind, filePath));
                }
            }
        }

        private static void PrintSpectreTable(
       List<PackageInfo> packages,
       string rootDirectory,
       bool showSummary)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold cyan]Discovered Packages[/]");

            table.AddColumn(new TableColumn("[u]Package[/]").LeftAligned());
            table.AddColumn(new TableColumn("[u]Version[/]").Centered());
            table.AddColumn(new TableColumn("[u]Kind[/]").LeftAligned());
            table.AddColumn(new TableColumn("[u]File[/]").LeftAligned());

            foreach (var p in packages)
            {
                var kindStyle = p.Kind switch
                {
                    "Dependency" => "green",
                    "DevDependency" => "grey",
                    "PeerDependency" => "yellow",
                    "OptionalDependency" => "blue",
                    _ => "white"
                };

                var packageNameMarkup = Escape(p.Name);

                var versionMarkup = Escape(p.Version);

                string relative = Path.GetRelativePath(rootDirectory, p.FilePath);
                table.AddRow(
                    packageNameMarkup,
                    versionMarkup,
                    $"[{kindStyle}]{Escape(p.Kind)}[/]",
                    Escape(relative)
                );
            }

            AnsiConsole.Write(table);

            if (showSummary)
            {
                AnsiConsole.WriteLine();

                var byKind = packages
                    .GroupBy(p => p.Kind)
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}: {g.Count()}");

                AnsiConsole.MarkupLine("[bold]Summary[/]: " + string.Join(" | ", byKind));
                AnsiConsole.MarkupLine($"[bold]Unique packages[/]: {packages.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()}");
                AnsiConsole.MarkupLine($"[bold]Total rows[/]: {packages.Count}");
            }

        }
        private static string Escape(string value) =>
    value.Replace("[", "[[").Replace("]", "]]");

    }
}
