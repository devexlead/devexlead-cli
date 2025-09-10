using System.Text.Json;
using DevExLead.Core;
using Spectre.Console;

namespace DevExLead.Modules.Tools.Handlers
{
    public class NpmVersionChecker : ICommandHandler
    {
        public record PackageInfo(string Name, string Version, string Kind, string FilePath);

        public async Task ExecuteAsync(Dictionary<string, string> options)
        {
            var rootDirectory = Directory.GetCurrentDirectory();
            var packages = ScanAll(rootDirectory);
            SaveSummaryToCsv(packages, rootDirectory, Path.Combine(rootDirectory, "packages-summary.csv"));
            //PrintSpectreTable(packages, rootDirectory, showSummary: true);
        }

        private static void SaveSummaryToCsv(List<PackageInfo> packages, string rootDirectory, string outputPath)
        {
            var lines = new List<string>
                    {
                        "Package,Version,Kind,File"
                    };

            foreach (var p in packages)
            {
                string relative = Path.GetRelativePath(rootDirectory, p.FilePath).Replace("\"", "\"\"");
                string name = p.Name.Replace("\"", "\"\"");
                string version = p.Version.Replace("\"", "\"\"");
                string kind = p.Kind.Replace("\"", "\"\"");

                lines.Add($"\"{name}\",\"{version}\",\"{kind}\",\"{relative}\"");
            }

            File.WriteAllLines(outputPath, lines);
        }


        private static List<PackageInfo> ScanAll(string rootDirectory)
        {
            var list = new List<PackageInfo>();

            foreach (var file in Directory.EnumerateFiles(rootDirectory, "package.json", SearchOption.AllDirectories))
            {
                try
                {
                    list.AddRange(ReadPackagesFromFile(file));
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to parse[/] {Path.GetRelativePath(rootDirectory, file)}");
                }
            }

            return list
                .OrderBy(p => p.Kind)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.FilePath, StringComparer.OrdinalIgnoreCase)
                .ToList();
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
