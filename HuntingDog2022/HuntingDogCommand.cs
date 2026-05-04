using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace HuntingDog
{
    internal sealed class HuntingDogCommand
    {
        private readonly AsyncPackage _package;
        public IVsWindowFrame _windowFrame = null;
        private HuntingDog.ucHost _uglyUsefuleDogFace;
        private bool _engineInitialized = false;

        private HuntingDogCommand(AsyncPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("HuntingDogCommand is null from an unknown reason.");

            this._package = package;

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(PackageGuids.HuntingDogCommandSetID, PackageIds.HuntingDogCommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        public static HuntingDogCommand Instance { get; private set; }
        private IServiceProvider ServiceProvider { get { return this._package; } }
        public string Caption { get { return "Hunting Dog"; } }

        public static void Initialize(AsyncPackage package)
        {
            Instance = new HuntingDogCommand(package);
            // Defer ShowToolWindow to let SSMS fully initialize its COM services before we access them.
            // Accessing IObjectExplorerService or DTE too early causes InvalidComObjectException.
            var delay = new System.Windows.Forms.Timer { Interval = 3000 };
            delay.Tick += (s, e) => { delay.Stop(); Instance.ShowToolWindow(); };
            delay.Start();
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            ShowToolWindow();
        }

        private void ShowToolWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            const string TOOLWINDOW_GUID = "{7C23E551-2E95-40A8-B783-3753D4E3DEAB}";

            if (_windowFrame == null)
            {
                _uglyUsefuleDogFace = new ucHost();
                _windowFrame = CreateToolWindow(Caption, TOOLWINDOW_GUID, _uglyUsefuleDogFace);
                ReadConfiguration();
            }

            EnsureEngineInitialized();

            if (_windowFrame != null && _cfg.ShowAfterOpen)
            {
                _windowFrame.Show();
                HuntingDog.DogEngine.Impl.DiConstruct.Instance.ForceShowYourself();
            }
        }

        // Called on menu click — always show the window.
        private void ShowWindowOnDemand()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnsureEngineInitialized();
            _windowFrame?.Show();
            HuntingDog.DogEngine.Impl.DiConstruct.Instance.ForceShowYourself();
        }

        private void EnsureEngineInitialized()
        {
            if (_engineInitialized) return;
            try
            {
                HuntingDog.DogEngine.Impl.DiConstruct.Instance.HideYourself += Instance_HideYourself;
                _engineInitialized = true;
            }
            catch (Exception)
            {
                // Services not ready yet — will retry on next call.
            }
        }

        private void Instance_HideYourself()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _windowFrame?.Hide();
        }

        private IVsWindowFrame CreateToolWindow(string caption, string guid, System.Windows.Forms.UserControl userControl)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            const int TOOL_WINDOW_INSTANCE_ID = 0;

            IVsUIShell uiShell = (IVsUIShell)ServiceProvider.GetService(typeof(SVsUIShell));
            Guid toolWindowPersistenceGuid = new Guid(guid);
            Guid guidNull = Guid.Empty;
            int[] position = new int[1];

            int result = uiShell.CreateToolWindow((uint)__VSCREATETOOLWIN.CTW_fInitNew, TOOL_WINDOW_INSTANCE_ID, userControl, ref guidNull, ref toolWindowPersistenceGuid, ref guidNull, null, caption, position, out IVsWindowFrame windowFrame);

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(result);
            return windowFrame;
        }

        Config.DogConfig _cfg = new Config.DogConfig();
        private void ReadConfiguration()
        {
            try
            {
                var userPreference = HuntingDog.DogFace.UserPreferencesStorage.Load();
                Config.ConfigPersistor pers = new Config.ConfigPersistor();
                _cfg = pers.Restore<Config.DogConfig>(userPreference);
            }
            catch (Exception)
            {
            }
        }
    }
}
