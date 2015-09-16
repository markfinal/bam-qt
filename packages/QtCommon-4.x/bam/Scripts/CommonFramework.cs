#region License
// Copyright (c) 2010-2015, Mark Final
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
    public abstract class CommonFramework :
        C.ExternalFramework
    {
        protected CommonFramework(
            string moduleName) :
            base()
        {
            this.Macros.Add("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtFrameworkPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", this));

            this.PublicPatch((settings, appliedTo) =>
            {
                var osxCompiler = settings as C.ICommonCompilerSettingsOSX;
                if (null != osxCompiler)
                {
                    osxCompiler.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }

                var osxLinker = settings as C.ILinkerSettingsOSX;
                if (null != osxLinker)
                {
                    osxLinker.Frameworks.AddUnique(Bam.Core.TokenizedString.Create("$(QtFrameworkPath)/Qt$(QtModuleName).framework", this));
                    osxLinker.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }
            });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // prebuilt - no execution policy
        }
    }
}