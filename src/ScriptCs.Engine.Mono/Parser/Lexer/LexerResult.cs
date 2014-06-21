namespace ScriptCs.Engine.Mono.Parser.Lexer
{
    public class LexerResult
    {
        public int Token { get; set; }
        public string TokenValue { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}

