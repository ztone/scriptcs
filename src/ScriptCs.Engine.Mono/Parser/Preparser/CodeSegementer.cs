namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System.Collections.Generic;
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    public class CodeSegementer
    {
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