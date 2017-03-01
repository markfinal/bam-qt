#region License
// Copyright (c) 2010-2017, Mark Final
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
using System.Linq;
namespace QtCommon
{
    public interface IICUMeta
    {
        string Version
        {
            get;
        }
    }

    [C.Prebuilt]
    public abstract class ICUBase :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var qtPackage = Bam.Core.Graph.Instance.Packages.First(item => item.Name == "Qt");
            var qtMeta = qtPackage.MetaData as IICUMeta;
            this.Macros.Add("ICUVersion", Bam.Core.TokenizedString.CreateVerbatim(qtMeta.Version));

            this.Macros["MajorVersion"] = this.Macros["ICUVersion"];
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("1");
            this.Macros.Remove("PatchVersion"); // does not use this part of the version numbering system

            this.Macros.Add("QtInstallPath", Configure.InstallPath);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros.Add("ICUInstallPath", this.CreateTokenizedString("$(QtInstallPath)/bin"));
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros.Add("ICUInstallPath", this.CreateTokenizedString("$(QtInstallPath)/lib"));
            }

            this.GeneratedPaths[C.DynamicLibrary.Key] = this.CreateTokenizedString("$(ICUInstallPath)/$(dynamicprefix)$(OutputName)$(dynamicext)");
        }
    }

    public sealed class ICUIN :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("icuin$(ICUVersion)");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("icui18n");
            }
            base.Init(parent);
        }
    }

    public sealed class ICUUC :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("icuuc$(ICUVersion)");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("icuuc");
            }
            base.Init(parent);
        }
    }

    public sealed class ICUDT :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = this.CreateTokenizedString("icudt$(ICUVersion)");
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.CreateVerbatim("icudata");
            }
            base.Init(parent);
        }
    }
}
