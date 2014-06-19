namespace ScriptCs.Engine.Mono.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    public class ScriptSegementer
    {
        public List<SegmentResult> SegmentCode(string code)
        {
            const string ScriptPattern = @"#line 1.*?\n";
            var isScriptFile = Regex.IsMatch(code, ScriptPattern);
            if(isScriptFile)
            {
                // Remove debug line
                code = Regex.Replace(code, ScriptPattern, "");
            }

            var ss = new ScriptSegmenter();
            var segments = ss.Segment(code);

            var metaData = new List<SegmentResult>();

            var parser = new SyntaxParser();

            foreach(var region in segments)
            {
                var segment = code.Substring(region.Offset, region.Length);
                region.LineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                var parsedResult = parser.Parse(segment);

                if(parsedResult.TypeDeclarations.Any())
                {
                    metaData.Add(new SegmentResult
                        {
                            Segment = SegmentType.Class,
                            Region = region,
                            CodeSegment = segment
                        });
                }
                else if(parsedResult.MethodExpressions.Any() && segment.EndsWith("}"))
                {
                    var purgedSegment = segment.PurgeExcept(Environment.NewLine);

                    metaData.Add(new SegmentResult
                        {
                            Segment = SegmentType.Prototype,
                            Region = region,
                            CodeSegment = parsedResult.MethodPrototypes.FirstOrDefault() 
                                + purgedSegment.Trim()
                        });

                    var segmentBlockStart = segment.IndexOf("{");
                    var segmentBlockEnd = segment.LastIndexOf("}");
                    var segmentMethodBlock = segment.Substring(segmentBlockStart, segmentBlockEnd - segmentBlockStart + 1);

                    var method = parsedResult.MethodExpressions.FirstOrDefault();
                    var methodBlockStart = method.IndexOf("{");
                    var methodBlockEnd = method.LastIndexOf("}");
                    method = method.Remove(methodBlockStart, methodBlockEnd - methodBlockStart + 1);
                    method = method.Insert(methodBlockStart, segmentMethodBlock);

                    var purgeSigneture = segment.Substring(0, segmentBlockStart - 1).PurgeExcept(Environment.NewLine);
                    method = purgeSigneture.Trim() + method;

                    metaData.Add(new SegmentResult
                        {
                            Segment = SegmentType.Method,
                            Region = region,
                            CodeSegment = method
                        });
                }
                else
                {
                    metaData.Add(new SegmentResult
                        {
                            Segment = SegmentType.Evaluation,
                            Region = region,
                            CodeSegment = segment
                        });
                }
            }

            return  metaData
                    .OrderBy(x => x.Segment)
                    .ToList();
        }
    }
}