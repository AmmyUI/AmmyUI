using System;
using System.Threading.Tasks;
using Ammy.VisualStudio.Service.Extensions;

namespace Ammy.VisualStudio.Service.Compilation
{
    class CompilationQueue
    {
        private readonly OneSlotStack<string, Action> _stack = new OneSlotStack<string, Action>();
        private Task _compilationTask = Task.FromResult(true);
        
        public void Push(string projectName, Action compilation)
        {
            _stack.Push(projectName, compilation);

            if (_compilationTask.IsCompleted)
                _compilationTask = _compilationTask.ContinueWith(ResumeCompilation);
        }

        private void ResumeCompilation(Task _)
        {
            while (true) {
                var compilationAction = _stack.Pop();

                // If stack is empty, quit
                if (compilationAction == null)
                    break;
                
                compilationAction();
            }
        }
    }
}