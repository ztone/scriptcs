namespace ScriptCs.Engine.Mono.Preparser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ScriptCs.Engine.Mono.Preparser.Lexer;

    public class ScriptParser
    {
        private ScriptLexer _lexer;
        private LexerResult _lexResult;
        private Stack<LexerResult> _history = new Stack<LexerResult>();

        public int TokenCount
        {
            get { return _history.Count; }
        }


        public ParserResult Parse (string code)
        {
            _lexer = new ScriptLexer(code);
            GetNextToken();
            return MainLoop(code);
        }

        private LexerResult GetNextToken()
        {
            _lexResult = _lexer.GetToken();
            _history.Push(_lexResult);
            return _lexResult;
        }

        private ParserResult MainLoop(string code)
        {
            var _result = new ParserResult();
            while(true)
            {
                RegionResult region;
                switch(_lexResult.Code)
                {
                case Token.Eof: return _result;
                case Token.Class:
                    region = ParseBlock();
                    if(region.IsValid)
                    {
                        _result.ClassRegions.Add(region);
                        _result.Classes.Add( (!region.IsIncomplete)
                            ? new Tuple<bool, string>(false, code.Substring(region.Offset, region.Length))
                            : new Tuple<bool, string>(true, string.Empty));
                    }
                    GetNextToken();
                    break;
                case Token.Block: 
                    ParseBlock(); 
                    GetNextToken();
                    break;
                case Token.LeftParenthese:
                    region = ParseMethod();
                    if(region.IsValid)
                    {
                        _result.MethodRegions.Add(region);
                        _result.Methods.Add( (!region.IsIncomplete)
                            ? new Tuple<bool, string>(false, code.Substring(region.Offset, region.Length))
                            : new Tuple<bool, string>(true, string.Empty));
                    }
                    GetNextToken();
                    break;
                default: 
                    GetNextToken();
                    break;
                }
            }
        }

        private RegionResult ParseBlock()
        {
            var current = _history.Pop();
            var start = GetModifierOffset(_lexResult.Start, 2);
            _history.Push(current);

            while(_lexResult.Code != Token.Eof)
            {
                GetNextToken();

                if(_lexResult.Code == Token.SemiColon)
                {
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _lexResult.End - start
                    };
                }

                if(_lexResult.Code == Token.LeftBracket)
                {
                    if(SkipScope(Token.LeftBracket, Token.RightBracket))
                    {
                        return new RegionResult
                        {
                            Offset = start,
                            Length = _lexResult.End - start
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

        private RegionResult ParseMethod()
        {
            var start = GetMethodSignatureOffset();
            if(start.Item1)
            {
                if(!SkipScope(Token.LeftParenthese, Token.RightParenthese))
                {
                    return RegionResult.Incomplete();
                }

                GetNextToken();

                if(_lexResult.Code == Token.LeftBracket)
                {
                    if(SkipScope(Token.LeftBracket, Token.RightBracket))
                    {
                        return new RegionResult
                        {
                            Offset = start.Item2,
                            Length = _lexResult.End - start.Item2
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
            if(_lexResult.Code != leftToken)
            {
                throw new ArgumentException("Invalid use of SkipBlock method, current token should equal left token parameter");
            }

            Stack<int> _scope = new Stack<int>();
            _scope.Push(1);

            while(_lexResult.Code != Token.Eof)
            {
                GetNextToken();

                if(_lexResult.Code == leftToken)
                {
                    _scope.Push(1);
                }

                if(_lexResult.Code == rightToken)
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
    }}

