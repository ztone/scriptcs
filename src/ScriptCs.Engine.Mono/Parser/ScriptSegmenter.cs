namespace ScriptCs.Engine.Mono.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    public class ScriptSegmenter
    {
        public List<SegmentResult> Segment(string code)
        {
            const string ScriptPattern = @"#line 1.*?\n";
            var isScriptFile = Regex.IsMatch(code, ScriptPattern);
            if(isScriptFile)
            {
                // Remove debug line
                code = Regex.Replace(code, ScriptPattern, "");
            }

            var parser = new SyntaxParser();
            var result = new List<SegmentResult>();
            var regionSegmenter = new RegionSegmenter();
            foreach(var region in regionSegmenter.Segment(code))
            {
                var segment = code.Substring(region.Offset, region.Length);
                region.LineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                var parsedResult = parser.Parse(segment);

                if(parsedResult.TypeDeclarations.Any())
                {
                    result.Add(new SegmentResult
                        {
                            SegmentType = SegmentType.Class,
                            Region = region,
                            SegmentCode = segment
                        });
                }
                else if(parsedResult.MethodExpressions.Any() && segment.EndsWith("}"))
                {
                    var purgedSegment = segment.PurgeExcept(Environment.NewLine);

                    result.Add(new SegmentResult
                        {
                            SegmentType = SegmentType.Prototype,
                            Region = region,
                            SegmentCode = parsedResult.MethodPrototypes.FirstOrDefault() 
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

                    result.Add(new SegmentResult
                        {
                            SegmentType = SegmentType.Method,
                            Region = region,
                            SegmentCode = method
                        });
                }
                else
                {
                    result.Add(new SegmentResult
                        {
                            SegmentType = SegmentType.Evaluation,
                            Region = region,
                            SegmentCode = segment
                        });
                }
            }

            return  result
                    .OrderBy(x => x.SegmentType)
                    .ToList();
        }
    }
}