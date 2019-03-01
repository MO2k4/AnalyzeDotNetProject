using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet.ProjectModel;

namespace AnalyzeDotNetProject
{
    public partial class Program
    {
        private static StringBuilder stringBuilder = new StringBuilder();

        public static void Main(string[] args)
        {
            // Replace to point to your project or solution
            string projectPath = @"xxx";

            var dependencyGraphService = new DependencyGraphService();
            var dependencyGraph = dependencyGraphService.GenerateDependencyGraph(projectPath);

            foreach (var project in dependencyGraph.Projects.Where(p => p.RestoreMetadata.ProjectStyle == ProjectStyle.PackageReference))
            {
                // Generate lock file
                var lockFileService = new LockFileService();
                var lockFile = lockFileService.GetLockFile(project.FilePath, project.RestoreMetadata.OutputPath);

                stringBuilder.AppendLine(project.Name);

                foreach (var targetFramework in project.TargetFrameworks)
                {
                    var dependencies = new List<Dependency>();

                    stringBuilder.AppendLine($"  [{targetFramework.FrameworkName}]");

                    var lockFileTargetFramework = lockFile.Targets.FirstOrDefault(t => t.TargetFramework.Equals(targetFramework.FrameworkName));
                    if (lockFileTargetFramework != null)
                    {
                        foreach (var dependency in targetFramework.Dependencies)
                        {
                            var projectLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == dependency.Name);
                            var reportDependency = ReportDependency(projectLibrary, lockFileTargetFramework, 1);
                            
                            if (reportDependency == null) continue;
                            
                            dependencies.Add(reportDependency);
                        }
                    }

                    var childrenDependencies = dependencies.SelectMany(dep => dep.Children).ToList();
                    var removableDependencies = new List<Dependency>();

                    foreach (var dependency in dependencies)
                    {
                        var children = childrenDependencies.Where(c => c.Equals(dependency)).ToList();

                        if (children.Count > 0)
                        {
                            dependency.ContainingPackages = children;
                            removableDependencies.Add(dependency);
                        }
                    }

                    var removableContent = string.Join(Environment.NewLine + new string(' ', 3), removableDependencies.Select(dep => dep.ToString()));

                    if (removableContent.Length > 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("Removable Dependencies");
                        stringBuilder.Append(new string(' ', 3) + removableContent);
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                    }
                }
            }

            Console.WriteLine(stringBuilder.ToString());
            Console.Read();
        }

        private static Dependency ReportDependency(LockFileTargetLibrary projectLibrary, LockFileTarget lockFileTargetFramework, int indentLevel, Dependency dependency = null)
        {
            if (projectLibrary == null)
                return null;

            if (indentLevel == 1)
                dependency = new Dependency(projectLibrary.Name, projectLibrary.Version.OriginalVersion);
            else
                dependency.Children.Add(new Dependency(projectLibrary.Name, projectLibrary.Version.OriginalVersion) { Parent = dependency.Name });

            // stringBuilder.Append(new string(' ', indentLevel * 2));
            // stringBuilder.AppendLine($"{projectLibrary.Name}, v{projectLibrary.Version}");

            foreach (var childDependency in projectLibrary.Dependencies)
            {
                var childLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == childDependency.Id);
                ReportDependency(childLibrary, lockFileTargetFramework, indentLevel + 1, dependency);
            }

            return dependency;
        }
    }
}
