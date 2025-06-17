//------------------------------------------------------------------------------
// <copyright file="HuntingDog2019.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HuntingDog {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    ///
    //[ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    //[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    //[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    //[Guid(PackageGuids.HuntingDogPackageIDString)]
    //[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    //[ProvideMenuResource("Menus.ctmenu", 1)]

    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.HuntingDogPackageIDString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class HuntingDogPackage : AsyncPackage {
        /// <summary>
        /// Initializes a new instance of the <see cref="HuntingDogPackage"/> class.
        /// </summary>
        public HuntingDogPackage() {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            Instance = this;
        }

        #region Package Members

        public static HuntingDogPackage Instance { get; set; }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Initialize();

            HuntingDogCommand.Initialize(this);

            // total hack https://ssmsschemafolders.codeplex.com/SourceControl/latest#README.md
            DelayAddSkipLoadingReg();
        }


        public object GetServiceHelper(Type type) {
            return GetService(type);
        }

        #endregion

        private void AddSkipLoadingReg() {
            var myPackage = UserRegistryRoot.CreateSubKey($@"Packages\{{{PackageGuids.HuntingDogPackageIDString}}}");
            if (myPackage != null) {
                myPackage.SetValue("SkipLoading", 1);
            }
        }

        private void DelayAddSkipLoadingReg() {
            var delay = new System.Windows.Forms.Timer();
            delay.Tick += delegate (object o, EventArgs e) {
                delay.Stop();
                AddSkipLoadingReg();
            };
            delay.Interval = 1000;
            delay.Start();
        }
    }
}
