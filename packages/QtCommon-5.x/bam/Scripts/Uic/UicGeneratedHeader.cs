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
namespace QtCommon
{
    public class UicGeneratedHeader :
        C.HeaderFile
    {
        private QUIFile SourceUIFile;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<UicTool>();
            this.Requires(this.Compiler);
            this.InputPath = this.CreateTokenizedString(
                "$(encapsulatingbuilddir)/$(config)/@changeextension(@trimstart(@relativeto($(QUIFilePath),$(packagedir)),../),.h)"
            );
        }

        public QUIFile UIFile
        {
            get
            {
                return this.SourceUIFile;
            }
            set
            {
                if (null != this.SourceUIFile)
                {
                    throw new Bam.Core.Exception(".ui file has already been assigned");
                }
                this.SourceUIFile = value;
                this.DependsOn(value);
                this.Macros.Add("QUIFilePath", value.InputPath);
                this.GetEncapsulatingReferencedModule(); // or the path above won't be parsable prior to all modules having been created
            }
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[HeaderFileKey].ToString();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(
                    this.GeneratedPaths[HeaderFileKey]
                );
                return;
            }
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var headerFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceUIFile.InputPath.ToString());
            if (headerFileWriteTime > sourceFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[HeaderFileKey],
                    this.SourceUIFile.InputPath
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
                    MakeFileSupport.Uic(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.Uic(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.Uic(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.Uic(this);
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

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(
                    C.HeaderFile.HeaderFileKey,
                    this.UIFile
                );
            }
        }
    }
}
