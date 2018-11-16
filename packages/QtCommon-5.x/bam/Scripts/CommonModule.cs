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
    public abstract class CommonModule :
        C.Cxx.DynamicLibrary
    {
        protected CommonModule(
            string moduleName,
            bool hasHeaders = true,
            bool hasPrefix = true,
            System.Tuple<string,string,string> customVersionNumber = null)
        {
            var graph = Bam.Core.Graph.Instance;
            graph.Macros.Add("QtInstallPath", Configure.InstallPath);

            this.Macros.AddVerbatim("QtModulePrefix", hasPrefix ? "Qt5" : string.Empty);
            this.Macros.AddVerbatim("QtModuleName", moduleName);
            this.HasHeaders = hasHeaders;
            this.CustomVersionNumber = customVersionNumber;
        }

        private bool HasHeaders { get; set; }
        private System.Tuple<string,string,string> CustomVersionNumber { get; set; }
        protected virtual Bam.Core.TypeArray RuntimeDependentModules => null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // using the real Qt installation directory allows headers in IDE projects to use more intuitive relative paths
            this.Macros["packagedir"] = Configure.InstallPath;

            if (null == this.CustomVersionNumber)
            {
                var version = Configure.Version;
                this.SetSemanticVersion(version[0], version[1], version[2]);
            }
            else
            {
                this.SetSemanticVersion(
                    this.CustomVersionNumber.Item1,
                    this.CustomVersionNumber.Item2,
                    this.CustomVersionNumber.Item3
                );
            }

            this.Macros.Add("QtIncludePath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/include", null));
            this.Macros.Add("QtLibraryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", null));
            this.Macros.Add("QtBinaryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/bin", null));
            this.Macros.Add("QtConfig", Bam.Core.TokenizedString.CreateVerbatim((this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ? "d" : string.Empty));

            if (this.HasHeaders)
            {
                // note the wildcard, to capture headers without extensions too
                this.CreateHeaderContainer("$(QtIncludePath)/Qt$(QtModuleName)/**.*", this);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("$(QtModulePrefix)$(QtModuleName)$(QtConfig)");
                this.RegisterGeneratedFile(
                    ExecutableKey,
                    this.CreateTokenizedString("$(QtBinaryPath)/$(dynamicprefix)$(OutputName)$(dynamicext)")
                );
                this.RegisterGeneratedFile(
                    ImportLibraryKey,
                    this.CreateTokenizedString("$(QtLibraryPath)/$(libprefix)$(OutputName)$(libext)")
                );
            }
            else
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("$(QtModulePrefix)$(QtModuleName)");
                this.RegisterGeneratedFile(
                    ExecutableKey,
                    this.CreateTokenizedString("$(QtLibraryPath)/$(dynamicprefix)$(OutputName)$(dynamicext)")
                );

                var linkerName = Bam.Core.Module.Create<CommonModuleSymbolicLink>(preInitCallback:module=>
                    {
                        module.Macros.Add("SymlinkFilename", this.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(linkernameext)"));
                        module.SharedObject = this;
                    });
                this.LinkerNameSymbolicLink = linkerName;

                var SOName = Bam.Core.Module.Create<CommonModuleSymbolicLink>(preInitCallback:module=>
                    {
                        module.Macros.Add("SymlinkFilename", this.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(sonameext)"));
                        module.SharedObject = this;
                    });
                this.SONameSymbolicLink = SOName;
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonCompilerSettings compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                    }

                    if (settings is C.ICommonLinkerSettings linker)
                    {
                        linker.LibraryPaths.AddUnique(this.Macros["QtLibraryPath"]);
                    }
                });

            var dependentTypes = this.RuntimeDependentModules;
            if (null != dependentTypes)
            {
                var requiredToExistMethod = this.GetType().GetMethod("RequiredToExist");
                foreach (var depType in dependentTypes)
                {
                    var genericVersionForModuleType = requiredToExistMethod.MakeGenericMethod(depType);
                    genericVersionForModuleType.Invoke(this, new [] { new C.CModule[0] });
                }
            }
        }
    }
}
