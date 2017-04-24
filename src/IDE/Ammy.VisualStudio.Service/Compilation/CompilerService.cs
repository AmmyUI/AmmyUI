using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using EnvDTE;
using Ammy.Build;
using Ammy.Language;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Nitra;
using Nitra.Declarations;
using Nitra.Runtime.Reflection;
using VSLangProj;
using Reference = VSLangProj.Reference;
using System.Windows;
using Ammy.VisualStudio.Service.Settings;

namespace Ammy.VisualStudio.Service.Compilation
{
    public partial class CompilerService : INeedLogging
    {
        private static CompilerService _instance;
        public static CompilerService Instance => _instance ?? (_instance = new CompilerService());
        
        public CompilerListeners Listeners { get; }

        private readonly AmmyCompiler _compiler;
        private readonly AdbService _adbService = new AdbService();
        private readonly Dictionary<string, DateTime> _referenceCache = new Dictionary<string, DateTime>();
        private readonly CompilerServiceRuntimeUpdate _compilerServiceRuntimeUpdate;
        private readonly List<ITextDocument> _openDocuments = new List<ITextDocument>(); 
        private readonly ConcurrentDictionary<string, AmmyProject> _projectCaches = new ConcurrentDictionary<string, AmmyProject>(); 
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<object> _eventContainers = new List<object>();
         
        public XamlProjectMeta PreviousUpdateMeta { get; set; }

        public CompileResult LatestResult { get; private set; }

        private readonly Subject<bool> _isCompiling = new Subject<bool>();
        public IObservable<bool> IsCompiling => _isCompiling.AsObservable();

        private readonly Subject<Exception> _compilationErrors = new Subject<Exception>();
        public IObservable<Exception> CompilationFatalErrors => _compilationErrors.AsObservable();

        private readonly Subject<CompileResult> _compilations = new Subject<CompileResult>();
        public IObservable<CompileResult> Compilations => _compilations.Where(val => val != null)
                                                                       .ObserveOn(SynchronizationContext.Current)
                                                                       .AsObservable();
        
        private readonly CompilationQueue _compilationQueue = new CompilationQueue();
        private bool _isDebugging;

        private CompilerService()
        {
            Listeners = new CompilerListeners(this);

            _compiler = new AmmyCompiler();
            _compilerServiceRuntimeUpdate = new CompilerServiceRuntimeUpdate(this);

            // Recompile if previous compilation didn't include BAML compilation
            // BAML compilation ensures we get XAML compiler errors if any
            _compilations.Throttle(TimeSpan.FromSeconds(1.5))
                         .Where(res => res != null)
                         .Where(res => !res.NeedBamlGeneration)
                         .Where(res => res.Files.Any())
                         .Subscribe(res => Compile(res.Files[0].FullName, false, true));

            var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
            if (dte != null)
                DteEventHandlers(dte);
        }

        public void Compile(string filename, bool sendRuntimeUpdate, bool needBamlCompilation = false)
        {
            this.LogDebugInfo($"Compile request with sendRuntimeUpdate={sendRuntimeUpdate} from " + filename);

            _compilationQueue.Push(filename, () => {
                try {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    
                    this.LogDebugInfo("Compilation started: " + filename);

                    _isCompiling.OnNext(true);

                    LatestResult = CompileImpl(filename, sendRuntimeUpdate, needBamlCompilation);

                    this.LogDebugInfo("Compilation ended: " + filename + " " + stopwatch.ElapsedMilliseconds + "ms");

                    _compilations.OnNext(LatestResult);

                    this.LogDebugInfo("Compilation took: " + stopwatch.ElapsedMilliseconds + "ms");
                } catch (CompilerServiceException) {
                    //_compilationErrors.OnNext(e);
                } catch (Exception e) {
                    this.LogDebugInfo(e.ToString());
                    _compilationErrors.OnNext(e);
                } finally {
                    _isCompiling.OnNext(false);
                }
            });
        }
        
        private CompileResult CompileImpl(string filename, bool sendRuntimeUpdate, bool needBamlCompilation = false)
        {
            if (sendRuntimeUpdate)
                needBamlCompilation = true;

            string projectName;
            var ammyProject = GetAmmyProject(filename, out projectName);
            
            var result = _compiler.Compile(ammyProject, false, needBamlCompilation);
            result.ProjectName = projectName;

            if (result.IsSuccess && sendRuntimeUpdate) {
                var objAbsolutePath = Path.Combine(ammyProject.FsProject.ProjectDir, ammyProject.OutputPath);
                _compilerServiceRuntimeUpdate.SendRuntimeUpdate(result, objAbsolutePath, ammyProject.AssemblyName);
            }
            
            return result;
        }

