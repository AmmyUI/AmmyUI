using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace Ammy.VisualStudio.Service
{
    internal static class TaskManager
    {
        private static volatile ErrorListProvider _errorListProvider;
        private static readonly object SyncRoot = new object();
        private static readonly List<ErrorTask> CurrentTasks = new List<ErrorTask>();

        public static void Initialize(IServiceProvider serviceProvider)
        {
            if (_errorListProvider == null)
                lock (SyncRoot)
                    if (_errorListProvider == null)
                        _errorListProvider = new ErrorListProvider(serviceProvider);
        }

        public static void AddError(string message, string filename, int line, int column)
        {
            AddTask(message, TaskErrorCategory.Error, filename, line, column);
        }

        public static void AddWarning(string message, string filename, int line, int column)
        {
            AddTask(message, TaskErrorCategory.Warning, filename, line, column);
        }

        public static void AddMessage(string message, string filename, int line, int column)
        {
            AddTask(message, TaskErrorCategory.Message, filename, line, column);
        }

        public static void ClearMessages()
        {
            lock (SyncRoot) {
                _errorListProvider.SuspendRefresh();

                try {
                    foreach (var task in CurrentTasks) {
                        task.Navigate -= ErrorTaskOnNavigate;
                        _errorListProvider.Tasks.Remove(task);
                    }

                    CurrentTasks.Clear();
                } catch {
                    // Ignore errors
                } finally {
                    _errorListProvider.ResumeRefresh();
                }
            }
        }

        private static void AddTask(string message, TaskErrorCategory category, string filename, int line, int column)
        {
            var errorTask = new ErrorTask {
                Category = TaskCategory.User,
                ErrorCategory = category,
                Text = message,
                Document = filename,
                Line = line,
                Column = column
            };
            
            errorTask.Navigate += ErrorTaskOnNavigate;

            lock (SyncRoot) {
                CurrentTasks.Add(errorTask);

                _errorListProvider.Tasks.Add(errorTask);
            }
        }

        private static void ErrorTaskOnNavigate(object sender, EventArgs eventArgs)
        {
            try {
                var task = (ErrorTask) sender;
                _errorListProvider.Navigate(task, new Guid(EnvDTE.Constants.vsViewKindCode));
            } catch {
                // Couldn't navigate
            }
        }
    }
}