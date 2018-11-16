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
using Bam.Core;
namespace QtCommon
{
    [C.Prebuilt]
    public abstract class CommonPlugin :
        C.Cxx.Plugin
    {
        protected CommonPlugin()
        {
            var graph = Bam.Core.Graph.Instance;
            graph.Macros.Add("QtInstallPath", Configure.InstallPath);
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug)
                {
                    this.RegisterGeneratedFile(
                        ExecutableKey,
                        this.CreateTokenizedString("$(QtInstallPath)/plugins/$(QtPluginDir)/$(QtPluginName)d.dll")
                    );
                }
                else
                {
                    this.RegisterGeneratedFile(
                        ExecutableKey,
                        this.CreateTokenizedString("$(QtInstallPath)/plugins/$(QtPluginDir)/$(QtPluginName).dll")
                    );
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.RegisterGeneratedFile(
                    ExecutableKey,
                    this.CreateTokenizedString("$(QtInstallPath)/plugins/$(QtPluginDir)/lib$(QtPluginName).so")
                );
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                if (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug)
                {
                    this.RegisterGeneratedFile(
                        ExecutableKey,
                        this.CreateTokenizedString("$(QtInstallPath)/plugins/$(QtPluginDir)/lib$(QtPluginName)_debug.dylib")
                    );
                }
                else
                {
                    this.RegisterGeneratedFile(
                        ExecutableKey,
                        this.CreateTokenizedString("$(QtInstallPath)/plugins/$(QtPluginDir)/lib$(QtPluginName).dylib")
                    );
                }
            }

            var dependentTypes = this.RuntimeDependentModules;
            if (null != dependentTypes)
            {
                var requiredToExistMethod = this.GetType().GetMethod("RequiredToExist");
                foreach (var depType in dependentTypes)
                {
                    var genericVersionForModuleType = requiredToExistMethod.MakeGenericMethod(depType);
                    genericVersionForModuleType.Invoke(this, new [] { new C.CModule[0] });
                }
            }
        }

        protected virtual Bam.Core.TypeArray RuntimeDependentModules => null;
    }
}
