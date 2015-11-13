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
using System.Linq;
namespace QtCommon
{
    class QtInstallPath :
        Bam.Core.IStringCommandLineArgument,
        Bam.Core.ICommandLineArgumentDefault<string>
    {
        string Bam.Core.ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the Qt install location";
            }
        }

        string Bam.Core.ICommandLineArgument.LongName
        {
            get
            {
                return "--Qt.installPath";
            }
        }

        string Bam.Core.ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string Bam.Core.ICommandLineArgumentDefault<string>.Default
        {
            get
            {
                var graph = Bam.Core.Graph.Instance;
                var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

                switch (Bam.Core.OSUtilities.CurrentOS)
                {
                    case Bam.Core.EPlatform.Windows:
                        return GetWindowsInstallPath(qtVersion);

                    case Bam.Core.EPlatform.Linux:
                        return GetLinuxInstallPath(qtVersion);

                    case Bam.Core.EPlatform.OSX:
                        return GetOSXInstallPath(qtVersion);
                }

                throw new Bam.Core.Exception("Unable to determine default Qt {0} installation", qtVersion);
            }
        }

        private static string
        GetWindowsInstallPath(
            string qtVersion)
        {
            var registrySubKey = "Qt " + qtVersion;
            var qtMeta = Bam.Core.Graph.Instance.PackageMetaData<Bam.Core.PackageMetaData>("Qt");
            if (qtMeta.Contains("WindowsProductCode"))
            {
                registrySubKey = qtMeta["WindowsProductCode"] as string;
            }
            var msvcFlavour = qtMeta["MSVCFlavour"] as string;

            var keyPath = System.String.Format(@"Microsoft\Windows\CurrentVersion\Uninstall\{0}", registrySubKey);
            using (var key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(keyPath))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception(@"Could not detect if Qt {0} libraries were installed; checked registry at HKEY_CURRENT_USER\Software\{1}", qtVersion, keyPath);
                }

                var installDir = key.GetValue("InstallLocation") as string;
                if (null == installDir)
                {
                    throw new Bam.Core.Exception(@"Unable to locate InstallLocation registry key for Qt {0} at HKEY_CURRENT_USER\Software\{1}\InstallationLocation", qtVersion, keyPath);
                }

                var qtVersionSplit = qtVersion.Split('.');

                var qtInstallPath = System.String.Format(@"{0}\{1}.{2}\{3}", installDir, qtVersionSplit[0], qtVersionSplit[1], msvcFlavour);

                Bam.Core.Log.DebugMessage("Qt installation folder is {0}", qtInstallPath);
                return qtInstallPath;
            }
        }

        private static string
        GetLinuxInstallPath(
            string qtVersion)
        {
            var homeDir = System.Environment.GetEnvironmentVariable("HOME");
            if (null == homeDir)
            {
                throw new Bam.Core.Exception("Unable to determine home directory");
            }

            var qtVersionSplit = qtVersion.Split('.');

            var installPath = System.String.Format("{0}/Qt{1}/{2}.{3}/gcc_64", homeDir, qtVersion, qtVersionSplit[0], qtVersionSplit[1]);
            return installPath;
        }

        private static string
        GetOSXInstallPath(
            string qtVersion)
        {
            var homeDir = System.Environment.GetEnvironmentVariable("HOME");
            if (null == homeDir)
            {
                throw new Bam.Core.Exception("Unable to determine home directory");
            }

            var qtVersionSplit = qtVersion.Split('.');

            var installPath = System.String.Format("{0}/Qt{1}/{2}.{3}/clang_64", homeDir, qtVersion, qtVersionSplit[0], qtVersionSplit[1]);
            return installPath;
        }
    }
}
