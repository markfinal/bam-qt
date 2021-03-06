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
namespace Qt4Test1
{
    sealed class QtApplication :
        C.Cxx.ConsoleApplication
    {
        public QtApplication()
        {
            if (!this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.BitDepth = C.EBit.ThirtyTwo;
            }
        }

        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCxxSourceCollection("$(packagedir)/source/*.cpp");
            var mocHeaders = this.CreateHeaderCollection("$(packagedir)/source/myobject*.h");
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
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("COMPILING_GENERATED_MOC");
                });
            }

            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                if (gccLinker != null)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.Add("$ORIGIN");
                }
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompileAndLinkAgainst<Qt.CoreFramework>(source);
                this.CompileAndLinkAgainst<Qt.GuiFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.Core>(source);
                this.CompileAndLinkAgainst<Qt.Gui>(source);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.CreateWinResourceCollection("$(packagedir)/resources/*.rc");
            }
        }
    }

    sealed class Qt4Test1Runtime :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.WindowedApplication);
            this.Include<QtApplication>(C.ConsoleApplication.ExecutableKey);
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt4Test1DebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateSymbolsFrom<Qt4Test1Runtime>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Qt4Test1Stripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init()
        {
            base.Init();

            this.StripBinariesFrom<Qt4Test1Runtime, Qt4Test1DebugSymbols>();
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

            this.SourceFolder<Qt4Test1Stripped>(Publisher.StrippedBinaryCollation.StripBinaryDirectoryKey);
        }
    }
}
