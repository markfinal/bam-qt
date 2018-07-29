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
namespace QtCommon
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        Rcc(
            RccGeneratedSource module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);
            var rule = meta.AddRule();
            rule.AddTarget(module.GeneratedPaths[RccGeneratedSource.SourceFileKey]);
            foreach (var input in module.InputModules)
            {
                System.Diagnostics.Debug.Assert(input.Key == C.HeaderFile.HeaderFileKey);
                rule.AddPrerequisite(input.Value, input.Key);
            }

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);

            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }
        }
    }
#else
    public sealed class MakeFileRccGeneration :
        IRccGenerationPolicy
    {
        void
        IRccGenerationPolicy.Rcc(
            RccGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool rccCompiler,
            Bam.Core.TokenizedString generatedRccSource,
            QRCFile source)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(generatedRccSource);
            rule.AddPrerequisite(source, C.HeaderFile.HeaderFileKey);

            var rccOutputPath = generatedRccSource.ToString();
            var rccOutputDir = System.IO.Path.GetDirectoryName(rccOutputPath);

            var args = new Bam.Core.StringArray();
            args.Add(CommandLineProcessor.Processor.StringifyTool(rccCompiler));
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o {0}", rccOutputPath));
            args.Add(source.InputPath.ToString());
            rule.AddShellCommand(args.ToString(' '));

            meta.CommonMetaData.AddDirectory(rccOutputDir);
        }
    }
#endif
}
