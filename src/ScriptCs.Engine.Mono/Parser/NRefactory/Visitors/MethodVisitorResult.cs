﻿namespace ScriptCs.Engine.Mono.Parser.NRefactory.Visitors
{
    using ICSharpCode.NRefactory.CSharp;

    public class MethodVisitorResult
    {
        public MethodDeclaration MethodDefinition { get; set; }
        public VariableDeclarationStatement MethodPrototype { get; set; }
        public ExpressionStatement MethodExpression { get; set; }
    }
}