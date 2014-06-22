namespace ScriptCs.Engine.Mono.Parser.NRefactory
{
    using System;
    using System.Linq;

    using ICSharpCode.NRefactory.CSharp;

    using ScriptCs.Engine.Mono.Parser.NRefactory.Visitors;

    public class CodeRewriter
    {
        public bool IsClass(string code)
        {
            var visitor = new ClassTypeVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetClassDeclarations().Any();
        }

        public bool IsMethod(string code)
        {
            var @class = "class A { " + code + " } ";
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(@class);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            return visitor.GetMethodDeclarations().Any() && code.TrimEnd().EndsWith("}");
        }

        public Tuple<string, string> RewriteMethod(string code)
        {
            var @class = "class A { " + code + " } ";
            var visitor = new MethodVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(@class);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            var result = visitor.GetMethodDeclarations().FirstOrDefault();

            // find newlines in method signature to maintain linenumbers
            var newLines = code.Substring(0, code.IndexOf("{") - 1)
                .Where(x => x.Equals('\n'))
                .Aggregate(string.Empty, (a, c) => a + c);

            // use code methodblock to maintain linenumbers
            var codeBlock = code.Substring(code.IndexOf("{"), code.LastIndexOf("}") - code.IndexOf("{") + 1);
            var method = result.MethodExpression.GetText();
            var blockStart = method.IndexOf("{");
            var blockEnd = method.LastIndexOf("}");
            method = method.Remove(blockStart, blockEnd - blockStart + 1);
            method = method.Insert(blockStart, codeBlock);

            return new Tuple<string, string>(
                result.MethodPrototype.GetText().Trim()+newLines, 
                newLines+method.Trim());
        }
    }
}
