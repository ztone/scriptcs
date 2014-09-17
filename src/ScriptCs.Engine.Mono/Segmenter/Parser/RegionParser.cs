using System;
using System.Collections.Generic;

using ScriptCs.Engine.Mono.Segmenter.Lexer;

namespace ScriptCs.Engine.Mono.Segmenter.Parser
{
    public class RegionParser
    {
        private ScriptLexer _lexer;
        private LexerResult _current;

        public List<RegionResult> Parse(string code)
        {
            _lexer = new ScriptLexer(code);
            _current = _lexer.GetToken();
            return GetRegionBlocks();
        }

        private List<RegionResult> GetRegionBlocks()
        {
            var result = new List<RegionResult>();
            while (true)
            {
                RegionResult region;
                switch (_current.Token)
                {
                    case Token.Eof: return result;
                    case Token.Do: // do-while has two blocks
                        region = ParseBlock();
                        _current = _lexer.GetToken();
                        if (_current.TokenValue.Equals("while"))
                        {
                            var doRegion = ParseBlock();
                            doRegion.Length = region.Length + doRegion.Length +
                                (doRegion.Offset - (region.Offset + region.Length));
                            doRegion.Offset = region.Offset;
                            result.Add(doRegion);
                            _current = _lexer.GetToken();
                            break;
                        }
                        result.Add(region);
                        break;
                    default:
                        region = ParseBlock();
                        result.Add(region);
                        _current = _lexer.GetToken();
                        break;
                }
            }
        }

        private RegionResult ParseBlock()
        {
            var start = _current.Start;

            // first token is Left curly bracket.
            bool block = _current.Token == Token.LeftBracket;

            while (_current.Token != Token.Eof)
            {
                _current = _lexer.GetToken();

                if ((!block && _current.Token == Token.SemiColon)
                    || (block && _current.Token == Token.RightParenthese)
                    || _current.Token == Token.Eof)
                {
                    return new RegionResult
                    {
                        Offset = start,
                        Length = _current.End - start
                    };
                }

                if (_current.Token == Token.LeftParenthese)
                {
                    var isComplete = SkipScope(Token.LeftParenthese, Token.RightParenthese);
                    if (_current.Token == Token.Eof)
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

                if (_current.Token == Token.LeftBracket)
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
            if (_current.Token != leftToken)
            {
                throw new ArgumentException("Invalid use of SkipBlock method, current token should equal left token parameter");
            }

            var scope = new Stack<int>();
            scope.Push(1);

            while (_current.Token != Token.Eof)
            {
                _current = _lexer.GetToken();

                if (_current.Token == leftToken)
                {
                    scope.Push(1);
                }

                if (_current.Token == rightToken)
                {
                    scope.Pop();
                }

                if (scope.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}