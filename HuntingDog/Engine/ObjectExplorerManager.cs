using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabaseObjectSearcher {
    public class ObjectExplorerManager {
        protected readonly Log log = LogFactory.GetLog();
        private readonly IAsyncServiceProvider Package;
        private static IObjectExplorerService _objExplorer = null;

        public event Action<SqlConnectionInfo> OnNewServerConnected;

        public event Action<string, string> OnDatabaseChanged;

        public event System.Action OnServerDisconnected;

        //public  List<NavigatorServer> GetServers()
        //{
        //    var r = new List<NavigatorServer>();
        //    foreach (var srvConnectionInfo in GetAllServers())
        //    {
        //        var nvServer = new NavigatorServer(srvConnectionInfo, srvConnectionInfo.ServerName);
        //        r.Add(nvServer);
        //    }
        //    return r;
        //}

        private MethodInfo _editTableMethod = null;

        public ObjectExplorerManager(IAsyncServiceProvider package) {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
            Init();
        }

        public IObjectExplorerService GetObjectExplorer() {
            if (_objExplorer == null) {
                _objExplorer = (IObjectExplorerService)Package.GetServiceAsync(typeof(IObjectExplorerService)).GetAwaiter().GetResult();
                
                //_objExplorer = (IObjectExplorerService) ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
            }
            //Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService
            //Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer
            return _objExplorer;
        }


        private void SetObjectExplorerEventProvider() {
            // the old way of doing things
            //Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService objectExplorer = Common.ObjectExplorerService as Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService;
            //int nodeCount;
            //INodeInformation[] nodes;
            //objectExplorer.GetSelectedNodes(out nodeCount, out nodes);
            //Microsoft.SqlServer.Management.SqlStudio.Explorer.ContextService contextService = (Microsoft.SqlServer.Management.SqlStudio.Explorer.ContextService)objectExplorer.Container.Components[1];
            //// or ContextService contextService = (ContextService)objectExplorer.Site.Container.Components[1];
            //INavigationContextProvider provider = contextService.ObjectExplorerContext;
            //provider.CurrentContextChanged += new NodesChangedEventHandler(ObjectExplorer_SelectionChanged);

            Type t = Assembly.Load("Microsoft.SqlServer.Management.SqlStudio.Explorer").GetType("Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService");
            MethodInfo mi = this.GetType().GetMethod("Provider_SelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            Object objectExplorer = GetObjectExplorer();

            // hack to load the OE in R2
            (objectExplorer as IObjectExplorerService).GetSelectedNodes(out int nodeCount, out INodeInformation[] nodes);

            PropertyInfo piContainer = t.GetProperty("Container", BindingFlags.Public | BindingFlags.Instance);
            Object objectExplorerContainer = piContainer.GetValue(objectExplorer, null);
            PropertyInfo piContextService = objectExplorerContainer.GetType().GetProperty("Components", BindingFlags.Public | BindingFlags.Instance);
            //object[] indexArgs = { 1 };
            ComponentCollection objectExplorerComponents = piContextService.GetValue(objectExplorerContainer, null) as ComponentCollection;
            Object contextService = null;

            contextService = objectExplorerComponents.Cast<Component>()
                .FirstOrDefault(component => component.GetType().FullName.Contains("ContextService"));

            if (contextService == null) {
                throw new Exception("Can't find ObjectExplorer ContextService.");
            }

            PropertyInfo piObjectExplorerContext = contextService.GetType().GetProperty("ObjectExplorerContext", BindingFlags.Public | BindingFlags.Instance);
            Object objectExplorerContext = piObjectExplorerContext.GetValue(contextService, null);

            EventInfo ei = objectExplorerContext.GetType().GetEvent("CurrentContextChanged", BindingFlags.Public | BindingFlags.Instance);

            Delegate del = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
            ei.AddEventHandler(objectExplorerContext, del);
        }

        public void Init() {
            ThreadHelper.ThrowIfNotOnUIThread();
            try {
                SetObjectExplorerEventProvider();

                // old way
                //var provider = (IObjectExplorerEventProvider)objectExplorer.GetService(typeof(IObjectExplorerEventProvider));
                //provider.SelectionChanged += new NodesChangedEventHandler(provider_SelectionChanged);
                //ContextService cs = (ContextService)objExplorerService.Container.Components[0];
                //cs.ObjectExplorerContext.CurrentContextChanged += new NodesChangedEventHandler(Provider_SelectionChanged);
            }
            catch (Exception ex) {
                // NEED TO LOG
                log.Error("Error Initializing object explorer (subscribing selection changed event) " + ex.Message, ex);
            }

            try {
                //  System.Threading.Thread.Sleep(80 * 1000);
                var cmdEvents = ServiceCache.ExtensibilityModel.Events.get_CommandEvents("{00000000-0000-0000-0000-000000000000}", 0);
                cmdEvents.AfterExecute += this.AfterExecute;
            }
            catch (Exception ex) {
                log.Error("Error Initializing object explorer  (subscribing command event)" + ex.Message, ex);
            }
        }

        void Provider_SelectionChanged(Object sender, NodesChangedEventArgs args) {
            try {
                var t = args.GetType();
                var pi = t.GetProperty("ChangedNodes");
                var coll = (ICollection)pi.GetValue(args, null);

                foreach (INavigationContext n in coll) {
                    if (n != null) {
                        if (n.Parent == null) //server node
                        {
                            log.Info("New Server Connected " + n.Name + " -  " + n.Connection.ServerName);

                            if (OnNewServerConnected != null) {
                                OnNewServerConnected((SqlConnectionInfo)n.Connection);
                            }
                        }
                        else if (n.ViewIdentity == "Server/Database") {
                            OnDatabaseChanged?.Invoke(n.Connection.ServerName, n.Name);
                        }
                    }
                }
            }
            catch (Exception ex) {
                log.Error("Error processing OnSelectionChanged event: " + ex.Message, ex);
            }
        }

        public void AfterExecute(String Guid, Int32 ID, Object CustomIn, Object CustomOut) {
            log.Info("After execute command:" + ID + " guid:" + Guid);

            // this could mean that server was removed
            if (ID == 516) {
                log.Info("Server disconnected..!");

                if (OnServerDisconnected != null) {
                    OnServerDisconnected();
                }
            }
        }

        private Object GetTreeControl() {
            Type t = GetObjectExplorer().GetType();
            PropertyInfo treeProperty = t.GetProperty("Tree", BindingFlags.Instance | BindingFlags.NonPublic);
            var objectTreeControl = treeProperty.GetValue(GetObjectExplorer(), null);
            return objectTreeControl;
        }

        // ugly reflection hack
        private IExplorerHierarchy GetHierarchyForConnection(SqlConnectionInfo connection) {
            var objectTreeControl = GetTreeControl();
            var objTreeRype = objectTreeControl.GetType();
            var getHierarchyMethod = objTreeRype.GetMethod("GetHierarchy", BindingFlags.Instance | BindingFlags.Public);
            return getHierarchyMethod.Invoke(objectTreeControl, new Object[] { connection, String.Empty }) as IExplorerHierarchy;
        }

        public IEnumerable<IExplorerHierarchy> GetExplorerHierarchies() {
            var objectTreeControl = GetTreeControl();
            var objTreeRype = objectTreeControl.GetType();
            var hierFieldInfo = objTreeRype.GetField("hierarchies", BindingFlags.Instance | BindingFlags.NonPublic);
            var hierDictionary = (IEnumerable<KeyValuePair<string, IExplorerHierarchy>>)hierFieldInfo.GetValue(objectTreeControl);

            foreach (var keyVaklue in hierDictionary) {
                yield return keyVaklue.Value;
            }
        }

        public List<SqlConnectionInfo> GetAllServers() {
            try {
                return GetExplorerHierarchies()
                    .Where(srvHerarchy => srvHerarchy.Root is IServiceProvider)
                    .Select(srvHerarchy => srvHerarchy.Root as IServiceProvider)
                    .Select(provider => {
                        INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                        return containedItem.Connection as SqlConnectionInfo;
                    })
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("ObjectExplorer manager failed:" + ex.Message, ex);
                throw;
            }
        }

        // select server on object window
        public void SelectServer(SqlConnectionInfo connection) {
            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);
            SelectNode(hierarchy.Root);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        public void OpenTable2(NamedSmoObject tbl, SqlConnectionInfo connection, Server server) {
            String fileName = null;

            // step1 - get script to edit table - SelectFromTableOrView(Server server, Urn urn, int topNValue)
            // step2 - create a file

            try {
                var t = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.OpenTableHelperClass,ObjectExplorer", true, true);
                var miSelectFromTable = t.GetMethod("SelectFromTableOrView", BindingFlags.Static | BindingFlags.Public);

                //SelectFromTableOrView(Microsoft.SqlServer.Management.Smo.Server server, Microsoft.SqlServer.Management.Sdk.Sfc.Urn urn, Int32 topNValue, Boolean scriptForSelectingRows, Boolean isDw, Boolean isMemoryOptimized)
                //scriptForSelectingRows ??
                //isDw ??
                //isMemoryOptimized add WITH (SNAPSHOT)
                String script = (String)miSelectFromTable.Invoke(null, new Object[] { server, tbl.Urn, 200, false, false, false });
                fileName = CreateFile(script);

                // invoke designer
                var mc = new ManagedConnection();
                mc.Connection = connection;

                if (_editTableMethod == null) {
                    _editTableMethod = ServiceCache.ScriptFactory
                        .GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                        .FirstOrDefault(mi => (mi.Name == "CreateDesigner") && (mi.GetParameters().Length == 5));
                }

                if (_editTableMethod != null) {
                    _editTableMethod.Invoke(ServiceCache.ScriptFactory, new Object[] { DocumentType.OpenTable, DocumentOptions.ManageConnection, new Urn(tbl.Urn.ToString() + "/Data"), mc, fileName });
                }
                else {
                    log.Error("Could not find CreateDesigner method");
                }
            }
            catch (Exception ex) {
                log.Error("Failed OpenTable2", ex);
            }
            finally {
                if (!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                    File.Delete(fileName);
                }
            }
        }

        public static String CreateFile(String script) {
            var path = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", new Object[] { Path.GetTempFileName(), "dtq" });
            var builder = new StringBuilder();
            builder.Append("[D035CF15-9EDB-4855-AF42-88E6F6E66540, 2.00]\r\n");
            builder.Append("Begin Query = \"Query1.dtq\"\r\n");
            builder.Append("Begin RunProperties =\r\n");
            builder.AppendFormat("{0}{1}{2}", "SQL = \"", script, "\"\r\n");
            builder.Append("ParamPrefix = \"@\"\r\n");
            builder.Append("ParamSuffix = \"\"\r\n");
            builder.Append("ParamSuffix = \"\\\"\r\n");
            builder.Append("End\r\n");
            builder.Append("End\r\n");

            using (var writer = new StreamWriter(path, false, Encoding.Unicode)) {
                writer.Write(builder.ToString());
            }

            return path;
        }

        internal void OpenTable(NamedSmoObject objectToSelect, SqlConnectionInfo connection) {
            try {
                IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);

                if (hierarchy == null) {
                    return; // there is nothing we can really do if we don't have one of these
                }

                HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);
                var resultNode = FindNodeForSmoObject(databasesNode, objectToSelect);

                //MSSQLController.Current.SearchWindow.Activate();

                if (resultNode != null) {
                    OpenTable(resultNode);
                }
            }
            catch (Exception ex) {
                log.Error("Error opening table: " + objectToSelect.Name, ex);
            }
        }

        internal void SelectSMOObjectInObjectExplorer(NamedSmoObject objectToSelect, SqlConnectionInfo connection) {
            if (objectToSelect.State == SqlSmoState.Dropped) {
                log.Info("Trying to locate dropped object:" + objectToSelect.Name);
                return;
            }

            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);

            if (hierarchy == null) {
                return; // there is nothing we can really do if we don't have one of these
            }

            HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);
            var resultNode = FindNodeForSmoObject(databasesNode, objectToSelect);

            if (resultNode != null) {
                SelectNode(resultNode);
            }
        }

        private HierarchyTreeNode GetUserDatabasesNode(HierarchyTreeNode rootNode) {
            if (rootNode == null || !rootNode.Expandable) {
                return null;
            }

            EnumerateChildrenSynchronously(rootNode);
            rootNode.Expand();

            // TODO this is horrible code - it assumes the first node will ALWAYS be the "Databases" node in the object explorer, which may not always be the case
            // however I couldn't think of a clean way to always find the right node
            return (HierarchyTreeNode)rootNode.Nodes[0];
        }

        private string GetNodeNameFor(NamedSmoObject smoObject) {
            return smoObject.ToString().Replace("[", "").Replace("]", "");
        }

        private HierarchyTreeNode FindTableNode(HierarchyTreeNode nodeDatabases, NamedSmoObject tableSmoObject) {
            var tableToSelect = (Table)tableSmoObject;
            return FindRecursively(nodeDatabases, tableToSelect.Parent, "Tables", GetNodeNameFor(tableSmoObject));
        }

        private HierarchyTreeNode FindNodeForSmoObject(HierarchyTreeNode nodeDatabases, NamedSmoObject objectToSelect) {
            if (objectToSelect is Table) {
                return FindTableNode(nodeDatabases, objectToSelect);
            }
            else if (objectToSelect is View viewToSelect) {
                return FindRecursively(nodeDatabases, viewToSelect.Parent, "Views", GetNodeNameFor(objectToSelect));
            }
            else if (objectToSelect is StoredProcedure procedure) {
                return FindRecursively(nodeDatabases, procedure.Parent, "Programmability", "Stored Procedures", GetNodeNameFor(objectToSelect));
            }
            else if (objectToSelect is UserDefinedFunction func) {
                string functionNodeName = func.FunctionType == UserDefinedFunctionType.Scalar ? "Scalar-valued Functions" : "Table-valued Functions";
                return FindRecursively(nodeDatabases, func.Parent, "Programmability", "Functions", functionNodeName, GetNodeNameFor(objectToSelect));
            }

            return null;

        }

        HierarchyTreeNode FindRecursively(HierarchyTreeNode parent, Database database, params string[] nodes) {
            var databaseNode = FindDatabaseNodeByName(parent, database);
            if (databaseNode == null)
                return null;

            HierarchyTreeNode currentLevel = databaseNode;
            foreach (var nodeName in nodes) {
                currentLevel = FindChildNodeByName(currentLevel, nodeName);

                if (currentLevel == null) {
                    return null;
                }

            }

            return currentLevel;
        }

        HierarchyTreeNode FindDatabaseNodeByName(HierarchyTreeNode parentNode, Database database) {
            var databaseNode = FindChildNodeByName(parentNode, database.Name);
            if (databaseNode != null)
                return databaseNode;

            // trying to find node with (Read-Only) text at the end as read only database node will change its text
            var readonlyDatabaseName = FindChildNodeByName(parentNode, database.Name + " (Read-Only)");
            if (readonlyDatabaseName != null)
                return readonlyDatabaseName;

            var standbyDatabaseName = FindChildNodeByName(parentNode, database.Name + " (Standby / Read-Only)");
            return standbyDatabaseName;
        }

        HierarchyTreeNode FindChildNodeByName(HierarchyTreeNode parentNode, string name) {
            if (!parentNode.Expandable)
                return null;

            EnumerateChildrenSynchronously(parentNode);
            parentNode.Expand();

            foreach (HierarchyTreeNode child in parentNode.Nodes) {
                if (child.Text.ToLower() == name.ToLower())
                    return child;
            }

            return null;
        }


        private void OpenTable(HierarchyTreeNode node) {
            var t = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.OpenTableHelperClass,ObjectExplorer", true, true);
            var mi = t.GetMethod("EditTopNRows", BindingFlags.Static | BindingFlags.Public);
            var ncT = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.NodeContext,ObjectExplorer", true, true);

            IServiceProvider provider = node as IServiceProvider;
            INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;

            var inst = Activator.CreateInstance(ncT, containedItem);

            if (inst == null) {
                throw new Exception("Cannot create type" + ncT.ToString());
            }

            mi.Invoke(null, new Object[] { containedItem, 200 });
        }

        private void SelectNode(HierarchyTreeNode node) {
            if (node is IServiceProvider provider && provider.GetService(typeof(INodeInformation)) is INodeInformation containedItem) {
                IObjectExplorerService objExplorer = GetObjectExplorer();
                objExplorer.SynchronizeTree(containedItem);
            }
        }

        // another exciting opportunity to use reflection
        private void EnumerateChildrenSynchronously(HierarchyTreeNode node) {
            Type t = node.GetType();
            MethodInfo method = t.GetMethod("EnumerateChildren", new Type[] { typeof(Boolean) });

            if (method != null) {
                method.Invoke(node, new Object[] { false });
            }
            else {
                // fail
                node.EnumerateChildren();
            }
        }
    }
}
