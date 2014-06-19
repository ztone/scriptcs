namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    public enum CodeSegment
    {
        NotSet = 0,
        Class = 1,
        Prototype = 2,
        Method = 3,
        Evaluation = 4
    }

    public class CodeMetaData
    {
        public CodeSegment Segment { get; set; }
        public RegionResult Region { get; set; }
        public string CodeSegment { get ; set; }
    }

    public class CodeSegementer
    {
        public List<CodeMetaData> SegmentCode(string code)
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

            var metaData = new List<CodeMetaData>();

            var parser = new SyntaxParser();

            foreach(var region in segments.Segments)
            {
                var segment = code.Substring(region.Offset, region.Length);
                region.LineNr = code.Substring(0, region.Offset).Count(x => x.Equals('\n'));

                var parsedResult = parser.Parse(segment);

                if(parsedResult.TypeDeclarations.Any())
                {
                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Class,
                            Region = region,
                            CodeSegment = segment
                        });
                }
                else if(parsedResult.MethodExpressions.Any() && segment.EndsWith("}"))
                {
                    var purgedSegment = segment.PurgeExcept(Environment.NewLine);

                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Prototype,
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

                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Method,
                            Region = region,
                            CodeSegment = method
                        });
                }
                else
                {
                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Evaluation,
                            Region = region,
                            CodeSegment = segment
                        });
                }
            }

            return  metaData
                    .OrderBy(x => x.Segment)
                    .ToList();
        }

        public List<string> Segment(string code)
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

            var purgedCode = code.PurgeExcept(Environment.NewLine);

            var metaData = new List<CodeMetaData>();

            var parser = new SyntaxParser();

            foreach(var region in segments.Segments)
            {
                var segment = code.Substring(region.Offset, region.Length);

                var parsedResult = parser.Parse(segment);

                if(parsedResult.TypeDeclarations.Any())
                {
                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Class,
                            Region = region,
                            CodeSegment = string.Format("{0}{1}",
                                purgedCode.Substring(0, region.Offset),
                                segment)
                        });
                }
                else if(parsedResult.MethodExpressions.Any())
                {
                    var purgedSegment = segment.PurgeExcept(Environment.NewLine);

                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Prototype,
                            Region = region,
                            CodeSegment = string.Format("{0}{1}",
                                purgedCode.Substring(0, region.Offset-1),
                                parsedResult.MethodPrototypes.FirstOrDefault() 
                                + purgedSegment.Trim())
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

                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Method,
                            Region = region,
                            CodeSegment = string.Format("{0}{1}",
                                purgedCode.Substring(0, region.Offset-1),
                                method)
                        });
                }
                else
                {
                    metaData.Add(new CodeMetaData
                        {
                            Segment = CodeSegment.Evaluation,
                            Region = region,
                            CodeSegment = string.Format("{0}{1}",
                                purgedCode.Substring(0, region.Offset-1),
                                segment)
                        });
                }
            }

            return  metaData
                .OrderBy(x => x.Segment)
                .Select(x => x.CodeSegment)
                .ToList();
        }

        public List<string> Run(string code)
        {
            var parser = new ScriptParser();
            var parsedCode = parser.Parse(code);

            var doc = new ScriptDocument();

            var classes = doc.ExtractRegions(code, parsedCode.ClassRegions);
            var methods = doc.ExtractRegions(code, parsedCode.MethodRegions);

            var syntaxParser = new SyntaxParser();
            var expr = syntaxParser.Parse(methods);

            var prototype = expr.MethodPrototypes.Aggregate(string.Empty, (x,y) => x + y);
            var methodsExpr = expr.MethodExpressions.Aggregate(string.Empty, (x,y) => x + y);

            var eval = doc.PurgeRegions(code, parsedCode.ClassRegions.Concat(parsedCode.MethodRegions));

            return new List<string> 
            {
                classes,
                prototype,
                methodsExpr,
                eval
            };
        }
    }
}