        private AmmyProject GetAmmyProject(string filename, out string projectName)
        {
            AmmyProject cachedProject;

            var genericProject = DteHelpers.GetProjectByFilename(filename);

            if (genericProject == null || genericProject.Kind == EnvDTE.Constants.vsProjectKindMisc)
                throw new CompilerServiceException("Project not found for file: " + filename);

            projectName = genericProject.Name;

            if (_projectCaches.TryGetValue(projectName, out cachedProject)) {
                var cachedReferences = cachedProject.References.Select(r => r.Path).ToList();
                var cachedFiles = GetCachedFiles(cachedProject);

                var newProject = new AmmyProject(cachedReferences, cachedFiles, cachedProject.CSharpProject, cachedProject.FsProject.Data, cachedProject.FsProject.ProjectDir, cachedProject.OutputPath, cachedProject.RootNamespace, cachedProject.AssemblyName, cachedProject.TargetPath);
                return newProject;
            }
            
            var projectItems = DteHelpers.GetFileList(genericProject);
            var ammyFileMetas = GetAmmyFileMetas(projectItems);
            var sourceFilenames = projectItems.Select(pi => DteHelpers.GetFilename(pi)).ToArray();
            var project = (VSProject) genericProject.Object;
            var fullPath = genericProject.Properties.Item("FullPath").Value.ToString();
            var objRelativePath = genericProject.ConfigurationManager.ActiveConfiguration.Properties.Item("IntermediatePath").Value.ToString();
            var outputPath = genericProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var outputFileName = genericProject.Properties.Item("OutputFileName").Value.ToString();
            var targetPath = Path.Combine(fullPath, outputPath, outputFileName);
            var rootNamespace = genericProject.Properties.Item("RootNamespace").Value.ToString();
            var assemblyName = genericProject.Properties.Item("AssemblyName").Value.ToString();
            //var hostAssemblyPath = Path.Combine(outputDir, outputFileName);
            var references = GetReferences(project);
            var compilationData = NeedReferenceRefresh(references) ? null : LatestResult?.CompilationData;
            var csharpProject = new CSharpProject(fullPath, sourceFilenames);
            var ammyProject = new AmmyProject(references, ammyFileMetas, csharpProject, compilationData, fullPath, objRelativePath, rootNamespace, assemblyName, targetPath);

            _projectCaches[projectName] = ammyProject;
            SubscribeToReferenceChanges(project, projectName);

            return ammyProject;
        }

        private List<AmmyFileMeta> GetCachedFiles(AmmyProject cachedProject)
        {
            var result = new List<AmmyFileMeta>();

            foreach (var file in cachedProject.Files) {
                var fileInfo = new FileInfo(file.FilePath);

                if (!fileInfo.Exists)
                    continue;

                var cachedVersion = ((ITextSnapshot)file.Meta.Snapshot)?.Version.VersionNumber;
                var currentDoc = _openDocuments.FirstOrDefault(d => d.FilePath == file.FilePath);
                var currentVersion = currentDoc?.TextBuffer?.CurrentSnapshot?.Version?.VersionNumber;
                var isSnapshotUpdated = cachedVersion < currentVersion;

                if (fileInfo.LastWriteTime > file.LastWriteTime || isSnapshotUpdated) {
                    result.Add(ToAmmyFileMeta(file.FilePath, file.Meta.ProjectItem));
                    this.LogDebugInfo("Using new " + file.OutputFilename);
                } else {
                    file.DeepResetProperties();
                    file.BamlCompilerMessages.Clear();
                    result.Add(file.Meta);
                    //if (file.Meta.Snapshot == null)
                    TryAssignSnapshot(file);
                    this.LogDebugInfo("Using cached " + file.OutputFilename);
                }
            }

            return result;
        }

        private void TryAssignSnapshot(AmmyFile<Top> file)
        {
            var doc = _openDocuments.FirstOrDefault(d => d.FilePath.Equals(file.FilePath, StringComparison.InvariantCultureIgnoreCase));
            if (doc != null && doc.TextBuffer?.CurrentSnapshot?.Version != null)
                file.Meta.UpdateSnapshot(doc.TextBuffer.CurrentSnapshot, doc.TextBuffer.CurrentSnapshot.Version.VersionNumber);
        }

        private IReadOnlyList<AmmyFileMeta> GetAmmyFileMetas(ProjectItem[] projectItems)
        {
            return projectItems.Where(pi => string.Equals(Path.GetExtension(DteHelpers.GetFilename(pi)), ".ammy", StringComparison.InvariantCultureIgnoreCase))
                               .SelectMany(pi => new[] { ToAmmyFileMeta(DteHelpers.GetFilename(pi), pi) })
                               .ToList();
        }

        public void DocumentOpened(ITextDocument document)
        {
            _openDocuments.Add(document);

            Compile(document.FilePath, false);
        }

        public void DocumentClosed(ITextDocument document)
        {
            _openDocuments.Remove(document);
        }

        private AmmyFileMeta ToAmmyFileMeta(string filename, object projectItem)
        {
            var doc = _openDocuments.FirstOrDefault(d => d.FilePath.Equals(filename, StringComparison.InvariantCultureIgnoreCase));
            if (doc != null) {
                var currentSnapshot = doc.TextBuffer.CurrentSnapshot;
                return new AmmyFileMeta(filename, currentSnapshot, currentSnapshot.GetText(), currentSnapshot.Version.VersionNumber, projectItem);
            }

            return new AmmyFileMeta(filename, projectItem);
        }

