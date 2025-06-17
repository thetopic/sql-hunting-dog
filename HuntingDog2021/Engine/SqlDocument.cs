using EnvDTE;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace HuntingDog.Engine {
    internal class SqlDocument {

        private readonly TextDocument _doc;

        public SqlDocument(TextDocument doc) {
            _doc = doc;
        }

        public static SqlDocument CreateBlankScriptDocument(UIConnectionInfo uIConnectionInfo) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            ServiceCache.ScriptFactory.CreateNewBlankScript(ScriptType.Sql, uIConnectionInfo, null);
            return new SqlDocument((TextDocument)ServiceCache.ExtensibilityModel.Application.ActiveDocument.Object(null));
        }

        public void InsertSql(string sql) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _doc.EndPoint.CreateEditPoint().Insert(sql);
        }

        public void Execute() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _doc.DTE.ExecuteCommand("Query.Execute");
        }
    }
}
