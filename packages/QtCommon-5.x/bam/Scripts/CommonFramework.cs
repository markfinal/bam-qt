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
namespace QtCommon
{
    [C.Prebuilt]
    abstract class CommonFramework :
        C.OSXFramework
    {
        protected CommonFramework(
            string moduleName)
        {
            var graph = Bam.Core.Graph.Instance;
            graph.Macros.Add("QtInstallPath", Configure.InstallPath);

            this.Macros.AddVerbatim("QtModuleName", moduleName);
            this.Macros.Add("QtFrameworkPath", this.CreateTokenizedString("$(QtInstallPath)/lib"));
            this.Macros.Add("QtFramework", this.CreateTokenizedString("Qt$(QtModuleName).framework"));
        }

        protected virtual Bam.Core.TypeArray RuntimeDependentModules => null;

        protected override void
        Init()
        {
            base.Init();

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonCompilerSettingsOSX osxCompiler)
                    {
                        osxCompiler.FrameworkSearchPaths.AddUnique(this.Macros["QtFrameworkPath"]);
                    }

                    if (settings is C.ICommonLinkerSettingsOSX osxLinker)
                    {
                        osxLinker.Frameworks.AddUnique(this.CreateTokenizedString("$(QtFrameworkPath)/$(QtFramework)"));
                        osxLinker.FrameworkSearchPaths.AddUnique(this.Macros["QtFrameworkPath"]);
                    }
                });

            var dependentTypes = this.RuntimeDependentModules;
            if (null != dependentTypes)
            {
                var graph = Bam.Core.Graph.Instance;
                var findReferencedModuleMethod = graph.GetType().GetMethod("FindReferencedModule", System.Type.EmptyTypes);
                foreach (var depType in dependentTypes)
                {
                    var genericVersionForModuleType = findReferencedModuleMethod.MakeGenericMethod(depType);
                    var depModule = genericVersionForModuleType.Invoke(graph, null) as Bam.Core.Module;
                    this.Requires(depModule);
                }
            }
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        public override Bam.Core.TokenizedString FrameworkPath => this.Macros["QtFrameworkPath"];
        protected override Bam.Core.TokenizedString FrameworkBundleName => this.Macros["QtFramework"];
        protected override Bam.Core.TokenizedString FrameworkLibraryPath => this.CreateTokenizedString("$(QtFramework)/Versions/5/Qt$(QtModuleName)");
        public virtual Bam.Core.TokenizedStringArray PublishingExclusions
        {
            get
            {
                var exclusions = new Bam.Core.TokenizedStringArray
                {
                    Bam.Core.TokenizedString.CreateVerbatim("Headers/"),
                    Bam.Core.TokenizedString.CreateVerbatim("Headers"),
                    Bam.Core.TokenizedString.CreateVerbatim("*_debug"),
                    Bam.Core.TokenizedString.CreateVerbatim("*.prl")
                };
                return exclusions;
            }
        }
    }
}
