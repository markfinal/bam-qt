#region License
// Copyright (c) 2010-2016, Mark Final
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
    public abstract class CommonModule :
        C.Cxx.DynamicLibrary
    {
        protected CommonModule(
            string moduleName,
            bool hasHeaders = true)
        {
            this.Macros.AddVerbatim("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
            this.HasHeaders = hasHeaders;
        }

        private bool HasHeaders
        {
            get;
            set;
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // aliasing the packagedir to the real Qt installation directory allows headers in IDE projects to use more intuitive relative paths
            if (!this.Macros["packagedir"].IsAliased)
            {
                this.Macros["packagedir"].Aliased(Configure.InstallPath);
            }

            var version = Configure.Version;
            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(version[0]);
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim(version[1]);
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim(version[2]);

            this.Macros.Add("QtIncludePath", this.CreateTokenizedString("$(QtInstallPath)/include"));
            this.Macros.Add("QtLibraryPath", this.CreateTokenizedString("$(QtInstallPath)/lib"));
            this.Macros.Add("QtBinaryPath", this.CreateTokenizedString("$(QtInstallPath)/bin"));
            this.Macros.Add("QtConfig", Bam.Core.TokenizedString.CreateVerbatim((this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ? "d" : string.Empty));

            if (this.HasHeaders)
            {
                // note the wildcard, to capture headers without extensions too
                this.CreateHeaderContainer("$(QtIncludePath)/Qt$(QtModuleName)/**.*", this);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("Qt5$(QtModuleName)$(QtConfig)");
                this.GeneratedPaths[Key] = this.CreateTokenizedString("$(QtBinaryPath)/$(dynamicprefix)$(OutputName)$(dynamicext)");
                this.GeneratedPaths[ImportLibraryKey] = this.CreateTokenizedString("$(QtLibraryPath)/$(libprefix)$(OutputName)$(libext)");
            }
            else
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("Qt5$(QtModuleName)");
                this.GeneratedPaths[Key] = this.CreateTokenizedString("$(QtLibraryPath)/$(dynamicprefix)$(OutputName)$(dynamicext)");
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                }

                var linker = settings as C.ICommonLinkerSettings;
                if (null != linker)
                {
                    linker.LibraryPaths.AddUnique(this.Macros["QtLibraryPath"]);
                }
            });
        }
    }
}
