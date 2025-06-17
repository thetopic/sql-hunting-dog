
using DatabaseObjectSearcher;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;



namespace HuntingDog.DogEngine.Impl {
    public sealed class StudioController : IStudioController {
        public event Action ShowYourself;

        public event Action HideYourself;

        public event Action<List<IServer>> OnServersAdded;

        public event Action<List<IServer>> OnServersRemoved;

        public event Action<string, string> OnDatabaseChanged;

        private readonly Log log = LogFactory.GetLog();

        private readonly ObjectExplorerManager manager;

        private readonly IServerWatcher _srvWatcher;

        public StudioController(ObjectExplorerManager mgr, IServerWatcher watcher) {
            manager = mgr;
            manager.OnDatabaseChanged += Manager_OnDatabaseChanged;

            _srvWatcher = watcher;
            _srvWatcher.OnServersAdded += _srvWatcher_OnServersAdded;
            _srvWatcher.OnServersRemoved += _srvWatcher_OnServersRemoved;

            Servers = new Dictionary<IServer, DatabaseLoader>();
        }

        private void Manager_OnDatabaseChanged(string serverName, string databaseName) {
            if (OnDatabaseChanged != null)
                OnDatabaseChanged(serverName, databaseName);
        }

        void IStudioController.Initialise() {

        }


        void _srvWatcher_OnServersRemoved(List<IServerWithConnection> removedServers) {

            removedServers
                .Where(removedServer => Servers.ContainsKey(removedServer))
                .Select(removedServer => Servers.Remove(removedServer))
                .ToList();

            if (Servers.Count == 0) {
                GC.Collect();
            }

            OnServersRemoved(removedServers.Cast<IServer>().ToList());
        }

        void _srvWatcher_OnServersAdded(List<IServerWithConnection> addedServers) {

            foreach (var addedServer in addedServers) {
                var nvServer = new DatabaseLoader();
                nvServer.Initialise(addedServer);
                Servers.Add(addedServer, nvServer);
            }

            OnServersAdded(addedServers.Cast<IServer>().ToList());
        }


        Dictionary<IServer, DatabaseLoader> Servers {
            get;
            set;
        }


        List<Entity> IStudioController.Find(IServer serverName, String databaseName, String searchText, int searchLimit) {
            var server = Servers[serverName];
            var keywords = new List<String>();
            var listFound = server.Find(searchText, databaseName, searchLimit, keywords);

            return listFound.Select(found => new Entity {
                Name = found.Name,
                IsFunction = found.IsFunction,
                IsProcedure = found.IsStoredProc,
                IsTable = found.IsTable,
                IsView = found.IsView,
                FullName = found.SchemaAndName,
                InternalObject = found.Result,
                Keywords = keywords,
                DatabaseName = databaseName
            })
                .ToList();
        }

        void IStudioController.NavigateObject(IServer server, Entity entityObject) {
            try {
                var srv = this.Servers[server];
                manager.SelectSMOObjectInObjectExplorer(entityObject.InternalObject as ScriptSchemaObjectBase, srv.Connection);
                ForceHideYourselfIfNeeded();
            }
            catch (Exception ex) {
                log.Error("Error locating object", ex);
            }
        }



        List<IServer> IStudioController.ListServers() {
            return Servers.Keys.Select(srvKey => srvKey).ToList();
        }

        public List<String> ListDatabase(IServer serverName) {
            if (! Servers.ContainsKey(serverName)) {
                log.Error("Requested unknown server " + serverName.ID);

                foreach (var srv in Servers) {
                    log.Error("Available server: " + srv.Key);
                }

                return new List<String>();
            }

            return Servers[serverName].DatabaseList;
        }

        void IStudioController.RefreshServer(IServer serverName) {
            this.SafeRun(() => {
                var serverInfo = GetServer(serverName);
                serverInfo.RefreshDatabaseList();

            }, "Refreshing database list failed - " + serverName.ServerName);

        }

        void IStudioController.RefreshDatabase(IServer serverName, String dbName) {

            this.SafeRun(() => {
                var serverInfo = GetServer(serverName);
                serverInfo.RefreshDatabase(dbName);

            }, "Refreshing database failed - " + serverName.ServerName + " " + dbName);

        }

        private String GetSafeEntityObject(Entity entityObject) {
            return (entityObject != null)
                ? entityObject.ToSafeString()
                : "NULL entityObject";
        }

