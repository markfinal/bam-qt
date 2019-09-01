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
using Bam.Core;
namespace Qt
{
    sealed class Core :
        QtCommon.Core
    {
        protected override void
        Init()
        {
            base.Init();

#if D_PACKAGE_GCCCOMMON
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.PublicPatch((settings, appliedTo) =>
                    {
                        if (settings is GccCommon.ICommonLinkerSettings gccLinker)
                        {
                            // New in Qt 5.5
                            // required to link against Qt5Core, which depend on the icu shared libraries
                            gccLinker.RPathLink.AddUnique(this.CreateTokenizedString("$(0)/lib", QtCommon.Configure.InstallPath));
                        }
                    });
            }
#endif
        }
    }

    sealed class Concurrent :
        QtCommon.Concurrent
    { }

    sealed class DBus :
        QtCommon.DBus
    { }

    sealed class Declarative :
        QtCommon.Declarative
    { }

    sealed class Designer :
        QtCommon.Designer
    { }

    sealed class Gui :
        QtCommon.Gui
    { }

    sealed class Help :
        QtCommon.Help
    { }

    sealed class Multimedia :
        QtCommon.Multimedia
    { }

    sealed class MultimediaWidgets :
        QtCommon.MultimediaWidgets
    { }

    sealed class Network :
        QtCommon.Network
    { }

    sealed class OpenGL :
        QtCommon.OpenGL
    { }

    sealed class OpenVG :
        QtCommon.OpenVG
    { }

    sealed class Phonon :
        QtCommon.Phonon
    { }

    sealed class Positioning :
        QtCommon.Positioning
    { }

    sealed class PrintSupport :
        QtCommon.PrintSupport
    { }

    sealed class Qml :
        QtCommon.Qml
    { }

    sealed class Quick :
        QtCommon.Quick
    { }

    sealed class Script :
        QtCommon.Script
    { }

    sealed class ScriptTools :
        QtCommon.ScriptTools
    { }

    sealed class Sql :
        QtCommon.Sql
    { }

    sealed class Svg :
        QtCommon.Svg
    { }

    sealed class Test :
        QtCommon.Test
    { }

    sealed class WebKit :
        QtCommon.WebKit
    { }

    sealed class WebEngineCore :
        QtCommon.WebEngineCore
    { }

    sealed class WebChannel :
        QtCommon.WebChannel
    { }

    sealed class WebEngineWidgets :
        QtCommon.WebEngineWidgets
    { }

    sealed class Widgets :
        QtCommon.Widgets
    { }

    sealed class Xml :
        QtCommon.Xml
    { }

    sealed class XmlPatterns :
        QtCommon.XmlPatterns
    { }

    // specific to this version

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    sealed class XcbQpa :
        QtCommon.XcbQpa
    { }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    sealed class GSTTools :
        QtCommon.GSTTools
    { }
}
