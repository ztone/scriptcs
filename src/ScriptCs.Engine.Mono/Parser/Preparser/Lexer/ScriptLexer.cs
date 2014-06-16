namespace ScriptCs.Engine.Mono.Preparser.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class ScriptLexer
    {
        private List<string> _blockKeywords;
        private int _lastChar = ' ';
        private string _identifier = string.Empty;
        private int _position;
        private StringReader _sr;

        public ScriptLexer(string code)
        {
            _position = -1; //inital pos

            _blockKeywords = new []{
                "if", "else", "while", "using", "foreach", "do"
            }.ToList();

            _sr = new StringReader(code);
        }

        /// <summary>
        /// Return the next token from script
        /// </summary>
        /// <returns>The next token</returns>
        public LexerResult GetToken()
        {
            _identifier = string.Empty;

            // skip any whitespace
            while(IsSpace((char)_lastChar))
            {
                _lastChar = Read();
            }

            // identifiers [a-zA-Z_][a-zA-Z0-9_]
            if(IsAlpha(_lastChar))
            {
                _identifier = string.Empty;
                _identifier += (char)_lastChar;
                _lastChar =  Read();
                while(IsAlphaNumeric(_lastChar))
                {
                    _identifier += (char)_lastChar;
                    _lastChar =  Read();
                }

                if(_blockKeywords.Contains(_identifier))
                {
                    return new LexerResult
                    {
                        Code = Token.Block,
                        Identifier = _identifier,
                        Start = StartPos(),
                        End = _position
                    };
                }

                if(_identifier.Equals("class", StringComparison.InvariantCulture))
                {
                    return new LexerResult
                    {
                        Code = Token.Class,
                        Identifier = _identifier,
                        Start = StartPos(),
                        End = _position
                    };
                }

                return new LexerResult
                {
                    Code = Token.Identifier,
                    Identifier = _identifier,
                    Start = StartPos(),
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
                    Code = Token.Eof,
                    Identifier = string.Empty,
                    Start = StartPos(),
                    End = _position
                };
            }

            int thisChar = _lastChar;
            _lastChar = Read();
            return new LexerResult
            {
                Code = thisChar,
                Identifier = string.Empty,
                Start = StartPos(),
                End = _position
            };
        }

        public static bool IsSpace(int token)
        {
            return token == Token.Space 
                || token == Token.NewLine 
                || token == Token.LineFeed 
                || token == Token.Tab;
        }

        private static bool IsAlpha(int token)
        {
            Regex rg = new Regex(@"^[a-zA-Z_@]*$");
            return rg.IsMatch(((char)token).ToString());
        }

        private static bool IsAlphaNumeric(int token)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9_]*$");
            return rg.IsMatch(((char)token).ToString());
        }

        private int StartPos()
        {
            if(!string.IsNullOrWhiteSpace(_identifier))
            {
                return _position - _identifier.Length;
            }

            return _position - 1;
        }

        private int Read()
        {
            _position += 1;
            return _sr.Read();
        }
    }
}

