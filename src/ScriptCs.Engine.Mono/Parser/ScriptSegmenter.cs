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

            var rewriter = new CodeRewriter();
            var result = new List<SegmentResult>();
            var parser = new RegionParser();
            foreach(var region in parser.Parse(code))
            {
                // add the line row to region
                region.LineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                var segment = code.Substring(region.Offset, region.Length);

                if(rewriter.IsClass(segment))
                {
                    result.Add(new SegmentResult
                        {
                            Type = SegmentType.Class,
                            Region = region,
                            Code = segment
                        });
                }
                else if(rewriter.IsMethod(segment))
                {
                    var method = rewriter.RewriteMethod(segment);

                    result.Add(new SegmentResult
                        {
                            Type = SegmentType.Prototype,
                            Region = region,
                            Code = method.Item1
                        });

                    result.Add(new SegmentResult
                        {
                            Type = SegmentType.Method,
                            Region = region,
                            Code = method.Item2
                        });
                }
                else
                {
                    result.Add(new SegmentResult
                        {
                            Type = SegmentType.Evaluation,
                            Region = region,
                            Code = segment
                        });
                }
            }

            return  result
                    .OrderBy(x => x.Type)
                    .ToList();
        }
    }
}