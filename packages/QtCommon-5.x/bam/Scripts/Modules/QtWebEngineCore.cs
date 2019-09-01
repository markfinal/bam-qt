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
    abstract class WebEngineCore :
        CommonModule
    {
        public WebEngineCore() :
            base("WebEngineCore")
        { }

        protected override Bam.Core.TypeArray RuntimeDependentModules
        {
            get
            {
                return new Bam.Core.TypeArray {
                    typeof(Qt.WebChannel),
                    typeof(Qt.Positioning),
                    typeof(Qt.QtWebEngineProcess)
                };
            }
        }

        protected override void
        Init()
        {
            base.Init();

            this.Macros.Add("ICUDTL", this.CreateTokenizedString("$(QtInstallPath)/resources/icudtl.dat"));
            this.Macros.Add("ResourcePak", this.CreateTokenizedString("$(QtInstallPath)/resources/qtwebengine_resources.pak"));
            this.Macros.Add("ResourcePak100p", this.CreateTokenizedString("$(QtInstallPath)/resources/qtwebengine_resources_100p.pak"));
            this.Macros.Add("ResourcePak200p", this.CreateTokenizedString("$(QtInstallPath)/resources/qtwebengine_resources_200p.pak"));
            this.Macros.Add("Locales", this.CreateTokenizedString("$(QtInstallPath)/translations/qtwebengine_locales"));
        }
    }

    abstract class WebEngineCoreFramework :
        CommonFramework
    {
        public WebEngineCoreFramework() :
            base("WebEngineCore")
        { }

        protected override Bam.Core.TypeArray RuntimeDependentModules
        {
            get
            {
                // QtWebEngineProcess is included as a Helper in the framework
                return new Bam.Core.TypeArray {
                    typeof(Qt.WebChannelFramework),
                    typeof(Qt.PositioningFramework)
                };
            }
        }
    }
}
