namespace ScriptCs.Engine.Mono.Tests.Parser.NRefactory
{
    using System;
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser.NRefactory;

    using Should;

    using Xunit;

    public class CodeRewriterTests
    {
        public class AnalyseSegments
        {
            [Fact]
            public void ShouldReturnTrueIfIsClass()
            {
                const string Code = "class A { }";

                var rewriter = new CodeRewriter();
                rewriter.IsClass(Code).ShouldBeTrue();
            }

            [Fact]
            public void ShouldReturnFalseIfIsNotClass()
            {
                const string Code = "void Bar() { }";

                var rewriter = new CodeRewriter();
                rewriter.IsClass(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfIsNotMethod()
            {
                const string Code = "class A { } ";

                var rewriter = new CodeRewriter();
                rewriter.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfIncompeteMethod()
            {
                const string Code = "void Bar() { ";

                var rewriter = new CodeRewriter();
                rewriter.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnFalseIfMissingMethodBody()
            {
                const string Code = "void Bar()";

                var rewriter = new CodeRewriter();
                rewriter.IsMethod(Code).ShouldBeFalse();
            }

            [Fact]
            public void ShouldReturnTrueIfIsMethod()
            {
                const string Code = "void Bar() { }";

                var rewriter = new CodeRewriter();
                rewriter.IsMethod(Code).ShouldBeTrue();
            }
        }

        public class MethodRewrites
        {
            [Fact]
            public void ShouldRewriteToPrototypeAndExpression()
            {
                const string Code = "void Bar() { }";

                var rewriter = new CodeRewriter();
                var method = rewriter.RewriteMethod(Code);

                method.Item1.ShouldEqual("Action Bar;");
                method.Item2.ShouldEqual("Bar = delegate () { };");
            }

            [Fact]
            public void ShouldPreserveMethodBody()
            {
                const string Code = "int Foo(int a) { Foo();\n\nreturn a;\n}";

                var rewriter = new CodeRewriter();
                var method = rewriter.RewriteMethod(Code);

                method.Item1.ShouldEqual("Func<int, int> Foo;");
                method.Item2.ShouldEqual("Foo = delegate (int a) { Foo();\n\nreturn a;\n};");
            }

            [Fact]
            public void ShouldPreserveLineCountInMethodSignature()
            {
                const string Code = "int\nFoo\n(\n)\n { return 42; }";

                var rewriter = new CodeRewriter();
                var method = rewriter.RewriteMethod(Code);

                method.Item1.ShouldEqual("Func<int> Foo;\n\n\n\n");
                method.Item2.ShouldEqual("\n\n\n\nFoo = delegate () { return 42; };");
            } 
        }
    }
}