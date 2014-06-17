namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    public class CodeSegementer
    {
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

            var classes = new List<string>();
            var proto = new List<string>();
            var methods = new List<string>();
            var evals = new List<string>();

            var parser = new SyntaxParser();
            foreach(var region in segments.Segments)
            {
                var result = parser.Parse(code.Substring(region.Offset, region.Length));

                if(result.TypeDeclarations.Any())
                {
                    classes.AddRange(result.TypeDeclarations);
                }

                if(result.MethodExpressions.Any())
                {
                    proto.AddRange(result.MethodPrototypes);
                    methods.AddRange(result.MethodExpressions);
                }

                if(!string.IsNullOrWhiteSpace(result.Evaluations))
                {
                    evals.Add(result.Evaluations);
                }
            }

            var codeList = new List<string>();
            codeList.AddRange(classes);
            codeList.AddRange(proto);
            codeList.AddRange(methods);
            codeList.AddRange(evals);
            return codeList;
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