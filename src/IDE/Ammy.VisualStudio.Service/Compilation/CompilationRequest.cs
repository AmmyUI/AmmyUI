using Microsoft.VisualStudio.Text;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class CompilationRequest
    {
        public string ProjectName { get; private set; }
        public bool SendRuntimeUpdate { get; private set; }

        public CompilationRequest(string projectName,  bool sendRuntimeUpdate)
        {
            ProjectName = projectName;
            SendRuntimeUpdate = sendRuntimeUpdate;
        }
    }
}