using System;
using System.Collections.Generic;
using Ammy.Build;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class CompilerListeners
    {
        private readonly List<ICompilationListener> _listenerList = new List<ICompilationListener>();
        private CompileResult _latestResult;

        public CompilerListeners(CompilerService compilerService)
        {
            compilerService.Compilations
                           .Subscribe(UpdateListeners);
        }

        private void UpdateListeners(CompileResult result)
        {
            _latestResult = result;

            foreach (var listener in _listenerList)
                UpdateListener(result, listener);
        }

        private static void UpdateListener(CompileResult result, ICompilationListener listener)
        {
            var file = result.GetFile(listener.FilePath);
            if (file != null)
                listener.Update(file);
        }

        public void AddListener(ICompilationListener listener)
        {
            _listenerList.Add(listener);

            if (_latestResult != null)
                UpdateListener(_latestResult, listener);
        }

        public void RemoveListener(ICompilationListener listener)
        {
            _listenerList.Remove(listener);
        }
    }
}