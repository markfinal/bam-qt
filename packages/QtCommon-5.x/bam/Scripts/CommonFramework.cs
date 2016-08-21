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
using System.Linq;
namespace QtCommon
{
    [C.Prebuilt]
    public abstract class CommonFramework :
        C.OSXFramework
    {
        private bool FixIncorrectFrameworks = false;

        protected CommonFramework(
            string moduleName) :
            base()
        {
            this.Macros.AddVerbatim("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
            this.Macros.Add("QtFrameworkPath", this.CreateTokenizedString("$(QtInstallPath)/lib"));
            this.Macros.Add("QtFramework", this.CreateTokenizedString("Qt$(QtModuleName).framework"));

            // required for C.OSXFramework
            this.Macros["FrameworkLibraryPath"].Aliased(this.CreateTokenizedString("$(QtFramework)/Versions/5/Qt$(QtModuleName)"));

            var graph = Bam.Core.Graph.Instance;
            var qtPackage = graph.Packages.First(item => item.Name == "Qt");
            var qtVersion = qtPackage.Version;
            var qtVersionSplit = qtVersion.Split('.');
            var minorVersion = System.Convert.ToInt32(qtVersionSplit[1]);
            if (minorVersion < 5)
            {
                this.FixIncorrectFrameworks = true;
            }
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.PublicPatch((settings, appliedTo) =>
            {
                var osxCompiler = settings as C.ICommonCompilerSettingsOSX;
                if (null != osxCompiler)
                {
                    osxCompiler.FrameworkSearchPaths.AddUnique(this.Macros["QtFrameworkPath"]);
                }

                var osxLinker = settings as C.ICommonLinkerSettingsOSX;
                if (null != osxLinker)
                {
                    osxLinker.Frameworks.AddUnique(this.CreateTokenizedString("$(QtFrameworkPath)/$(QtFramework)"));
                    osxLinker.FrameworkSearchPaths.AddUnique(this.Macros["QtFrameworkPath"]);
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

        public override Bam.Core.TokenizedString FrameworkPath
        {
            get
            {
                return this.Macros["QtFrameworkPath"];
            }
        }

        public override Bam.Core.Array<Path> DirectoriesToPublish
        {
            get
            {
                return null;
            }
        }

        public override Bam.Core.Array<Path> FilesToPublish
        {
            get
            {
                var toPublish = new Bam.Core.Array<Path>();
                toPublish.Add(new Path(this.Macros["FrameworkLibraryPath"]));

                if (this.FixIncorrectFrameworks)
                {
                    // Info.plist is in the wrong location for codesigning
                    toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Contents/Info.plist"), this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/Info.plist")));
                }
                else
                {
                    toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/Info.plist")));
                }

                return toPublish;
            }
        }

        public override Bam.Core.Array<Path> SymlinksToPublish
        {
            get
            {
                var toPublish = new Bam.Core.Array<Path>();
                toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/Current")));
                toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Qt$(QtModuleName)")));
                if (this.FixIncorrectFrameworks)
                {
                    // Resources symlink does not exist in the SDK frameworks
                    toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Resources"), Bam.Core.TokenizedString.CreateVerbatim("Versions/5/Resources")));
                }
                else
                {
                    toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Resources")));
                }
                return toPublish;
            }
        }
    }
}
