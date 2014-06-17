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

            while(_curLexResult.Code != Token.Eof)
            {
                GetNextToken();

                if(_curLexResult.Code == Token.SemiColon)
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
                    if(!SkipScope(Token.LeftParenthese, Token.RightParenthese))
                    {
                        return RegionResult.Incomplete();
                    }

                    continue;
                }

                // if block, return block region
                if(_curLexResult.Code == Token.LeftBracket)
                {
                    if(SkipScope(Token.LeftBracket, Token.RightBracket))
                    {
                        return new RegionResult
                        {
                            Offset = start,
                            Length = _curLexResult.End - start
                        };
                    }
                    else
                    {
                        return RegionResult.Incomplete();
                    }
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

        private Tuple<bool, int> GetMethodSignatureOffset()
        {
            if(_history.Count <= 2)
            {
                return new Tuple<bool, int>(false, -1);
            }

            var current = _history.Pop();
            var methodName = _history.Pop();
            var methodResultType = _history.Pop();

            if(methodResultType.Code != Token.Identifier && methodName.Code != Token.Identifier)
            {
                return new Tuple<bool, int>(false, -1);
            }

            var start = GetModifierOffset(methodResultType.Start, 3);

            _history.Push(methodResultType);
            _history.Push(methodName);
            _history.Push(current);

            return new Tuple<bool, int>(true, start);;
        }

        private int GetModifierOffset(int start, int depth)
        {
            var modifiers = new[] { "public", "private", "internal", "protected", "static", "async" };

            //check for modifiers
            Stack<LexerResult> restoreHistory = new Stack<LexerResult>();
            for(var i = 0; i < depth; i++)
            {
                if(_history.Count == 0)
                {
                    break;
                }

                var modifier = _history.Pop();
                restoreHistory.Push(modifier);
                if(modifier.Code != Token.Identifier || !modifiers.Contains(modifier.Identifier))
                {
                    break;
                }

                start = modifier.Start;
            }

            for(var i = 0; i < 3; i++)
            {
                if(restoreHistory.Count == 0)
                {
                    break;
                }

                _history.Push(restoreHistory.Pop());
            }

            return start;
        }
    }
}

