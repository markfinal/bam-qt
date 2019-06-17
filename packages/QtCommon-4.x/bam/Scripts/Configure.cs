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
using System.Linq;
namespace QtCommon
{
    public static class Configure
    {
        static Configure()
        {
            var graph = Bam.Core.Graph.Instance;
            var qtPackage = graph.Packages.FirstOrDefault(item => item.Name == "Qt");
            if (null == qtPackage)
            {
                throw new Bam.Core.Exception("Unable to locate Qt package for this build");
            }
            var qtVersion = qtPackage.Version;
            Version = new Bam.Core.StringArray(qtVersion.Split(new [] {'.'}));

            var qtInstallDir = Bam.Core.CommandLineProcessor.Evaluate(new Options.QtInstallPath());
            if (!System.IO.Directory.Exists(qtInstallDir))
            {
                throw new Bam.Core.Exception("Qt install dir, {0}, does not exist", qtInstallDir);
            }
            InstallPath = Bam.Core.TokenizedString.CreateVerbatim(qtInstallDir);

            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                ExecutableExtension = Bam.Core.TokenizedString.CreateVerbatim(".exe");
            }
            else
            {
                ExecutableExtension = Bam.Core.TokenizedString.CreateVerbatim(string.Empty);
            }
        }

        public static Bam.Core.TokenizedString InstallPath
        {
            get;
            private set;
        }

        public static Bam.Core.StringArray Version
        {
            get;
            private set;
        }

        public static Bam.Core.TokenizedString ExecutableExtension
        {
            get;
            private set;
        }
    }
}
