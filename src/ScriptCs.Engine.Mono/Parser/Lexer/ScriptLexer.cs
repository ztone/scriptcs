namespace ScriptCs.Engine.Mono.Parser.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class ScriptLexer
    {
        private int _lastChar = ' ';
        private int _position;
        private StringReader _sr;

        public ScriptLexer(string code)
        {
            _position = -1; //inital pos

            _sr = new StringReader(code);
        }

        /// <summary>
        /// Return the next token from script
        /// </summary>
        /// <returns>The next token</returns>
        public LexerResult GetToken()
        {
            // skip any whitespace
            while(IsSpace((char)_lastChar))
            {
                _lastChar = Read();
            }

            if(_lastChar == Token.Quote)
            {
                var @string = string.Empty;
                @string += (char)_lastChar;

                int previous;
                do {
                    previous = _lastChar;
                    _lastChar = Read();
                    @string += (char)_lastChar;
                } while(!(_lastChar == Token.Quote && previous != Token.EscapeChar) 
                    && _lastChar != Token.Eof);

                _lastChar = Read(); //eat

                return new LexerResult
                {
                    Token = Token.String,
                    TokenValue = @string,
                    Start = _position - @string.Length,
                    End = _position
                };
            }

            if(_lastChar == Token.SingleQuote)
            {
                string @char = string.Empty;
                @char += (char)_lastChar;

                _lastChar = Read();
                int count = 0;
                while(count < 2 && _lastChar != Token.Eof)
                {
                    count++;
                    @char += (char)_lastChar;
                    _lastChar = Read();
                }

                return new LexerResult
                {
                    Token = Token.Character,
                    TokenValue = @char,
                    Start = _position - @char.Length,
                    End = _position
                };
            }

            // identifiers [a-zA-Z_][a-zA-Z0-9_]
            if(IsAlphaNumeric(_lastChar))
            {
                var identifier = string.Empty;
                identifier += (char)_lastChar;
                _lastChar =  Read();
                while(IsAlphaNumeric(_lastChar))
                {
                    identifier += (char)_lastChar;
                    _lastChar =  Read();
                }

                return new LexerResult
                {
                    Token = (identifier.Equals("do")) ? Token.Do : Token.Identifier,
                    TokenValue = identifier,
                    Start = _position - identifier.Length,
                    End = _position
                };
            }

            // single line comment
            if(_lastChar == Token.ForwardSlash && _sr.Peek() == Token.ForwardSlash)
            {
                do 
                {
                    _lastChar =  Read();
                } while (_lastChar != Token.Eof 
                    && _lastChar != Token.NewLine && _lastChar != Token.LineFeed);

                if(_lastChar != Token.Eof)
                {
                    return GetToken();
                }
            }

            // multi line comment
            if(_lastChar == Token.ForwardSlash && _sr.Peek() == Token.Star)
            {
                _lastChar = Read(); //eat
                _lastChar = Read(); //eat
                int nextChar;
                do 
                {
                    _lastChar =  Read();
                    nextChar = _sr.Peek();
                } while (_lastChar != Token.Eof 
                    && (_lastChar != Token.Star || nextChar != Token.ForwardSlash));

                if(_lastChar != Token.Eof)
                {
                    _lastChar = Read(); //eat
                    _lastChar = Read(); //eat
                    return GetToken();
                }
            }

            if(_lastChar == Token.Eof)
            {
                return new LexerResult
                {
                    Token = Token.Eof,
                    TokenValue = string.Empty,
                    Start = _position - 1,
                    End = _position
                };
            }

            int thisChar = _lastChar;
            _lastChar = Read();
            return new LexerResult
            {
                Token = thisChar,
                TokenValue = string.Empty,
                Start = _position - 1,
                End = _position
            };
        }

        private int Read()
        {
            _position += 1;
            return _sr.Read();
        }

        public static bool IsSpace(int token)
        {
            return token == Token.Space 
                || token == Token.NewLine 
                || token == Token.LineFeed 
                || token == Token.Tab;
        }

        private static bool IsAlphaNumeric(int token)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9_]*$");
            return rg.IsMatch(((char)token).ToString());
        }
    }
}

