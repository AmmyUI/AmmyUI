using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Ammy.VisualStudio.Service
{
    public class Logger
    {
        private readonly string _filename = Path.ChangeExtension(Path.GetTempFileName(), "ammy.log");
        private readonly Subject<string> _logMessages = new Subject<string>();
        private readonly StreamWriter _writer;

        private static Logger _instance;
        public static Logger Instance { get { return _instance ?? (_instance = new Logger()); } }

        private Logger()
        {
            _writer = File.CreateText(_filename);
            _writer.AutoFlush = true;

            _logMessages.ObserveOn(new EventLoopScheduler())
                        .Subscribe(msg => {
                            _writer.WriteLine(msg);
                        });
        }

        public void Info(string info)
        {
            var time = GetTime();
            _logMessages.OnNext(time + ":  " + info); 
        }

        private string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss,fff");
        }

        internal void Exception(string info, Exception e)
        {
            var time = GetTime();
            _logMessages.OnNext(time + ":  " + info + Environment.NewLine + e.ToString());
        }
    }

    public static class LoggerExtensions
    {
        public static void LogDebugInfo(this INeedLogging _, string info)
        {
            Debug.WriteLine(info);
            Logger.Instance.Info(info);
        }

        public static void LogDebugException(this INeedLogging _, string info, Exception e)
        {
            Debug.WriteLine(info);
            Logger.Instance.Exception(info, e);
        }
    }

    public interface INeedLogging
    {}
}