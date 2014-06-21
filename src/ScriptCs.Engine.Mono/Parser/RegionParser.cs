namespace ScriptCs.Engine.Mono.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser.Lexer;

    public class RegionParser
    {
        private ScriptLexer _lexer;
        private LexerResult _current;
        private Stack<LexerResult> _history = new Stack<LexerResult>();

        public int TokenCount
        {
            get { return _history.Count; }
        }

        public List<RegionResult> Parse(string code)
        {
            _lexer = new ScriptLexer(code);
            GetNextToken();
            return MainLoop(code);
        }

        private LexerResult GetNextToken()
        {
            _current = _lexer.GetToken();
            _history.Push(_current);
            return _current;
        }

        private List<RegionResult> MainLoop(string code)
        {
            var _result = new List<RegionResult>();
            while(true)
            {
                RegionResult region;
                switch(_current.Token)
                {
                case Token.Eof: return _result;
                case Token.Do: // do-while has two blocks
                    region = ParseBlock();
                    GetNextToken();
                    if(_current.TokenValue.Equals("while"))
                    {
                        var doRegion = ParseBlock();
                        doRegion.Length = region.Length + doRegion.Length + 
                            (doRegion.Offset - (region.Offset+region.Length));
                        doRegion.Offset = region.Offset;
                        _result.Add(doRegion);
                        GetNextToken();
                        break;
                    }
                    _result.Add(region);
                    break;
                default: 
                    region = ParseBlock(); 
                    _result.Add(region);
                    GetNextToken();
                    break;
                }
            }
        }

        private RegionResult ParseBlock()
        {
            var start = _current.Start;

            // first token is Left curly bracket.
            bool block = _current.Token == Token.LeftBracket;

            while(_current.Token != Token.Eof)
            {
                GetNextToken();

                if( (!block && _current.Token == Token.SemiColon)
                    || (block && _current.Token == Token.RightParenthese)
                    || _current.Token == Token.Eof)
                {
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _current.End - start
                    };
                }

                if(_current.Token == Token.LeftParenthese)
                {
                    var isComplete = SkipScope(Token.LeftParenthese, Token.RightParenthese);
                    if(_current.Token == Token.Eof)
                    {
                        return new RegionResult
                        {
                            Offset = start,
                            Length = _current.End - start,
                            IsCompleteBlock = isComplete
                        };
                    }

                    continue;
                }

                if(_current.Token == Token.LeftBracket)
                {
                    bool isComplete = SkipScope(Token.LeftBracket, Token.RightBracket);
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _current.End - start,
                        IsCompleteBlock = isComplete
                    };

                }
            }

            throw new InvalidOperationException(typeof(RegionParser).Name + "should never reach this point.");
        }

        private bool SkipScope(int leftToken, int rightToken)
        {
            if(_current.Token != leftToken)
            {
                throw new ArgumentException("Invalid use of SkipBlock method, current token should equal left token parameter");
            }

            Stack<int> _scope = new Stack<int>();
            _scope.Push(1);

            while(_current.Token != Token.Eof)
            {
                GetNextToken();

                if(_current.Token == leftToken)
                {
                    _scope.Push(1);
                }

                if(_current.Token == rightToken)
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