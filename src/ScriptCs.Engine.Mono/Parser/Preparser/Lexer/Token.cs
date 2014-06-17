namespace ScriptCs.Engine.Mono.Parser.Preparser.Lexer
{
    using System;

    public static class Token {
        public const int Eof = -1;
        public const int Identifier = -2;
        public const int Block = -3;
        public const int Class = -4;
        public const int String = -5;
        public const int Character = -6;
        public const int LeftBracket = (int)'{';
        public const int RightBracket = (int)'}';
        public const int LeftParenthese = (int)'(';
        public const int RightParenthese = (int)')';
        public const int SemiColon = (int)';';
        public const int ForwardSlash = (int)'/';
        public const int EscapeChar = (int)'\\';
        public const int Star = (int)'*';
        public const int Space = (int)' ';
        public const int Tab = (int)'\t';
        public const int LineFeed = (int)'\r';
        public const int NewLine = (int)'\n';
        public const int Quote = (int)'"';
        public const int SingleQuote = (int)'\'';
    }
}

