
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System;
using System.Reflection;
using HuntingDog.Core;

namespace HuntingDog.DogEngine {
    public class AddinCreater {
        private AddIn addIn;

        private static readonly String windowId = "{7146B360-D37D-44A1-8D4C-5E7E36EA81D4}";

        private readonly Log log = LogFactory.GetLog();

        private EnvDTE.Window SearchWindow {
            get;
            set;
        }

        public EnvDTE.Window CreateAddinWindow(AddIn addIn, string caption) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try {
                this.addIn = addIn;

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var className = typeof(HuntingDog.ucHost).FullName;
                Object userControl = null;


                if (ServiceCache.ExtensibilityModel.Windows is Windows2 windows) {
                    if ((SearchWindow == null) || (windows.Item(windowId) == null)) {
                        SearchWindow = windows.CreateToolWindow2(addIn, assemblyLocation, className, caption, windowId, ref userControl);
                        SearchWindow.SetTabPicture(Properties.Resources.footprint.GetHbitmap());
                        Impl.DiConstruct.Instance.HideYourself += Instance_HideYourself; ;
                    }
                    ReadConfiguration();
                    SearchWindow.Visible = _cfg.ShowAfterOpen;
                }

                return SearchWindow;
            }
            catch (Exception ex) {
                log.Error("AddIn window could not be created", ex);
                throw;
            }
        }

        private void Instance_HideYourself() {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (SearchWindow != null)
                SearchWindow.Visible = false;
        }


        // default configuration
        Config.DogConfig _cfg = new Config.DogConfig();
        // To refactor - use already read config from Connect.cs
        private void ReadConfiguration() {
            try {
                var userPreference = DogFace.UserPreferencesStorage.Load();
                Config.ConfigPersistor pers = new Config.ConfigPersistor();
                _cfg = pers.Restore<Config.DogConfig>(userPreference);

            }
            catch (Exception ex) {
                log.Error("ReadConfiguration: failed", ex);
            }
        }
    }
}