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
namespace QtCommon.Options
{
    class QtInstallPath :
        Bam.Core.IStringCommandLineArgument,
        Bam.Core.ICommandLineArgumentDefault<string>
    {
        string Bam.Core.ICommandLineArgument.ContextHelp => "Define the Qt install location";
        string Bam.Core.ICommandLineArgument.LongName => "--Qt.installPath";
        string Bam.Core.ICommandLineArgument.ShortName => null;
        string Bam.Core.ICommandLineArgumentDefault<string>.Default
        {
            get
            {
                var graph = Bam.Core.Graph.Instance;
                var qtVersion = graph.Packages.First(item => item.Name == "Qt").Version;

                switch (Bam.Core.OSUtilities.CurrentOS)
                {
                    case Bam.Core.EPlatform.Windows:
                        return GetWindowsInstallPath(qtVersion);

                    case Bam.Core.EPlatform.Linux:
                        return GetLinuxInstallPath(qtVersion);

                    case Bam.Core.EPlatform.OSX:
                        return GetOSXInstallPath(qtVersion);
                }

                throw new Bam.Core.Exception($"Unable to determine default Qt {qtVersion} installation");
            }
        }

        private static string
        GetWindowsInstallPath(
            string qtVersion)
        {
            var registrySubKey = "Qt " + qtVersion;
            var qtMeta = Bam.Core.Graph.Instance.PackageMetaData<Bam.Core.PackageMetaData>("Qt");
            var msvcFlavour = qtMeta["MSVCFlavour"] as string;

            var uninstallKey = @"Microsoft\Windows\CurrentVersion\Uninstall";
            using (var key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(uninstallKey))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception(
                        $@"Could not detect if Qt {qtVersion} libraries were installed; checked registry at HKEY_CURRENT_USER\Software\{uninstallKey}"
                    );
                }

                foreach (var subKey in key.SubKeys)
                {
                    var displayName = subKey.FindStringValue("DisplayName");
                    if (null != displayName)
                    {
                        if (displayName == registrySubKey)
                        {
                            try
                            {
                                var installDir = subKey.GetStringValue("InstallLocation");
                                if (!System.IO.Directory.Exists(installDir))
                                {
                                    throw new Bam.Core.Exception(
                                        $"Qt {qtVersion} installation directory, {installDir}, does not exist"
                                    );
                                }

                                var qtVersionSplit = qtVersion.Split('.');

                                var qtInstallPath = $@"{installDir}\{qtVersionSplit[0]}.{qtVersionSplit[1]}\{msvcFlavour}";

                                Bam.Core.Log.DebugMessage($"Qt installation folder is {qtInstallPath}");
                                return qtInstallPath;
                            }
                            catch (Bam.Core.Exception exception)
                            {
                                throw new Bam.Core.Exception(
                                    exception,
                                    $@"InstallLocation registry key for Qt {qtVersion} at HKEY_CURRENT_USER\Software\{uninstallKey}\{subKey.Name}\InstallationLocation is invalid"
                                );
                            }
                        }
                    }
                }
            }

            throw new Bam.Core.Exception($"Unable to detect from the Windows registry whether Qt {qtVersion} was installed");
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

            var installPath = $"{homeDir}/Qt{qtVersion}/{qtVersionSplit[0]}.{qtVersionSplit[1]}/gcc_64";
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

            var installPath = $"{homeDir}/Qt{qtVersion}/{qtVersionSplit[0]}.{qtVersionSplit[1]}/clang_64";
            return installPath;
        }
    }
}
