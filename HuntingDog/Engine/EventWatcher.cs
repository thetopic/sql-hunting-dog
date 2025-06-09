
using System;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace DatabaseObjectSearcher {
    public class EventWatcher {
        #region EventWatcher fields
        private WindowEvents _windowsEvents;
        private TextEditorEvents _textEditorEvents;
        private TaskListEvents _taskListEvents;
        private SolutionEvents _solutionEvents;
        private SelectionEvents _selectionEvents;
        private OutputWindowEvents _outputWindowEvents;
        private FindEvents _findEvents;
        private DTEEvents _dteEvents;
        private DocumentEvents _documentEvents;
        private DebuggerEvents _debuggerEvents;
        private CommandEvents _commandEvents;
        private BuildEvents _buildEvents;
        private ProjectItemsEvents _miscFilesEvents;
        private ProjectItemsEvents _solutionItemsEvents;
        private ProjectItemsEvents _globalProjectItemsEvents;
        private ProjectsEvents _globalProjectsEvents;
        private TextDocumentKeyPressEvents _textDocumentKeyPressEvents;
        private CodeModelEvents _codeModelEvents;
        private WindowVisibilityEvents _windowVisibilityEvents;
        private DebuggerProcessEvents _debuggerProcessEvents;
        private DebuggerExpressionEvaluationEvents _debuggerExpressionEvaluationEvents;
        private PublishEvents _publishEvents;
        private OutputWindowPane _outputWindowPane;
        #endregion

        _DTE applicationObject;

        public void Attach(_DTE app) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            applicationObject = app;

            Events events = applicationObject.Events;
            OutputWindow outputWindow = (OutputWindow)applicationObject.Windows.Item(Constants.vsWindowKindOutput).Object;


            //IObjectExplorerService objectExplorer = ServiceCache.GetObjectExplorer();
            //var provider = (IObjectExplorerEventProvider)objectExplorer.GetService(typeof(IObjectExplorerEventProvider));

            //provider.SelectionChanged += new NodesChangedEventHandler(provider_SelectionChanged);

            _outputWindowPane = outputWindow.OutputWindowPanes.Add("DTE Event Information - C# Event Watcher");

            //Retrieve the event objects from the automation model
            _windowsEvents = events.get_WindowEvents(null);
            _textEditorEvents = events.get_TextEditorEvents(null);
            _taskListEvents = events.get_TaskListEvents("");
            _solutionEvents = events.SolutionEvents;
            _selectionEvents = events.SelectionEvents;
            _outputWindowEvents = events.get_OutputWindowEvents("");
            _findEvents = events.FindEvents;
            _dteEvents = events.DTEEvents;
            _documentEvents = events.get_DocumentEvents(null);
            _debuggerEvents = events.DebuggerEvents;
            _commandEvents = events.get_CommandEvents("{00000000-0000-0000-0000-000000000000}", 0);
            _buildEvents = events.BuildEvents;
            _miscFilesEvents = events.MiscFilesEvents;
            _solutionItemsEvents = events.SolutionItemsEvents;

            _globalProjectItemsEvents = ((Events2)events).ProjectItemsEvents;
            _globalProjectsEvents = ((Events2)events).ProjectsEvents;
            _textDocumentKeyPressEvents = ((Events2)events).get_TextDocumentKeyPressEvents(null);
            _codeModelEvents = ((Events2)events).get_CodeModelEvents(null);
            _windowVisibilityEvents = ((Events2)events).get_WindowVisibilityEvents(null);
            _debuggerProcessEvents = ((Events2)events).DebuggerProcessEvents;
            _debuggerExpressionEvaluationEvents = ((Events2)events).DebuggerExpressionEvaluationEvents;
            _publishEvents = ((Events2)events).PublishEvents;

            //Connect to each delegate exposed from each object retrieved above
            _windowsEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(this.WindowActivated);
            _windowsEvents.WindowClosing += new _dispWindowEvents_WindowClosingEventHandler(this.WindowClosing);
            _windowsEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(this.WindowCreated);
            _windowsEvents.WindowMoved += new _dispWindowEvents_WindowMovedEventHandler(this.WindowMoved);
            _textEditorEvents.LineChanged += new _dispTextEditorEvents_LineChangedEventHandler(this.LineChanged);
            _taskListEvents.TaskAdded += new _dispTaskListEvents_TaskAddedEventHandler(this.TaskAdded);
            _taskListEvents.TaskModified += new _dispTaskListEvents_TaskModifiedEventHandler(this.TaskModified);
            _taskListEvents.TaskNavigated += new _dispTaskListEvents_TaskNavigatedEventHandler(this.TaskNavigated);
            _taskListEvents.TaskRemoved += new _dispTaskListEvents_TaskRemovedEventHandler(this.TaskRemoved);
            _solutionEvents.AfterClosing += new _dispSolutionEvents_AfterClosingEventHandler(this.AfterClosing);
            _solutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(this.BeforeClosing);
            _solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(this.Opened);
            _solutionEvents.ProjectAdded += new _dispSolutionEvents_ProjectAddedEventHandler(this.ProjectAdded);
            _solutionEvents.ProjectRemoved += new _dispSolutionEvents_ProjectRemovedEventHandler(this.ProjectRemoved);
            _solutionEvents.ProjectRenamed += new _dispSolutionEvents_ProjectRenamedEventHandler(this.ProjectRenamed);
            _solutionEvents.QueryCloseSolution += new _dispSolutionEvents_QueryCloseSolutionEventHandler(this.QueryCloseSolution);
            _solutionEvents.Renamed += new _dispSolutionEvents_RenamedEventHandler(this.Renamed);
            _selectionEvents.OnChange += new _dispSelectionEvents_OnChangeEventHandler(this.OnChange);
            _outputWindowEvents.PaneAdded += new _dispOutputWindowEvents_PaneAddedEventHandler(this.PaneAdded);
            _outputWindowEvents.PaneClearing += new _dispOutputWindowEvents_PaneClearingEventHandler(this.PaneClearing);
            _outputWindowEvents.PaneUpdated += new _dispOutputWindowEvents_PaneUpdatedEventHandler(this.PaneUpdated);
            _findEvents.FindDone += new _dispFindEvents_FindDoneEventHandler(this.FindDone);
            _dteEvents.ModeChanged += new _dispDTEEvents_ModeChangedEventHandler(this.ModeChanged);
            _dteEvents.OnBeginShutdown += new _dispDTEEvents_OnBeginShutdownEventHandler(this.OnBeginShutdown);
            _dteEvents.OnMacrosRuntimeReset += new _dispDTEEvents_OnMacrosRuntimeResetEventHandler(this.OnMacrosRuntimeReset);
            _dteEvents.OnStartupComplete += new _dispDTEEvents_OnStartupCompleteEventHandler(this.OnStartupComplete);
            _documentEvents.DocumentClosing += new _dispDocumentEvents_DocumentClosingEventHandler(this.DocumentClosing);
            _documentEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(this.DocumentOpened);
            _documentEvents.DocumentOpening += new _dispDocumentEvents_DocumentOpeningEventHandler(this.DocumentOpening);
            _documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(this.DocumentSaved);
            _debuggerEvents.OnContextChanged += new _dispDebuggerEvents_OnContextChangedEventHandler(this.OnContextChanged);
            _debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(this.OnEnterBreakMode);
            _debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(this.OnEnterDesignMode);
            _debuggerEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(this.OnEnterRunMode);
            _debuggerEvents.OnExceptionNotHandled += new _dispDebuggerEvents_OnExceptionNotHandledEventHandler(this.OnExceptionNotHandled);
            _debuggerEvents.OnExceptionThrown += new _dispDebuggerEvents_OnExceptionThrownEventHandler(this.OnExceptionThrown);
            _commandEvents.AfterExecute += new _dispCommandEvents_AfterExecuteEventHandler(this.AfterExecute);
            _commandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(this.BeforeExecute);
            _buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
            _buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(this.OnBuildDone);
            _buildEvents.OnBuildProjConfigBegin += new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(this.OnBuildProjConfigBegin);
            _buildEvents.OnBuildProjConfigDone += new _dispBuildEvents_OnBuildProjConfigDoneEventHandler(this.OnBuildProjConfigDone);
            _miscFilesEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(this.MiscFilesEvents_ItemAdded);
            _miscFilesEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(this.MiscFilesEvents_ItemRemoved);
            _miscFilesEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(this.MiscFilesEvents_ItemRenamed);
            _solutionItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(this.SolutionItemsEvents_ItemAdded);
            _solutionItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(this.SolutionItemsEvents_ItemRemoved);
            _solutionItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(this.SolutionItemsEvents_ItemRenamed);
            _globalProjectItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(GlobalProjectItemsEvents_ItemAdded);
            _globalProjectItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(GlobalProjectItemsEvents_ItemRemoved);
            _globalProjectItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(GlobalProjectItemsEvents_ItemRenamed);
            _globalProjectsEvents.ItemAdded += new _dispProjectsEvents_ItemAddedEventHandler(GlobalProjectsEvents_ItemAdded);
            _globalProjectsEvents.ItemRemoved += new _dispProjectsEvents_ItemRemovedEventHandler(GlobalProjectsEvents_ItemRemoved);
            _globalProjectsEvents.ItemRenamed += new _dispProjectsEvents_ItemRenamedEventHandler(GlobalProjectsEvents_ItemRenamed);
            _textDocumentKeyPressEvents.AfterKeyPress += new _dispTextDocumentKeyPressEvents_AfterKeyPressEventHandler(AfterKeyPress);
            _textDocumentKeyPressEvents.BeforeKeyPress += new _dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler(BeforeKeyPress);
            _codeModelEvents.ElementAdded += new _dispCodeModelEvents_ElementAddedEventHandler(ElementAdded);
            _codeModelEvents.ElementChanged += new _dispCodeModelEvents_ElementChangedEventHandler(ElementChanged);
            _codeModelEvents.ElementDeleted += new _dispCodeModelEvents_ElementDeletedEventHandler(ElementDeleted);
            _windowVisibilityEvents.WindowHiding += new _dispWindowVisibilityEvents_WindowHidingEventHandler(WindowHiding);
            _windowVisibilityEvents.WindowShowing += new _dispWindowVisibilityEvents_WindowShowingEventHandler(WindowShowing);
            _debuggerExpressionEvaluationEvents.OnExpressionEvaluation += new _dispDebuggerExpressionEvaluationEvents_OnExpressionEvaluationEventHandler(OnExpressionEvaluation);
            _debuggerProcessEvents.OnProcessStateChanged += new _dispDebuggerProcessEvents_OnProcessStateChangedEventHandler(OnProcessStateChanged);
            _publishEvents.OnPublishBegin += new _dispPublishEvents_OnPublishBeginEventHandler(OnPublishBegin);
            _publishEvents.OnPublishDone += new _dispPublishEvents_OnPublishDoneEventHandler(OnPublishDone);


        }

        #region Events

        void provider_SelectionChanged(object sender, NodesChangedEventArgs args) {
            //_outputWindowPane.OutputString("SelectionChanged \n");
            //var res = "";
            //foreach (var n in args.ChangedNodes)
            //{
            //    if (n.Parent == null)
            //        res += " server " + n.Name + n.Connection.ServerName;
            //    else
            //        res += n.Name;
            //}

            //_outputWindowPane.OutputString(res);
        }




        #region EventWatcher - Methods
        //WindowEvents
        public void WindowClosing(Window closingWindow) {
            //_outputWindowPane.OutputString("WindowEvents, WindowClosing\n");
            //_outputWindowPane.OutputString("\tWindow: " + closingWindow.Caption + "\n");
        }

        public void WindowActivated(Window gotFocus, Window lostFocus) {
            //_outputWindowPane.OutputString("WindowEvents, WindowActivated\n");
            //_outputWindowPane.OutputString("\tWindow receiving focus: " + gotFocus.Caption + "\n");
            //_outputWindowPane.OutputString("\tWindow that lost focus: " + lostFocus.Caption + "\n");
        }

        public void WindowCreated(Window window) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("WindowEvents, WindowCreated\n");
            _outputWindowPane.OutputString("\tWindow: " + window.Caption + "\n");
        }

        public void WindowMoved(Window window, int top, int left, int width, int height) {
            //_outputWindowPane.OutputString("WindowEvents, WindowMoved\n");
            //_outputWindowPane.OutputString("\tWindow: " + window.Caption + "\n");
            // _outputWindowPane.OutputString("\tLocation: (" + top.ToString() + " , " + left.ToString() + " , " + width.ToString() + " , " + height.ToString() + ")\n");
        }

        //TextEditorEvents
        public void LineChanged(TextPoint startPoint, TextPoint endPoint, int hint) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            vsTextChanged textChangedHint = (vsTextChanged)hint;

            _outputWindowPane.OutputString("TextEditorEvents, LineChanged\n");
            _outputWindowPane.OutputString("\tDocument: " + startPoint.Parent.Parent.Name + "\n");
            _outputWindowPane.OutputString("\tChange hint: " + textChangedHint.ToString() + "\n");
        }

        //TaskListEvents
        public void TaskAdded(TaskItem taskItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TaskListEvents, TaskAdded\n");
            _outputWindowPane.OutputString("\tTask description: " + taskItem.Description + "\n");
        }

        public void TaskModified(TaskItem taskItem, vsTaskListColumn columnModified) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TaskListEvents, TaskModified\n");
            _outputWindowPane.OutputString("\tTask description: " + taskItem.Description + "\n");
        }

        public void TaskNavigated(TaskItem taskItem, ref bool navigateHandled) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TaskListEvents, TaskNavigated\n");
            _outputWindowPane.OutputString("\tTask description: " + taskItem.Description + "\n");
        }

        public void TaskRemoved(TaskItem taskItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TaskListEvents, TaskRemoved\n");
            _outputWindowPane.OutputString("\tTask description: " + taskItem.Description + "\n");
        }

        //SolutionEvents
        public void AfterClosing() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, AfterClosing\n");
        }

        public void BeforeClosing() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, BeforeClosing\n");
        }

        public void Opened() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, Opened\n");
        }

        public void ProjectAdded(Project project) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, ProjectAdded\n");
            _outputWindowPane.OutputString("\tProject: " + project.UniqueName + "\n");
        }

        public void ProjectRemoved(Project project) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, ProjectRemoved\n");
            _outputWindowPane.OutputString("\tProject: " + project.UniqueName + "\n");
        }

        public void ProjectRenamed(Project project, string oldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, ProjectRenamed\n");
            _outputWindowPane.OutputString("\tProject: " + project.UniqueName + "\n");
        }

        public void QueryCloseSolution(ref bool cancel) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, QueryCloseSolution\n");
        }

        public void Renamed(string oldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionEvents, Renamed\n");
        }

        //SelectionEvents
        public void OnChange() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SelectionEvents, OnChange\n");

            int count = applicationObject.SelectedItems.Count;

            for (int i = 1; i <= applicationObject.SelectedItems.Count; i++) {
                _outputWindowPane.OutputString("Item name: " + applicationObject.SelectedItems.Item(i).Name + "\n");
            }
        }

        //OutputWindowEvents
        public void PaneAdded(OutputWindowPane pane) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("OutputWindowEvents, PaneAdded\n");
            _outputWindowPane.OutputString("\tPane: " + pane.Name + "\n");
        }

        public void PaneClearing(OutputWindowPane pane) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("OutputWindowEvents, PaneClearing\n");
            _outputWindowPane.OutputString("\tPane: " + pane.Name + "\n");
        }

        public void PaneUpdated(OutputWindowPane pane) {
            //Dont want to do this one, or we will end up in a recursive call:
            //outputWindowPane.OutputString("OutputWindowEvents, PaneUpdated\n");
            //outputWindowPane.OutputString("\tPane: " + pane.Name + "\n");
        }

        //FindEvents
        public void FindDone(vsFindResult result, bool cancelled) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("FindEvents, FindDone\n");
        }

        //DTEEvents
        public void ModeChanged(vsIDEMode LastMode) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DTEEvents, ModeChanged\n");
        }

        public void OnBeginShutdown() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DTEEvents, OnBeginShutdown\n");
        }

        public void OnMacrosRuntimeReset() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DTEEvents, OnMacrosRuntimeReset\n");
        }

        public void OnStartupComplete() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DTEEvents, OnStartupComplete\n");
        }

        //DocumentEvents
        public void DocumentClosing(Document document) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DocumentEvents, DocumentClosing\n");
            _outputWindowPane.OutputString("\tDocument: " + document.Name + "\n");
        }

        public void DocumentOpened(Document document) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DocumentEvents, DocumentOpened\n");
            _outputWindowPane.OutputString("\tDocument: " + document.Name + "\n");
        }

        public void DocumentOpening(string documentPath, bool ReadOnly) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DocumentEvents, DocumentOpening\n");
            _outputWindowPane.OutputString("\tPath: " + documentPath + "\n");
        }

        public void DocumentSaved(Document document) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DocumentEvents, DocumentSaved\n");
            _outputWindowPane.OutputString("\tDocument: " + document.Name + "\n");
        }

        //DebuggerEvents
        public void OnContextChanged(EnvDTE.Process NewProcess, Program NewProgram, Thread NewThread, EnvDTE.StackFrame NewStackFrame) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DebuggerEvents, OnContextChanged\n");
        }

        public void OnEnterBreakMode(dbgEventReason reason, ref dbgExecutionAction executionAction) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            executionAction = dbgExecutionAction.dbgExecutionActionDefault;
            _outputWindowPane.OutputString("DebuggerEvents, OnEnterBreakMode\n");
        }

        public void OnEnterDesignMode(dbgEventReason Reason) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DebuggerEvents, OnEnterDesignMode\n");
        }

        public void OnEnterRunMode(dbgEventReason Reason) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DebuggerEvents, OnEnterRunMode\n");
        }

        public void OnExceptionNotHandled(string exceptionType, string name, int code, string description, ref dbgExceptionAction exceptionAction) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            exceptionAction = dbgExceptionAction.dbgExceptionActionDefault;
            _outputWindowPane.OutputString("DebuggerEvents, OnExceptionNotHandled\n");
        }

        public void OnExceptionThrown(string exceptionType, string name, int code, string description, ref dbgExceptionAction exceptionAction) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            exceptionAction = dbgExceptionAction.dbgExceptionActionDefault;
            _outputWindowPane.OutputString("DebuggerEvents, OnExceptionThrown\n");
        }

        //CommandEvents
        public void AfterExecute(string Guid, int ID, object CustomIn, object CustomOut) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string commandName = "";

            try {
                commandName = applicationObject.Commands.Item(Guid, ID).Name;
            }
            catch {
            }
            _outputWindowPane.OutputString("CommandEvents, AfterExecute\n");
            if (commandName != "")
                _outputWindowPane.OutputString("\tCommand name: " + commandName + "\n");

            Debug.WriteLine("\tCommand name: " + commandName + "\n");
            Console.WriteLine("\tCommand GUID/ID: " + Guid + ", " + ID.ToString() + "\n");
            _outputWindowPane.OutputString("\tCommand GUID/ID: " + Guid + ", " + ID.ToString() + "\n");
        }

        public void BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string commandName = "";

            try {
                commandName = applicationObject.Commands.Item(Guid, ID).Name;
            }
            catch {
            }
            _outputWindowPane.OutputString("CommandEvents, BeforeExecute\n");
            if (commandName != "")
                _outputWindowPane.OutputString("\tCommand name: " + commandName + "\n");

            _outputWindowPane.OutputString("\tCommand GUID/ID: " + Guid + ", " + ID.ToString() + "\n");
        }

        //BuildEvents
        public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("BuildEvents, OnBuildBegin\n");
        }

        public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("BuildEvents, OnBuildDone\n");
        }

        public void OnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("BuildEvents, OnBuildProjConfigBegin\n");
            _outputWindowPane.OutputString("\tProject: " + project + "\n");
            _outputWindowPane.OutputString("\tProject Configuration: " + projectConfig + "\n");
            _outputWindowPane.OutputString("\tPlatform: " + platform + "\n");
            _outputWindowPane.OutputString("\tSolution Configuration: " + solutionConfig + "\n");
        }

        public void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("BuildEvents, OnBuildProjConfigDone\n");
            _outputWindowPane.OutputString("\tProject: " + project + "\n");
            _outputWindowPane.OutputString("\tProject Configuration: " + projectConfig + "\n");
            _outputWindowPane.OutputString("\tPlatform: " + platform + "\n");
            _outputWindowPane.OutputString("\tSolution Configuration: " + solutionConfig + "\n");
            _outputWindowPane.OutputString("\tBuild success: " + success.ToString() + "\n");
        }

        //MiscFilesEvents
        public void MiscFilesEvents_ItemAdded(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("MiscFilesEvents, ItemAdded\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void MiscFilesEvents_ItemRemoved(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("MiscFilesEvents, ItemRemoved\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void MiscFilesEvents_ItemRenamed(ProjectItem projectItem, string OldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("MiscFilesEvents, ItemRenamed\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        //SolutionItemsEvents
        public void SolutionItemsEvents_ItemAdded(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionItemsEvents, ItemAdded\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void SolutionItemsEvents_ItemRemoved(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionItemsEvents, ItemRemoved\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void SolutionItemsEvents_ItemRenamed(ProjectItem projectItem, string OldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("SolutionItemsEvents, ItemRenamed\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }


        //Global ProjectItemsEvents
        public void GlobalProjectItemsEvents_ItemAdded(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectItemsEvents, ItemAdded\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void GlobalProjectItemsEvents_ItemRemoved(ProjectItem projectItem) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectItemsEvents, ItemRemoved\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        public void GlobalProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string OldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectItemsEvents, ItemRenamed\n");
            _outputWindowPane.OutputString("\tProject Item: " + projectItem.Name + "\n");
        }

        //Global ProjectsEvents
        public void GlobalProjectsEvents_ItemAdded(Project project) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectsEvents, ItemAdded\n");
            _outputWindowPane.OutputString("\tProject: " + project.Name + "\n");
        }

        public void GlobalProjectsEvents_ItemRemoved(Project project) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectsEvents, ItemRemoved\n");
            _outputWindowPane.OutputString("\tProject: " + project.Name + "\n");
        }

        public void GlobalProjectsEvents_ItemRenamed(Project project, string OldName) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("GlobalProjectsEvents, ItemRenamed\n");
            _outputWindowPane.OutputString("\tProject: " + project.Name + "\n");
        }


        //TextDocumentKeyPressEvents
        public void AfterKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TextDocumentKeyPressEvents, AfterKeyPress\n");
            _outputWindowPane.OutputString("\tKey: " + Keypress + "\n");
            _outputWindowPane.OutputString("\tSelection: " + Selection.Text + "\n");
            _outputWindowPane.OutputString("\tInStatementCompletion: " + InStatementCompletion.ToString() + "\n");
        }

        public void BeforeKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("TextDocumentKeyPressEvents, BeforeKeyPress\n");
            _outputWindowPane.OutputString("\tKey: " + Keypress + "\n");
            _outputWindowPane.OutputString("\tSelection: " + Selection.Text + "\n");
            _outputWindowPane.OutputString("\tInStatementCompletion: " + InStatementCompletion.ToString() + "\n");
        }

        //CodeModelEvents
        public void ElementAdded(CodeElement Element) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("CodeModelEvents, ElementAdded\n");
            _outputWindowPane.OutputString("\tElement: " + Element.FullName + "\n");
        }

        public void ElementChanged(CodeElement Element, vsCMChangeKind Change) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("CodeModelEvents, ElementChanged\n");
            _outputWindowPane.OutputString("\tElement: " + Element.FullName + "\n");
            _outputWindowPane.OutputString("\tChange: " + Change.ToString() + "\n");
        }

        public void ElementDeleted(object Parent, CodeElement Element) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("CodeModelEvents, ElementDeleted\n");
            _outputWindowPane.OutputString("\tElement: " + Element.FullName + "\n");
            if (Parent is CodeElement) {
                _outputWindowPane.OutputString("\tParent: " + ((CodeElement)Parent).FullName + "\n");
            }
            else if (Parent is ProjectItem) {
                _outputWindowPane.OutputString("\tParent: " + ((ProjectItem)Parent).get_FileNames(0) + "\n");
            }
        }

        //WindowVisibilityEvents
        public void WindowHiding(Window pWindow) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("WindowVisibilityEvents, WindowHiding\n");
            _outputWindowPane.OutputString("\tWindow: " + pWindow.Caption + "\n");
        }

        public void WindowShowing(Window pWindow) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("WindowVisibilityEvents, WindowShowing\n");
            _outputWindowPane.OutputString("\tWindow: " + pWindow.Caption + "\n");
        }

        //DebuggerProcessEvents
        public void OnProcessStateChanged(EnvDTE.Process NewProcess, dbgProcessState processState) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DebuggerProcessEvents, OnProcessStateChanged\n");
            _outputWindowPane.OutputString("\tNew Process: " + NewProcess.Name + "\n");
            _outputWindowPane.OutputString("\tProcess State: " + processState.ToString() + "\n");
        }

        //DebuggerExpressionEvaluationEvents
        public void OnExpressionEvaluation(EnvDTE.Process pProcess, Thread thread, dbgExpressionEvaluationState processState) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("DebuggerExpressionEvaluationEvents, OnExpressionEvaluation\n");
            _outputWindowPane.OutputString("\tProcess: " + pProcess.Name + "\n");
            _outputWindowPane.OutputString("\tThread: " + thread.Name + "\n");
            _outputWindowPane.OutputString("\tExpression Evaluation State: " + processState.ToString() + "\n");
        }

        //PublishEvents
        public void OnPublishBegin(ref bool Continue) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("PublishEvents, OnPublishBegin\n");
            Continue = true;
        }

        public void OnPublishDone(bool Success) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString("PublishEvents, OnPublishDone\n");
            _outputWindowPane.OutputString("\tSuccess: " + Success.ToString() + "\n");
        }

        #endregion

        //public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        //{
        //    #region ObjectExplorer event - Sub

        //    provider.SelectionChanged -= new NodesChangedEventHandler(provider_SelectionChanged);
        //    commandEventsQueryExecute.AfterExecute -= new _dispCommandEvents_AfterExecuteEventHandler(commandEventsQueryExecute_AfterExecute);

        //    #endregion

        //    #region EventWatcher - Sub

        //    if (testEvents)
        //    {
        //        //If the delegate handlers have been connected, then disconnect them here. 
        //        //	This needs to be done, otherwise the handler may still fire since they
        //        //	have not been garbage collected.
        //        if (_windowsEvents != null)
        //        {
        //            _windowsEvents.WindowActivated -= new _dispWindowEvents_WindowActivatedEventHandler(this.WindowActivated);
        //            _windowsEvents.WindowClosing -= new _dispWindowEvents_WindowClosingEventHandler(this.WindowClosing);
        //            _windowsEvents.WindowCreated -= new _dispWindowEvents_WindowCreatedEventHandler(this.WindowCreated);
        //            _windowsEvents.WindowMoved -= new _dispWindowEvents_WindowMovedEventHandler(this.WindowMoved);
        //        }

        //        if (_textEditorEvents != null)
        //            _textEditorEvents.LineChanged -= new _dispTextEditorEvents_LineChangedEventHandler(this.LineChanged);

        //        if (_taskListEvents != null)
        //        {
        //            _taskListEvents.TaskAdded -= new _dispTaskListEvents_TaskAddedEventHandler(this.TaskAdded);
        //            _taskListEvents.TaskModified -= new _dispTaskListEvents_TaskModifiedEventHandler(this.TaskModified);
        //            _taskListEvents.TaskNavigated -= new _dispTaskListEvents_TaskNavigatedEventHandler(this.TaskNavigated);
        //            _taskListEvents.TaskRemoved -= new _dispTaskListEvents_TaskRemovedEventHandler(this.TaskRemoved);
        //        }

        //        if (_solutionEvents != null)
        //        {
        //            _solutionEvents.AfterClosing -= new _dispSolutionEvents_AfterClosingEventHandler(this.AfterClosing);
        //            _solutionEvents.BeforeClosing -= new _dispSolutionEvents_BeforeClosingEventHandler(this.BeforeClosing);
        //            _solutionEvents.Opened -= new _dispSolutionEvents_OpenedEventHandler(this.Opened);
        //            _solutionEvents.ProjectAdded -= new _dispSolutionEvents_ProjectAddedEventHandler(this.ProjectAdded);
        //            _solutionEvents.ProjectRemoved -= new _dispSolutionEvents_ProjectRemovedEventHandler(this.ProjectRemoved);
        //            _solutionEvents.ProjectRenamed -= new _dispSolutionEvents_ProjectRenamedEventHandler(this.ProjectRenamed);
        //            _solutionEvents.QueryCloseSolution -= new _dispSolutionEvents_QueryCloseSolutionEventHandler(this.QueryCloseSolution);
        //            _solutionEvents.Renamed -= new _dispSolutionEvents_RenamedEventHandler(this.Renamed);
        //        }

        //        if (_selectionEvents != null)
        //            _selectionEvents.OnChange -= new _dispSelectionEvents_OnChangeEventHandler(this.OnChange);

        //        if (_outputWindowEvents != null)
        //        {
        //            _outputWindowEvents.PaneAdded -= new _dispOutputWindowEvents_PaneAddedEventHandler(this.PaneAdded);
        //            _outputWindowEvents.PaneClearing -= new _dispOutputWindowEvents_PaneClearingEventHandler(this.PaneClearing);
        //            _outputWindowEvents.PaneUpdated -= new _dispOutputWindowEvents_PaneUpdatedEventHandler(this.PaneUpdated);
        //        }

        //        if (_findEvents != null)
        //            _findEvents.FindDone -= new _dispFindEvents_FindDoneEventHandler(this.FindDone);

        //        if (_dteEvents != null)
        //        {
        //            _dteEvents.ModeChanged -= new _dispDTEEvents_ModeChangedEventHandler(this.ModeChanged);
        //            _dteEvents.OnBeginShutdown -= new _dispDTEEvents_OnBeginShutdownEventHandler(this.OnBeginShutdown);
        //            _dteEvents.OnMacrosRuntimeReset -= new _dispDTEEvents_OnMacrosRuntimeResetEventHandler(this.OnMacrosRuntimeReset);
        //            _dteEvents.OnStartupComplete -= new _dispDTEEvents_OnStartupCompleteEventHandler(this.OnStartupComplete);
        //        }

        //        if (_documentEvents != null)
        //        {
        //            _documentEvents.DocumentClosing -= new _dispDocumentEvents_DocumentClosingEventHandler(this.DocumentClosing);
        //            _documentEvents.DocumentOpened -= new _dispDocumentEvents_DocumentOpenedEventHandler(this.DocumentOpened);
        //            _documentEvents.DocumentOpening -= new _dispDocumentEvents_DocumentOpeningEventHandler(this.DocumentOpening);
        //            _documentEvents.DocumentSaved -= new _dispDocumentEvents_DocumentSavedEventHandler(this.DocumentSaved);
        //        }

        //        if (_debuggerEvents != null)
        //        {
        //            _debuggerEvents.OnContextChanged -= new _dispDebuggerEvents_OnContextChangedEventHandler(this.OnContextChanged);
        //            _debuggerEvents.OnEnterBreakMode -= new _dispDebuggerEvents_OnEnterBreakModeEventHandler(this.OnEnterBreakMode);
        //            _debuggerEvents.OnEnterDesignMode -= new _dispDebuggerEvents_OnEnterDesignModeEventHandler(this.OnEnterDesignMode);
        //            _debuggerEvents.OnEnterRunMode -= new _dispDebuggerEvents_OnEnterRunModeEventHandler(this.OnEnterRunMode);
        //            _debuggerEvents.OnExceptionNotHandled -= new _dispDebuggerEvents_OnExceptionNotHandledEventHandler(this.OnExceptionNotHandled);
        //            _debuggerEvents.OnExceptionThrown -= new _dispDebuggerEvents_OnExceptionThrownEventHandler(this.OnExceptionThrown);
        //        }

        //        if (_commandEvents != null)
        //        {
        //            _commandEvents.AfterExecute -= new _dispCommandEvents_AfterExecuteEventHandler(this.AfterExecute);
        //            _commandEvents.BeforeExecute -= new _dispCommandEvents_BeforeExecuteEventHandler(this.BeforeExecute);
        //        }

        //        if (_buildEvents != null)
        //        {
        //            _buildEvents.OnBuildBegin -= new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
        //            _buildEvents.OnBuildDone -= new _dispBuildEvents_OnBuildDoneEventHandler(this.OnBuildDone);
        //            _buildEvents.OnBuildProjConfigBegin -= new _dispBuildEvents_OnBuildProjConfigBeginEventHandler(this.OnBuildProjConfigBegin);
        //            _buildEvents.OnBuildProjConfigDone -= new _dispBuildEvents_OnBuildProjConfigDoneEventHandler(this.OnBuildProjConfigDone);
        //        }

        //        if (_miscFilesEvents != null)
        //        {
        //            _miscFilesEvents.ItemAdded -= new _dispProjectItemsEvents_ItemAddedEventHandler(this.MiscFilesEvents_ItemAdded);
        //            _miscFilesEvents.ItemRemoved -= new _dispProjectItemsEvents_ItemRemovedEventHandler(this.MiscFilesEvents_ItemRemoved);
        //            _miscFilesEvents.ItemRenamed -= new _dispProjectItemsEvents_ItemRenamedEventHandler(this.MiscFilesEvents_ItemRenamed);
        //        }

        //        if (_solutionItemsEvents != null)
        //        {
        //            _solutionItemsEvents.ItemAdded -= new _dispProjectItemsEvents_ItemAddedEventHandler(this.SolutionItemsEvents_ItemAdded);
        //            _solutionItemsEvents.ItemRemoved -= new _dispProjectItemsEvents_ItemRemovedEventHandler(this.SolutionItemsEvents_ItemRemoved);
        //            _solutionItemsEvents.ItemRenamed -= new _dispProjectItemsEvents_ItemRenamedEventHandler(this.SolutionItemsEvents_ItemRenamed);
        //        }

        //        if (_globalProjectItemsEvents != null)
        //        {
        //            _globalProjectItemsEvents.ItemAdded -= new _dispProjectItemsEvents_ItemAddedEventHandler(GlobalProjectItemsEvents_ItemAdded);
        //            _globalProjectItemsEvents.ItemRemoved -= new _dispProjectItemsEvents_ItemRemovedEventHandler(GlobalProjectItemsEvents_ItemRemoved);
        //            _globalProjectItemsEvents.ItemRenamed -= new _dispProjectItemsEvents_ItemRenamedEventHandler(GlobalProjectItemsEvents_ItemRenamed);
        //        }

        //        if (_globalProjectsEvents != null)
        //        {
        //            _globalProjectsEvents.ItemAdded -= new _dispProjectsEvents_ItemAddedEventHandler(GlobalProjectsEvents_ItemAdded);
        //            _globalProjectsEvents.ItemRemoved -= new _dispProjectsEvents_ItemRemovedEventHandler(GlobalProjectsEvents_ItemRemoved);
        //            _globalProjectsEvents.ItemRenamed -= new _dispProjectsEvents_ItemRenamedEventHandler(GlobalProjectsEvents_ItemRenamed);
        //        }

        //        if (_textDocumentKeyPressEvents != null)
        //        {
        //            _textDocumentKeyPressEvents.AfterKeyPress -= new _dispTextDocumentKeyPressEvents_AfterKeyPressEventHandler(AfterKeyPress);
        //            _textDocumentKeyPressEvents.BeforeKeyPress -= new _dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler(BeforeKeyPress);
        //        }

        //        if (_codeModelEvents != null)
        //        {
        //            _codeModelEvents.ElementAdded -= new _dispCodeModelEvents_ElementAddedEventHandler(ElementAdded);
        //            _codeModelEvents.ElementChanged -= new _dispCodeModelEvents_ElementChangedEventHandler(ElementChanged);
        //            _codeModelEvents.ElementDeleted -= new _dispCodeModelEvents_ElementDeletedEventHandler(ElementDeleted);
        //        }

        //        if (_windowVisibilityEvents != null)
        //        {
        //            _windowVisibilityEvents.WindowHiding -= new _dispWindowVisibilityEvents_WindowHidingEventHandler(WindowHiding);
        //            _windowVisibilityEvents.WindowShowing -= new _dispWindowVisibilityEvents_WindowShowingEventHandler(WindowShowing);
        //        }

        //        if (_debuggerExpressionEvaluationEvents != null)
        //        {
        //            _debuggerExpressionEvaluationEvents.OnExpressionEvaluation -= new _dispDebuggerExpressionEvaluationEvents_OnExpressionEvaluationEventHandler(OnExpressionEvaluation);
        //        }

        //        if (_debuggerProcessEvents != null)
        //        {
        //            _debuggerProcessEvents.OnProcessStateChanged -= new _dispDebuggerProcessEvents_OnProcessStateChangedEventHandler(OnProcessStateChanged);
        //        }

        //        if (_publishEvents != null)
        //        {
        //            _publishEvents.OnPublishBegin -= new _dispPublishEvents_OnPublishBeginEventHandler(OnPublishBegin);
        //            _publishEvents.OnPublishDone -= new _dispPublishEvents_OnPublishDoneEventHandler(OnPublishDone);
        //        }

        //    }

        //    #endregion
        //}




        #endregion
    }
}
