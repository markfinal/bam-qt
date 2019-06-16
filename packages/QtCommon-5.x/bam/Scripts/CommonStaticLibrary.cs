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
namespace QtCommon
{
    [C.Prebuilt]
    [Bam.Core.ModuleGroup("Thirdparty/Qt5")]
    public abstract class CommonStaticLibrary :
        C.StaticLibrary
    {
        protected CommonStaticLibrary(
            string moduleName,
            bool hasHeaders = true,
            bool hasPrefix = true)
        {
            var graph = Bam.Core.Graph.Instance;
            graph.Macros.Add("QtInstallPath", Configure.InstallPath);

            this.Macros.AddVerbatim("QtModulePrefix", hasPrefix ? "Qt5" : string.Empty);
            this.Macros.AddVerbatim("QtModuleName", moduleName);
            this.SetQtConfig();
            this.HasHeaders = hasHeaders;
        }

        private void
        SetQtConfig()
        {
            var vcMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            if (null != vcMeta)
            {
                if (vcMeta.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebug ||
                    vcMeta.RuntimeLibrary == VisualCCommon.ERuntimeLibrary.MultiThreadedDebugDLL)
                {
                    this.Macros.Add("QtConfig", Bam.Core.TokenizedString.CreateVerbatim("d"));
                    return;
                }
            }
            this.Macros.Add("QtConfig", Bam.Core.TokenizedString.CreateVerbatim(string.Empty));
        }

        private bool HasHeaders { get; set; }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // using the real Qt installation directory allows headers in IDE projects to use more intuitive relative paths
            this.Macros["packagedir"] = Configure.InstallPath;

            this.Macros.Add("QtIncludePath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/include", null));
            this.Macros.Add("QtLibraryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", null));

            if (this.HasHeaders)
            {
                // note the wildcard, to capture headers without extensions too
                this.CreateHeaderContainer("$(QtIncludePath)/Qt$(QtModuleName)/**.*", this);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("$(QtModulePrefix)$(QtModuleName)$(QtConfig)");
                this.RegisterGeneratedFile(
                    LibraryKey,
                    this.CreateTokenizedString("$(QtLibraryPath)/$(libprefix)$(OutputName)$(libext)")
                );
            }
            else
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("$(QtModulePrefix)$(QtModuleName)");
                this.RegisterGeneratedFile(
                    LibraryKey,
                    this.CreateTokenizedString("$(QtLibraryPath)/$(libprefix)$(OutputName)$(libext)")
                );
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                    }

                    if (settings is C.ICommonLinkerSettings linker)
                    {
                        linker.LibraryPaths.AddUnique(this.Macros["QtLibraryPath"]);
                    }
                });
        }
    }
}
