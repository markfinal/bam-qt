#region License
// Copyright (c) 2010-2017, Mark Final
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
using QtCommon.RccExtension;
using System.Linq;
namespace Qt5Test2
{
    sealed class Qt5Application :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/*.cpp");

            var qrcResources = this.CreateQrcContainer("$(packagedir)/resources/*.qrc");
            foreach (var rccFile in qrcResources.Children)
            {
                /*var rccMocTuple = */source.Rcc(rccFile);
            }

            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN");
                    }

                    var clangLinker = settings as ClangCommon.ICommonLinkerSettings;
                    if (null != clangLinker)
                    {
                        clangLinker.RPath.AddUnique("@executable_path/../Frameworks");
                    }
                });

            source.PrivatePatch(settings =>
                {
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        // because Qt5.6.0/5.6/gcc_64/include/QtCore/qglobal.h:1090:4: error: #error "You must build your code with position independent code if Qt was built with -reduce-relocations. " "Compile your code with -fPIC (-fPIE is not enough)."
                        gccCompiler.PositionIndependentCode = true;
                    }

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        if (source.Compiler.IsAtLeast(14))
                        {
                            var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                            cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous; // C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\include\iosfwd(343): warning C4577: 'noexcept' used with no exception handling mode specified; termination on exception is not guaranteed. Specify /EHsc
                        }
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompileAndLinkAgainst<Qt.CoreFramework>(source);
            }
            else
            {
                var qtPackage = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "Qt");
                var qtVersionSplit = qtPackage.Version.Split('.');
                if (System.Convert.ToInt32(qtVersionSplit[1]) >= 5) // if 5.x >= 5.5
                {
                    this.CompileAndLinkAgainst<Qt.Core>(source); // requires link patches for ICU (at least on Linux)
                }
                else
                {
                    this.CompileAndLinkAgainst<Qt.Core>(source);
                }
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

    sealed class Qt5Test2Runtime :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacros(EPublishingType.WindowedApplication);
            this.Include<Qt5Application>(C.Cxx.GUIApplication.Key);
#else
            var app = this.Include<Qt5Application>(C.ConsoleApplication.Key, EPublishingType.WindowedApplication);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                var qtPackage = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "Qt");
                var qtVersionSplit = qtPackage.Version.Split('.');
                var updateInstallName = (System.Convert.ToInt32(qtVersionSplit[1]) < 5); // < Qt5.5 requires install name updates

                this.IncludeFramework<Qt.CoreFramework>("../Frameworks", app, updateInstallName: updateInstallName);

                // required by the platform plugin
                this.IncludeFramework<Qt.PrintSupportFramework>("../Frameworks", app, updateInstallName: updateInstallName);
#if D_PACKAGE_QT_5_5_1 || D_PACKAGE_QT_5_6_0 || D_PACKAGE_QT_5_6_1 || D_PACKAGE_QT_5_6_2
                this.IncludeFramework<Qt.DBusFramework>("../Frameworks", app, updateInstallName: updateInstallName);
#endif

                this.Include<Qt.PlatformPlugin>(C.Plugin.Key, "../Plugins/qtplugins/platforms", app);
                this.IncludeFile(this.CreateTokenizedString("$(packagedir)/resources/osx/qt.conf"), "../Resources", app);
            }
            else
            {
                this.Include<Qt.Core>(C.DynamicLibrary.Key, ".", app);

                this.IncludeFile(this.CreateTokenizedString("$(packagedir)/resources/qt.conf"), ".", app);
                var platformPlugin = this.Include<Qt.PlatformPlugin>(C.Plugin.Key, "qtplugins/platforms", app);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.Include<QtCommon.ICUIN>(C.DynamicLibrary.Key, ".", app);
                    this.Include<QtCommon.ICUUC>(C.DynamicLibrary.Key, ".", app);
                    this.Include<QtCommon.ICUDT>(C.DynamicLibrary.Key, ".", app);

                    this.ChangeRPath(platformPlugin, "$ORIGIN/../..");
                    this.Include<Qt.DBus>(C.DynamicLibrary.Key, ".", app); // for qxcb plugin

#if D_PACKAGE_QT_5_5_1 || D_PACKAGE_QT_5_6_0 || D_PACKAGE_QT_5_6_1 || D_PACKAGE_QT_5_6_2
                    this.Include<Qt.XcbQpa>(C.DynamicLibrary.Key, ".", app);
#endif
                }

                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                    this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                    (app.SourceModule as Qt5Application).Linker is VisualCCommon.LinkerBase)
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
    sealed class Qt5Test2DebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
#else
            this.CreateSymbolsFrom<Qt5Test2Runtime>();
#endif
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt5Test2Stripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
#else
            this.StripBinariesFrom<Qt5Test2Runtime, Qt5Test2DebugSymbols>();
#endif
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

#if D_NEW_PUBLISHING
#else
            this.SourceFolder<Qt5Test2Stripped>(Publisher.StrippedBinaryCollation.Key);
#endif
        }
    }
}
