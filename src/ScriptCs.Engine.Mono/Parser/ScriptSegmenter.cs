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
                // Calculate region linenumber
                var lineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                var segment = code.Substring(region.Offset, region.Length);

                if(rewriter.IsClass(segment))
                {
                    result.Add(new SegmentResult
                        {
                            Type = SegmentType.Class,
                            BeginLine = lineNr,
                            Code = segment
                        });
                }
                else 
                {
                    var isMethod = rewriter.IsMethod(segment);

                    // can't rewrite method, has error
                    if(isMethod.Item1 && isMethod.Item2 != null)
                    {
                        result.Add(new SegmentResult
                            {
                                Type = SegmentType.MethodError,
                                BeginLine = lineNr,
                                Code = segment,
                                ErrorMessages = isMethod.Item2
                            });
                    }
                    // method ok
                    else if(isMethod.Item1)
                    {
                        var method = rewriter.RewriteMethod(segment);

                        result.Add(new SegmentResult
                            {
                                Type = SegmentType.Prototype,
                                BeginLine = lineNr,
                                Code = method.Item1
                            });

                        result.Add(new SegmentResult
                            {
                                Type = SegmentType.Method,
                                BeginLine = lineNr,
                                Code = method.Item2
                            });
                    }
                    else
                    {
                        result.Add(new SegmentResult
                            {
                                Type = SegmentType.Evaluation,
                                BeginLine = lineNr,
                                Code = segment
                            });
                    }
                }
            }

            return  result
                    .OrderBy(x => x.Type)
                    .ToList();
        }
    }
}