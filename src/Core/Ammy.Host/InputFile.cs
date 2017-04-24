using Ammy.Language;
using Nitra.ProjectSystem;

namespace Ammy
{
    class InputFile : FsFile<Start>
    {
        public override int Id { get; }

        public InputFile(int id, string filePath, Nitra.Language language, FsProject<Start> fsProject = null, FileStatistics statistics = null) : base(filePath, language, fsProject, statistics)
        {
            Id = id;
        }
    }
}