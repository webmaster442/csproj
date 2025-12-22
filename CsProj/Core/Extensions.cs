using System.Diagnostics;

using CsProj.Domain;

namespace CsProj.Core;

internal static class Extensions
{
    extension (ILogger logger)
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