        private static List<string> GetReferences(VSProject project)
        {
            return project.References
                          .Cast<Reference>()
                          .Where(rf => rf.Type == prjReferenceType.prjReferenceTypeAssembly)
                          .SelectMany(rf => new[] {rf.Path})
                          .Where(reference => !string.IsNullOrWhiteSpace(reference))
                          //.Concat(new[] {hostAssemblyPath})
                          .ToList();
        }

        private bool NeedReferenceRefresh(IReadOnlyList<string> references)
        {
            var currentLastWriteTimes = new Dictionary<string, DateTime>();

            foreach (var reference in references)
                currentLastWriteTimes[reference] = File.GetLastWriteTime(reference);

            var needRefresh = false;

            if (_referenceCache.Count != currentLastWriteTimes.Count) {
                needRefresh = true;
            } else {
                foreach (var reference in currentLastWriteTimes) {
                    DateTime cachedLastWriteTime;

                    if (_referenceCache.TryGetValue(reference.Key, out cachedLastWriteTime)) {
                        if (cachedLastWriteTime != reference.Value) {
                            needRefresh = true;
                            break;
                        }
                    } else {
                        needRefresh = true;
                        break;
                    }
                }
            }

            foreach (var reference in currentLastWriteTimes)
                _referenceCache[reference.Key] = reference.Value;

            return needRefresh;
        }

        public void AfterNextCompilation(Action<CompileResult> action, SynchronizationContext context = null)
        {
            var observable = Compilations.Take(1);

            if (context == null)
                observable.Subscribe(action);
            else
                observable.ObserveOn(context)
                          .Subscribe(action);
        }

        public IAst[] GetCurrentAstStack(string fileName, int caretPosition)
        {
            var file = LatestResult?.GetFile(fileName);

            if (file == null)
                return new IAst[0];

            var visitor = new FindNodeAstVisitor(new NSpan(caretPosition, caretPosition));
            visitor.Visit(file.Ast);

            return visitor.Stack.ToArray();
        }

        private void SubscribeToReferenceChanges(VSProject project, string projectName)
        {
            var referencesEvents = project.Events.ReferencesEvents;
            AmmyProject cachedProject;

            referencesEvents.ReferenceAdded += reference => {
                _projectCaches.TryRemove(projectName, out cachedProject);
            };
            referencesEvents.ReferenceChanged += reference => {
                _projectCaches.TryRemove(projectName, out cachedProject);
            };
            referencesEvents.ReferenceRemoved += reference => {
                _projectCaches.TryRemove(projectName, out cachedProject);
            };
            _eventContainers.Add(referencesEvents);
        }

        private void DteEventHandlers(DTE dte)
        {

            var events2 = dte.Events as Events2;

            if (events2 == null)
                return;

            var solutionEvents = events2.SolutionEvents;
            var piEvents = events2.ProjectItemsEvents;
            var dEvents = events2.DebuggerEvents;
            var documentEvents = events2.DocumentEvents;

            AmmyProject project;
            
            documentEvents.DocumentSaved += document => {
                if (document.FullName.EndsWith(".cs"))
                    LatestResult?.AmmyProject.CSharpProject.MarkAsDirty(document.FullName);
            };
            
            solutionEvents.ProjectAdded += project1 => {
                var ammyFilename = DteHelpers.GetFileList(project1)
                                             .Select(pi => DteHelpers.GetFilename(pi))
                                             .FirstOrDefault(f => f.EndsWith(".ammy"));

                if (ammyFilename != null)
                    Compile(ammyFilename, false);
            };
            
            dEvents.OnEnterDesignMode += reason => {
                LatestResult?.AmmyProject.Context.ClearBindingConverters();
                _isDebugging = false;
            };
            
            dEvents.OnEnterRunMode += reason => {
                LatestResult?.AmmyProject.Context.ClearBindingConverters();
                
                if (!_isDebugging && LatestResult?.AmmyProject.PlatformName == "XamarinForms") {
                    var result = _adbService.SetupAdbForwarding();

                    if (!result && !AmmySettings.SuppressAdbWarning)
                        MessageBox.Show("Ammy cannot find `adb.exe`" + Environment.NewLine + Environment.NewLine +
                                        "You can specify `adb.exe` path in settings window (top right corner)" + Environment.NewLine +
                                        "Or you can also suppress this warning in the same settings window");
                }

                _isDebugging = true;
            };

            piEvents.ItemAdded += item => {
                _projectCaches.TryRemove(item.ContainingProject.Name, out project);
            };
            piEvents.ItemRemoved += item => {
                _projectCaches.TryRemove(item.ContainingProject.Name, out project);
            };
            piEvents.ItemRenamed += (item, name) => {
                _projectCaches.TryRemove(item.ContainingProject.Name, out project);
            };

            _eventContainers.Add(piEvents);
            _eventContainers.Add(solutionEvents);
            _eventContainers.Add(dEvents);
            _eventContainers.Add(documentEvents);
        }

        class CompilerServiceException : Exception
        {
            public CompilerServiceException(string message) : base(message)
            {}
        }
    }
}