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
namespace QtCommon.RccExtension
{
    public static class RccExtension
    {
        public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
        Rcc(
            this C.Cxx.ObjectFileCollection collection,
            QRCFile qrcFile)
        {
            // rcc the .qrc file to generate the source file
            var rccSourceFile = Bam.Core.Module.Create<RccGeneratedSource>(collection);

            // compile the generated source file
            var objFile = collection.AddFile(rccSourceFile);

            // set the source file AFTER the source has been chained into the object file
            // so that the encapsulating module can be determined
            rccSourceFile.SourceHeader = qrcFile;

            // return both rcc'd source, and the compiled object file
            return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(rccSourceFile, objFile);
        }

        public static QRCFileCollection
        CreateQrcCollection(
            this C.CModule module,
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = Bam.Core.Module.Create<QRCFileCollection>(module);
            module.Requires(source);
            if (null != wildcardPath)
            {
                (source as C.IAddFiles).AddFiles(wildcardPath, macroModuleOverride: macroModuleOverride, filter: filter);
            }
            return source;
        }
    }
}
