namespace ScriptCs.Engine.Mono.Preparser.Lexer
{
    using System;

    public class LexerResult
    {
        public int Code { get; set; }
        public string Identifier { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}

