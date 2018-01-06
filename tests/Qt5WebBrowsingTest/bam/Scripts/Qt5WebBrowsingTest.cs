#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core;
using QtCommon.MocExtension;
using System.Linq;
namespace Qt5WebBrowsingTest
{
    sealed class WebBrowser :
        C.Cxx.GUIApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/*.cpp");

            source.PrivatePatch(settings =>
                {
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        // because Qt5.6.0/5.6/gcc_64/include/QtCore/qglobal.h:1090:4: error: #error "You must build your code with position independent code if Qt was built with -reduce-relocations. " "Compile your code with -fPIC (-fPIE is not enough)."
                        gccCompiler.PositionIndependentCode = true;
                    }

                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    if (null != cxxCompiler)
                    {
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN/../lib");
                    }

                    var clangLinker = settings as ClangCommon.ICommonLinkerSettings;
                    if (null != clangLinker)
                    {
                        clangLinker.RPath.AddUnique("@executable_path/../Frameworks");
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompileAndLinkAgainst<Qt.CoreFramework>(source);
                this.CompileAndLinkAgainst<Qt.WidgetsFramework>(source);
                this.CompileAndLinkAgainst<Qt.WebEngineWidgetsFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.Core>(source);
                this.CompileAndLinkAgainst<Qt.Widgets>(source);
                this.CompileAndLinkAgainst<Qt.WebEngineWidgets>(source);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CreateWinResourceContainer("$(packagedir)/resources/*.rc");
                if (this.Linker is VisualCCommon.LinkerBase)
                {
                    this.CompileAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
                }
            }
        }
    }

    sealed class Qt5WebBrowsingTestRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacrosAndMappings(EPublishingType.WindowedApplication);
            var appAnchor = this.Include<WebBrowser>(C.Cxx.GUIApplication.Key);

            var qtPlatformPlugin = this.Find<QtCommon.PlatformPlugin>().First();
            (qtPlatformPlugin as Publisher.CollatedObject).SetPublishingDirectory("$(0)/platforms", this.PluginDir);

            var includeWebEngineResourceData = false;
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                var collatedQtFrameworks = this.Find<QtCommon.CommonFramework>();
                collatedQtFrameworks.ToList().ForEach(collatedFramework =>
                    // must be a public patch in order for the stripping mode to inherit the settings
                    (collatedFramework as Publisher.CollatedObject).PublicPatch((settings, appliedTo) =>
                        {
                            var rsyncSettings = settings as Publisher.IRsyncSettings;
                            rsyncSettings.Exclusions = (collatedFramework.SourceModule as QtCommon.CommonFramework).PublishingExclusions;
                        }));

                this.IncludeFiles(
                    this.CreateTokenizedString("$(packagedir)/resources/osx/qt.conf"),
                    this.Macros["macOSAppBundleResourcesDir"],
                    appAnchor);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                includeWebEngineResourceData = true;

                this.IncludeFiles(
                    this.CreateTokenizedString("$(packagedir)/resources/linux/qt.conf"),
                    this.ExecutableDir,
                    appAnchor);

                var XCBGLIntegrationPlugin = this.Find<QtCommon.XCBGLIntegrations>().First();
                (XCBGLIntegrationPlugin as Publisher.CollatedObject).SetPublishingDirectory("$(0)/xcbglintegrations", this.PluginDir);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                includeWebEngineResourceData = true;

                this.IncludeFiles(
                    this.CreateTokenizedString("$(packagedir)/resources/windows/qt.conf"),
                    this.ExecutableDir,
                    appAnchor);

                var app = appAnchor.SourceModule as WebBrowser;
                if (this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                    app.Linker is VisualCCommon.LinkerBase)
                {
                    var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                    this.IncludeFiles(runtimeLibrary.CRuntimePaths(app.BitDepth), this.ExecutableDir, appAnchor);
                    this.IncludeFiles(runtimeLibrary.CxxRuntimePaths(app.BitDepth), this.ExecutableDir, appAnchor);
                }
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported platform");
            }

            if (includeWebEngineResourceData)
            {
                var webEngine = this.Find<Qt.WebEngineCore>().First().SourceModule;
                this.IncludeFiles(webEngine.Macros["ICUDTL"], this.ResourceDir, appAnchor);
                this.IncludeFiles(webEngine.Macros["ResourcePak"], this.ResourceDir, appAnchor);
                this.IncludeFiles(webEngine.Macros["ResourcePak100p"], this.ResourceDir, appAnchor);
                this.IncludeFiles(webEngine.Macros["ResourcePak200p"], this.ResourceDir, appAnchor);
                this.IncludeDirectories(webEngine.Macros["Locales"], this.ResourceDir, appAnchor);
            }
#else
            var app = this.Include<WebBrowser>(C.ConsoleApplication.Key, EPublishingType.WindowedApplication);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                var qtPackage = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "Qt");
                var qtVersionSplit = qtPackage.Version.Split('.');
                var updateInstallName = (System.Convert.ToInt32(qtVersionSplit[1]) < 5); // < Qt5.5 requires install name updates

                this.IncludeFramework<Qt.CoreFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.WidgetsFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.GuiFramework>("../Frameworks", app, updateInstallName: updateInstallName);

                // required by the platform plugin
                this.IncludeFramework<Qt.PrintSupportFramework>("../Frameworks", app, updateInstallName: updateInstallName);
#if D_PACKAGE_QT_5_5_1 || D_PACKAGE_QT_5_6_0 || D_PACKAGE_QT_5_6_1 || D_PACKAGE_QT_5_6_2
                this.IncludeFramework<Qt.DBusFramework>("../Frameworks", app, updateInstallName: updateInstallName);
#endif

                this.Include<Qt.PlatformPlugin>(C.Plugin.Key, "../Plugins/qtplugins/platforms", app);
                this.IncludeFile(this.CreateTokenizedString("$(packagedir)/resources/osx/qt.conf"), "../Resources", app);

                this.IncludeFramework<Qt.NetworkFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.QmlFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.QuickFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.WebEngineCoreFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.WebChannelFramework>("../Frameworks", app, updateInstallName: updateInstallName);
                this.IncludeFramework<Qt.WebEngineWidgetsFramework>("../Frameworks", app, updateInstallName: updateInstallName);
            }
            else
            {
                this.Include<Qt.Core>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Widgets>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Gui>(C.DynamicLibrary.Key, ".", app);

                // Windows Qt builds no longer depend on ICU
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.Include<QtCommon.ICUIN>(C.DynamicLibrary.Key, ".", app);
                    this.Include<QtCommon.ICUUC>(C.DynamicLibrary.Key, ".", app);
                    this.Include<QtCommon.ICUDT>(C.DynamicLibrary.Key, ".", app);
                }

                this.IncludeFile(this.CreateTokenizedString("$(packagedir)/resources/qt.conf"), ".", app);
                var platformPlugin = this.Include<Qt.PlatformPlugin>(C.Plugin.Key, "qtplugins/platforms", app);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.ChangeRPath(platformPlugin, "$ORIGIN/../..");
                    this.Include<Qt.DBus>(C.DynamicLibrary.Key, ".", app); // for qxcb plugin

#if D_PACKAGE_QT_5_5_1 || D_PACKAGE_QT_5_6_0 || D_PACKAGE_QT_5_6_1 || D_PACKAGE_QT_5_6_2
                    this.Include<Qt.XcbQpa>(C.DynamicLibrary.Key, ".", app);
#endif

                    // required for OpenGL support
                    var xcbGLIntegrationPlugin = this.Include<Qt.XCBGLIntegrationsPlugin>(C.Plugin.Key, "qtplugins/xcbglintegrations", app);
                    this.ChangeRPath(xcbGLIntegrationPlugin, "$ORIGIN/../..");
                }

                this.Include<Qt.Network>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Qml>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Quick>(C.DynamicLibrary.Key, ".", app);
                var webEngineCore = this.Include<Qt.WebEngineCore>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.WebChannel>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.WebEngineWidgets>(C.DynamicLibrary.Key, ".", app);

                var webEngineProcess = this.Include<Qt.QtWebEngineProcess>(C.Cxx.ConsoleApplication.Key, ".", app);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.ChangeRPath(webEngineProcess, "$ORIGIN");
                }

                this.IncludeFile(webEngineCore.SourceModule.Macros["ICUDTL"], "resources", app);
                this.IncludeFile(webEngineCore.SourceModule.Macros["ResourcePak"], "resources", app);
                this.IncludeFile(webEngineCore.SourceModule.Macros["ResourcePak100p"], "resources", app);
                this.IncludeFile(webEngineCore.SourceModule.Macros["ResourcePak200p"], "resources", app);

                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                    this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                    (app.SourceModule as WebBrowser).Linker is VisualCCommon.LinkerBase)
                {
                    var visualCRuntimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                    foreach (var libpath in visualCRuntimeLibrary.CRuntimePaths((app.SourceModule as C.CModule).BitDepth))
                    {
                        this.IncludeFile(libpath, ".", app);
                    }
                    foreach (var libpath in visualCRuntimeLibrary.CxxRuntimePaths((app.SourceModule as C.CModule).BitDepth))
                    {
                        this.IncludeFile(libpath, ".", app);
                    }
                }
            }
#endif
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt5WebBrowsingTestDebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateSymbolsFrom<Qt5WebBrowsingTestRuntime>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt5WebBrowsingTestStripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.StripBinariesFrom<Qt5WebBrowsingTestRuntime, Qt5WebBrowsingTestDebugSymbols>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class TarBallInstaller :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.SourceFolder<Qt5WebBrowsingTestStripped>(Publisher.StrippedBinaryCollation.Key);
        }
    }
}
