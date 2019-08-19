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
    public class MocGeneratedSource :
        C.SourceFile
    {
        private C.HeaderFile SourceHeaderModule;

        protected override void
        Init()
        {
            base.Init();

            var graph = Bam.Core.Graph.Instance;
            this.Compiler = graph.FindReferencedModule<MocTool>();
            this.Requires(this.Compiler);

            var parentModule = graph.ModuleStack.Peek();
            this.InputPath = this.CreateTokenizedString(
                "$(0)/$(config)/@changeextension(@trimstart(@isrelative(@relativeto($(MOCHeaderPath),$(packagedir)),../),@filename($(MOCHeaderPath))),.moc.cpp)",
                new[] { parentModule.Macros[Bam.Core.ModuleMacroNames.PackageBuildDirectory] }
            );
        }

        public C.HeaderFile SourceHeader
        {
            get
            {
                return this.SourceHeaderModule;
            }
            set
            {
                if (null != this.SourceHeaderModule)
                {
                    throw new Bam.Core.Exception(".h file has already been assigned");
                }
                this.SourceHeaderModule = value;
                this.DependsOn(value);
                this.Macros.Add("MOCHeaderPath", value.InputPath);
            }
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[SourceFileKey].ToString();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(
                    this.GeneratedPaths[SourceFileKey]
                );
                return;
            }
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var headerFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceHeaderModule.InputPath.ToString());
            if (headerFileWriteTime > sourceFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[SourceFileKey],
                    this.SourceHeaderModule.InputPath
                );
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionBuilder.Support.AddCustomBuildStepForCommandLineTool(
                        this,
                        this.GeneratedPaths[SourceFileKey],
                        "Moc'ing",
                        false // headers already exist in the project
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    {
                        XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                            this,
                            out XcodeBuilder.Target target,
                            out XcodeBuilder.Configuration configuration,
                            true,
                            false,
                            outputPaths: new Bam.Core.TokenizedStringArray(this.GeneratedPaths[SourceFileKey])
                        );
                    }
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        private Bam.Core.PreBuiltTool Compiler
        {
            get
            {
                return this.Tool as Bam.Core.PreBuiltTool;
            }

            set
            {
                this.Tool = value;
            }
        }

        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module,string)> InputModulePaths
        {
            get
            {
                yield return (this.SourceHeaderModule, C.HeaderFile.HeaderFileKey);
            }
        }
    }
}
