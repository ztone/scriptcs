using System;

namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser.Preparser.Lexer;

    public class ScriptSegmenter
    {
        private ScriptLexer _lexer;
        private LexerResult _curLexResult;
        private Stack<LexerResult> _history = new Stack<LexerResult>();

        public int TokenCount
        {
            get { return _history.Count; }
        }


        public ParserResult Segment (string code)
        {
            _lexer = new ScriptLexer(code);
            GetNextToken();
            return MainLoop(code);
        }

        private LexerResult GetNextToken()
        {
            _curLexResult = _lexer.GetToken();
            _history.Push(_curLexResult);
            return _curLexResult;
        }

        private ParserResult MainLoop(string code)
        {
            var _result = new ParserResult();
            while(true)
            {
                RegionResult region;
                switch(_curLexResult.Code)
                {
                case Token.Eof: return _result;
                case Token.LeftBracket:
                case Token.LeftParenthese:
                case Token.Class:
                case Token.Block:
                case Token.Identifier:
                    region = ParseStatement(); 
                    _result.Segments.Add(region);
                    GetNextToken();
                    break;
                default: 
                    GetNextToken();
                    break;
                }
            }
        }

        private RegionResult ParseStatement()
        {
            var start = _curLexResult.Start;

            //special case, first token is Left curly bracket.
            bool block = _curLexResult.Code == Token.LeftBracket;
        
            while(_curLexResult.Code != Token.Eof)
            {
                GetNextToken();

                if( (!block && _curLexResult.Code == Token.SemiColon)
                    || (block && _curLexResult.Code == Token.RightParenthese)
                    || _curLexResult.Code == Token.Eof)
                {
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _curLexResult.End - start
                    };
                }

                // skip all in parenthese
                if(_curLexResult.Code == Token.LeftParenthese)
                {
                    SkipScope(Token.LeftParenthese, Token.RightParenthese);
                    if(_curLexResult.Code == Token.Eof)
                    {
                        return new RegionResult
                        {
                            Offset = start,
                            Length = _curLexResult.End - start
                        };
                    }

                    continue;
                }

                // if block, return block region
                if(_curLexResult.Code == Token.LeftBracket)
                {
                    SkipScope(Token.LeftBracket, Token.RightBracket);

                    return new RegionResult
                    {
                        Offset = start,
                        Length = _curLexResult.End - start
                    };

                }
            }
            return RegionResult.Invalid();
        }

        private bool SkipScope(int leftToken, int rightToken)
        {
            if(_curLexResult.Code != leftToken)
            {
                throw new ArgumentException("Invalid use of SkipBlock method, current token should equal left token parameter");
            }

            Stack<int> _scope = new Stack<int>();
            _scope.Push(1);

            while(_curLexResult.Code != Token.Eof)
            {
                GetNextToken();

                if(_curLexResult.Code == leftToken)
                {
                    _scope.Push(1);
                }

                if(_curLexResult.Code == rightToken)
                {
                    _scope.Pop();
                }

                if(_scope.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

