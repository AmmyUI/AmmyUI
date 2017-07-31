using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nitra.Declarations;
using Nitra.ProjectSystem;
using Ammy.Language;
using Ammy.Xaml;
using Nitra;

namespace Ammy.Build
{
    public class AmmyFile<T> : FsFile<T> where T : IAst
    {
        public override int Id { get; }

        public FileEvalPropertiesData EvalPropertiesData { get; private set; }
        public AmmyFileMeta Meta { get; }
        public List<CompilerMessage> BamlCompilerMessages { get; } = new List<CompilerMessage>();
        public string OutputFilename { get; }
        public DateTime LastWriteTime { get; set; }
        public override int Length
        {
            get {
                var sourceText = Meta?.SourceText;
                if (sourceText != null)
                    return Meta.SourceText.Length;
                return (int) new FileInfo(FilePath).Length;
            }
        }

        public AmmyFile(int id, AmmyFileMeta meta, Nitra.Language language, string projectDir, FsProject<T> fsProject = null, FileStatistics statistics = null)
            : base(meta.Filename, language, fsProject, statistics)
        {
            Id = id;
            Meta = meta;

            var filename = Path.ChangeExtension(Meta.Filename, ".xaml");

            if (Path.IsPathRooted(filename))
                OutputFilename = filename;
            else
                OutputFilename = Path.Combine(projectDir, filename);

            LastWriteTime = new FileInfo(FilePath).LastWriteTime;

            EvalPropertiesData = GetEvalPropertiesData();
        }

        public TopWithNode GetTopWithNode()
        {
            return ((Start)Ast).Top as TopWithNode;
        }

        public override SourceSnapshot GetSource()
        {
            var text = Meta?.SourceText ?? System.IO.File.ReadAllText(FilePath);
            return new SourceSnapshot(text, this, Version); 
        }

        private IEnumerable<CompilerMessage> GetXamlNodeErrors()
        {
            try {
                var noErrors = Enumerable.Empty<CompilerMessage>();
                var topWithNode = GetTopWithNode();

                if (topWithNode == null) return noErrors;
                if (!topWithNode.IsXamlEvaluated) return noErrors;

                var xamlNode = topWithNode.Xaml as XamlNode;

                if (xamlNode == null) return noErrors;


                return xamlNode.Errors;
            } catch {
                return Enumerable.Empty<CompilerMessage>();
            }
        }

        public IReadOnlyList<CompilerMessage> GetErrors()
        {
            try {
                var errors = GetCompilerMessages().Concat(EvalPropertiesData.GetCompilerMessage())
                                                  .Concat(GetXamlNodeErrors())
                                                  .Concat(BamlCompilerMessages)
                                                  .Distinct(new ErrorEqualityComparer())
                                                  .Take(50)
                                                  .ToList();

                if (errors.Any(c => !c.Text.StartsWith("XAML not evaluated: ")))
                    return errors.Where(c => !c.Text.StartsWith("XAML not evaluated: "))
                                 .ToArray();

                return errors;
            } catch {
                return new CompilerMessage[0];
            }
        }
    }

    public class ErrorEqualityComparer : IEqualityComparer<CompilerMessage>
    {
        public bool Equals(CompilerMessage x, CompilerMessage y)
        {
            return x.Text == y.Text &&
                   x.HasNestedMessages == y.HasNestedMessages &&
                   x.Location.CompareTo(y.Location) == 0 &&
                   x.Number == y.Number &&
                   x.Kind == y.Kind &&
                   x.Type == y.Type;
        }

        public int GetHashCode(CompilerMessage obj)
        {
            return obj.Text.GetHashCode() ^ obj.Number ^ obj.Location.GetHashCode();
        }
    }
}