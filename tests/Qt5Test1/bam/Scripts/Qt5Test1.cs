#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace Qt5Test1
{
    sealed class Qt5Application :
        C.Cxx.GUIApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var mocHeaders = this.CreateHeaderCollection("$(packagedir)/source/*.h");
            var source = this.CreateCxxSourceCollection("$(packagedir)/source/*.cpp");
            foreach (var mocHeader in mocHeaders.Children)
            {
                var myObjectMocTuple = source.MocHeader(mocHeader as C.HeaderFile);

                // first item in Tuple is the generated moc source file
                myObjectMocTuple.Item1.PrivatePatch(settings =>
                    {
                        var mocSettings = settings as QtCommon.IMocSettings;
                        mocSettings.PreprocessorDefinitions.Add("GENERATING_MOC");
                    });

                // second item in Tuple is the C++ compilation of that generated source
                myObjectMocTuple.Item2.PrivatePatch(settings =>
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.PreprocessorDefines.Add("COMPILING_GENERATED_MOC");
                    });
            }

            source.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        // because Qt5.6.0/5.6/gcc_64/include/QtCore/qglobal.h:1090:4: error: #error "You must build your code with position independent code if Qt was built with -reduce-relocations. " "Compile your code with -fPIC (-fPIE is not enough)."
                        gccCompiler.PositionIndependentCode = true;
                    }

                    if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                    {
                        if (source.Compiler.Version.AtLeast(VisualCCommon.ToolchainVersion.VC2015))
                        {
                            var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                            cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous; // C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\include\iosfwd(343): warning C4577: 'noexcept' used with no exception handling mode specified; termination on exception is not guaranteed. Specify /EHsc
                        }
                    }
                });

            this.PrivatePatch(settings =>
                {
                    if (settings is GccCommon.ICommonLinkerSettings gccLinker)
                    {
                        gccLinker.CanUseOrigin = true;
                        gccLinker.RPath.AddUnique("$ORIGIN/../lib");
                    }

                    if (settings is ClangCommon.ICommonLinkerSettings clangLinker)
                    {
                        clangLinker.RPath.AddUnique("@executable_path/../Frameworks");
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompileAndLinkAgainst<Qt.CoreFramework>(source);
                this.CompileAndLinkAgainst<Qt.WidgetsFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.Core>(source);
                this.CompileAndLinkAgainst<Qt.Widgets>(source);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CreateWinResourceCollection("$(packagedir)/resources/*.rc");
            }
        }
    }

    sealed class Qt5Test1Runtime :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.WindowedApplication);
            var appAnchor = this.Include<Qt5Application>(C.Cxx.GUIApplication.ExecutableKey);

            var qtPlatformPlugin = this.Find<QtCommon.PlatformPlugin>().First();
            (qtPlatformPlugin as Publisher.CollatedObject).SetPublishingDirectory("$(0)/platforms", this.PluginDir);

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
                this.IncludeFiles(
                    this.CreateTokenizedString("$(packagedir)/resources/linux/qt.conf"),
                    this.ExecutableDir,
                    appAnchor);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.IncludeFiles(
                    this.CreateTokenizedString("$(packagedir)/resources/windows/qt.conf"),
                    this.ExecutableDir,
                    appAnchor);

                var app = appAnchor.SourceModule as Qt5Application;
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
                throw new Bam.Core.Exception("Unknown platform");
            }
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt5Test1DebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateSymbolsFrom<Qt5Test1Runtime>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt5Test1Stripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.StripBinariesFrom<Qt5Test1Runtime, Qt5Test1DebugSymbols>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class TarBallInstaller :
        Installer.TarBall
    {
        protected override void
        Init()
        {
            base.Init();

            this.SourceFolder<Qt5Test1Stripped>(Publisher.StrippedBinaryCollation.StripBinaryDirectoryKey);
        }
    }
}
