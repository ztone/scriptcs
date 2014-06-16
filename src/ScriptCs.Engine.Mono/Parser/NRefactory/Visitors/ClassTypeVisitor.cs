﻿namespace ScriptCs.Engine.Mono.Parser.NRefactory.Visitors
{
    using System.Collections.Generic;

    using ICSharpCode.NRefactory.CSharp;

    internal class ClassTypeVisitor : DepthFirstAstVisitor
    {
        private readonly List<TypeDeclaration> _classes;

        internal ClassTypeVisitor()
        {
            _classes = new List<TypeDeclaration>();
        }

        internal IList<TypeDeclaration> GetClassDeclarations()
        {
            return _classes;
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            _classes.Add(typeDeclaration);
            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}
