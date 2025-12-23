using System.Diagnostics;
using System.Reflection;

using CsProj.Domain;

using Spectre.Console;

namespace CsProj.Core;

internal static class Extensions
{
    extension(IAnsiConsole console)
    {
        public void Table<TElement>(IEnumerable<TElement> data)
        {
            var properties = typeof(TElement).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var table = new Table();
            foreach (var property in properties)
            {
                table.AddColumn(property.Name.EscapeMarkup());
            }
            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString()?.EscapeMarkup() ?? string.Empty).ToArray();
                table.AddRow(values);
            }
            console.Write(table);
        }

        public void Tree(DependencyTree dependencyTree)
        {
            foreach (var proj in dependencyTree)
            {
                var tree = new Tree($"[bold]{Path.GetFileName(proj.Key).EscapeMarkup()}[/]");
                PrintReferenceTree(dependencyTree, proj.Key, tree, []);
                console.Write(tree);
            }
        }

        private static void PrintReferenceTree(DependencyTree graph,
                                      string proj,
                                      IHasTreeNodes parent,
                                      HashSet<string> visited)
        {
            visited.Add(proj);
            foreach (var reference in graph[proj])
            {
                var nodeLabel = reference.EndsWith(".csproj") ? Path.GetFileName(reference) : reference;
                TreeNode child;
                switch (parent)
                {
                    case Tree tree:
                        child = tree.AddNode(nodeLabel);
                        break;
                    case TreeNode node:
                        child = node.AddNode(nodeLabel);
                        break;
                    default:
                        continue;
                }
                if (!visited.Contains(reference)
                    && graph.ContainsKey(reference))
                {
                    PrintReferenceTree(graph, reference, child, visited);
                }
            }
        }
    }

    extension(ILogger logger)
    {
        public void Error(LoadError loadError)
        {
            switch (loadError)
            {
                case LoadError.NoProjects:
                    logger.Error("No projects found");
                    break;
                case LoadError.MultipleProjects:
                    logger.Error("Multiple projects found. Please specify project name explicitly");
                    break;
                case LoadError.MultipleSolutions:
                    logger.Error("Multiple solutions found. Please specify solution name explicitly");
                    break;
                default:
                    throw new UnreachableException("Unknown project state");
            }
        }

        public void Info(Statistics statistics, TimeSpan totalTime)
        {
            string message = $"""

                Processed {statistics.Total} projects in {totalTime.TotalSeconds:N2} seconds.
                Modified: {statistics.Modified}
                Not Modified: {statistics.NotModified}
                Skipped: {statistics.Skipped}
                """;

            logger.Info(message);
        }

        public void Info(TimeSpan runtime)
        {
            logger.Info($"Operation completed in {runtime.TotalSeconds:N2} seconds.");
        }
    }
}
