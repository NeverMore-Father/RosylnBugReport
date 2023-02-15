using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;

class Program
{

    private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress loadProgress)
        {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null)
            {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }

    static async Task Main(string[] args)
    {
        var solutionPath = @"..\..\..\..\TargetProject\TargetProject.sln";

        using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
        {
            Console.WriteLine($"Loading solution '{solutionPath}'");
            Solution solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution '{solutionPath}'");
            var project = solution.Projects.ElementAt(0);
            var document = project.Documents.ToList().Find(itr => itr.Name.Contains("Student"));
            var root = await document.GetSyntaxRootAsync();
            var model = await document.GetSemanticModelAsync();
            var node = root.DescendantNodesAndSelf().ToList().Find(itr => itr is ClassDeclarationSyntax);
            var symbol = model.GetDeclaredSymbol(node);
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, "BadStudent", solution.Options);
            workspace.TryApplyChanges(newSolution);
        }

        Console.ReadLine();
    }
}