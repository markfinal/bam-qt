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
using System.Linq;
namespace Qt
{
    public class MetaData :
        Bam.Core.PackageMetaData,
        QtCommon.IICUMeta
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        public MetaData()
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                // TODO: reported this different version to Qt: https://bugreports.qt.io/browse/QTBUG-52083
                this.Meta.Add("ICUVersion", "54");
                var visualcPackage = Bam.Core.Graph.Instance.Packages.FirstOrDefault(item => item.Name == "VisualC");
                if (null == visualcPackage)
                {
                    throw new Bam.Core.Exception("Unable to locate the VisualC package");
                }
                var visualcVersion = visualcPackage.Version;
                if (visualcVersion == "12.0")
                {
                    this.Meta.Add("MSVCFlavour", "msvc2013_64");
                }
                else if (visualcVersion == "14.0")
                {
                    this.Meta.Add("MSVCFlavour", "msvc2015_64");
                }
                else
                {
                    throw new Bam.Core.Exception("VisualC version {0} not supported by this Qt installation", visualcVersion);
                }
            }
            else
            {
                this.Meta.Add("ICUVersion", "56");
            }
        }

        public override object this[string index]
        {
            get
            {
                return this.Meta[index];
            }
        }

        public override bool
        Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        string QtCommon.IICUMeta.Version
        {
            get
            {
                return this.Meta["ICUVersion"] as string;
            }
        }
    }
}
