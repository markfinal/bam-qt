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
namespace QtCommon
{
    public abstract class WebEngineCore :
        CommonModule
    {
        public WebEngineCore() :
            base("WebEngineCore")
        { }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros.Add("ICUDTL", this.CreateTokenizedString("$(QtInstallPath)/resources/icudtl.dat"));
            this.Macros.Add("ResourcePak", this.CreateTokenizedString("$(QtInstallPath)/resources/qtwebengine_resources.pak"));
        }
    }

    public abstract class WebEngineCoreFramework :
        CommonFramework
    {
        public WebEngineCoreFramework() :
            base("WebEngineCore")
        { }

        public override Bam.Core.Array<C.OSXFramework.Path> DirectoriesToPublish
        {
            get
            {
                // ignore the base, as it's empty
                var toPublish = new Bam.Core.Array<Path>();
                toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Helpers/QtWebEngineProcess.app/")));
                toPublish.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/qtwebengine_locales/")));
                return toPublish;
            }
        }

        public override Bam.Core.Array<C.OSXFramework.Path> FilesToPublish
        {
            get
            {
                var files = base.FilesToPublish;
                files.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/icudtl.dat")));
                files.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/qtwebengine_resources.pak")));
                files.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/qtwebengine_resources_100p.pak")));
                files.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Versions/5/Resources/qtwebengine_resources_200p.pak")));
                return files;
            }
        }

        public override Bam.Core.Array<C.OSXFramework.Path> SymlinksToPublish
        {
            get
            {
                var symlinks = base.SymlinksToPublish;
                symlinks.Add(new Path(this.CreateTokenizedString("$(QtFramework)/Helpers")));
                return symlinks;
            }
        }
    }
}