        List<TableColumn> IStudioController.ListViewColumns(Entity entityObject) {
            var result = new List<TableColumn>();

            try {
                var view = entityObject.InternalObject as View;
                view.Columns.Refresh();

                foreach (Column tc in view.Columns) {
                    result.Add(new TableColumn() {
                        Name = tc.Name,
                        IsPrimaryKey = tc.InPrimaryKey,
                        IsForeignKey = tc.IsForeignKey,
                        Nullable = tc.Nullable,
                        Type = tc.DataType.Name
                    });
                }
            }
            catch (Exception ex) {
                log.Error("ListViewColumns failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<TableColumn> IStudioController.ListColumns(Entity entityObject) {
            var result = new List<TableColumn>();

            try {
                var table = entityObject.InternalObject as Table;
                table.Columns.Refresh();

                result = table.Columns.Cast<Column>().Select(tc =>
                new TableColumn() {
                    Name = tc.Name,
                    IsPrimaryKey = tc.InPrimaryKey,
                    IsForeignKey = tc.IsForeignKey,
                    Nullable = tc.Nullable,
                    Type = tc.DataType.Name
                })
                    .ToList();

            }
            catch (Exception ex) {
                log.Error("ListColumns failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<FunctionParameter> IStudioController.ListFuncParameters(Entity entityObject) {
            var result = new List<FunctionParameter>();

            try {
                var func = entityObject.InternalObject as UserDefinedFunction;
                func.Parameters.Refresh();

                foreach (UserDefinedFunctionParameter tc in func.Parameters) {
                    result.Add(new FunctionParameter() {
                        Name = tc.Name,
                        Type = tc.DataType.Name
                    });
                }

            }
            catch (Exception ex) {
                log.Error("ListFuncParameters failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<ProcedureParameter> IStudioController.ListProcParameters(Entity entityObject) {
            var result = new List<ProcedureParameter>();

            try {
                var procedure = entityObject.InternalObject as StoredProcedure;
                procedure.Parameters.Refresh();

                foreach (StoredProcedureParameter tc in procedure.Parameters) {
                    result.Add(new ProcedureParameter() {
                        Name = tc.Name,
                        IsOut = tc.IsOutputParameter,
                        DefaultValue = tc.DefaultValue,
                        Type = tc.DataType.Name,
                    });
                }


            }
            catch (Exception ex) {
                log.Error("ListProcParameters failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }


        public void ForceShowYourself() {
            if (ShowYourself != null) {
                ShowYourself();
            }
        }
        public void ForceHideYourself() {
            if (HideYourself != null) {
                HideYourself();
            }
        }
        public void ForceHideYourselfIfNeeded() {
            if (_cfg.HideAfterAction)
                ForceHideYourself();
        }

        public void ModifyFunction(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection, _cfg.AlterOrCreate);
                ForceHideYourselfIfNeeded();
            }, "ModifyFunction failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyView(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.ModifyView(entityObject.InternalObject as View, serverInfo.Connection, _cfg.AlterOrCreate);
                ForceHideYourselfIfNeeded();
            }, "ModifyView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyProcedure(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                //System.Diagnostics.Debugger.Launch();
                //System.Diagnostics.Debugger.Break();
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.Connection, _cfg.AlterOrCreate);
                ForceHideYourselfIfNeeded();
            }, "ModifyProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromView(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.Connection, _cfg.SelectTopX, _cfg.IncludeAllColumns, _cfg.AddWhereClauseFor, _cfg.AddNoLock);
                ForceHideYourselfIfNeeded();
            }, "SelectFromView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteProcedure(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
                ForceHideYourselfIfNeeded();
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteFunction(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.ExecuteFunction(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
                ForceHideYourselfIfNeeded();
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        private DatabaseLoader GetServer(IServer server) {
            return Servers[server];
        }

        public void ScriptTable(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.ScriptTable(entityObject.InternalObject as Table, serverInfo.Connection, _cfg.ScriptIndexes, _cfg.ScriptForeignKeys, _cfg.ScriptTriggers);
                ForceHideYourselfIfNeeded();
            }, "ScriptTable - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromTable(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.Connection, _cfg.SelectTopX, _cfg.IncludeAllColumns, _cfg.AddWhereClauseFor, _cfg.AddNoLock, _cfg.OrderBy);
                ForceHideYourselfIfNeeded();
            }, "SelectFromTable - " + GetSafeEntityObject(entityObject));
        }

        public void EditTableData(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                manager.OpenTable2(entityObject.InternalObject as Table, serverInfo.Connection, serverInfo.Server);
                ForceHideYourselfIfNeeded();
            }, "EditTableData - " + GetSafeEntityObject(entityObject));
        }

        public void DesignTable(IServer server, Entity entityObject) {
            this.SafeRun(() => {
                var serverInfo = GetServer(server);
                serverInfo.Connection.DatabaseName = entityObject.DatabaseName;
                ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.Connection);
                ForceHideYourselfIfNeeded();
            }, "DesignTable - " + GetSafeEntityObject(entityObject));
        }


        // default configuration
        Config.DogConfig _cfg = new Config.DogConfig();

        public void SetConfiguration(Config.DogConfig cfg) {
            _cfg = cfg;
        }
    }
}
