namespace ScriptCs.Engine.Mono.Tests.Parser.Lexer
{
    using System;

    using Should;

    using ScriptCs.Engine.Mono.Parser.Lexer;

    using Xunit;

    public class ScriptLexerTests
    {
        public class TheLexer
        {
            [Fact]
            public void ShouldIdentifyIdentifiersAsToken()
            {
                const string Code = " id ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.Identifier);
                token.TokenValue.ShouldEqual("id");
            }

            [Fact]
            public void ShouldIdentifySemiColonAsToken()
            {
                const string Code = " ; ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.SemiColon);
            }

            [Fact]
            public void ShouldIdentifyLeftBracketAsToken()
            {
                const string Code = " { ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.LeftBracket);
            }

            [Fact]
            public void ShouldIdentifyRightBracketAsToken()
            {
                const string Code = " } ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.RightBracket);
            }

            [Fact]
            public void ShouldIdentifyLeftParentheseAsToken()
            {
                const string Code = " ( ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.LeftParenthese);
            }

            [Fact]
            public void ShouldIdentifyRightParentheseAsToken()
            {
                const string Code = " ) ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.RightParenthese);
            }

            [Fact]
            public void ShouldIdentifyStringsAsToken()
            {
                const string Code = "\"This is a string\"";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.String);
                token.Start.ShouldEqual(0);
                token.End.ShouldEqual(18);
                token.TokenValue.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldIdentifyEmptyStrings()
            {
                const string Code = "\"\"";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.String);
                token.Start.ShouldEqual(0);
                token.End.ShouldEqual(2);
                token.TokenValue.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldIdentifyStringsWithEscapeChars()
            {
                const string Code = "\"AssemblyInformationalVersion(\\\"\"";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.String);
                token.TokenValue.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldIdenityCharactersAsToken()
            {
                const string Code = "\'A\'";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.Character);
                token.Start.ShouldEqual(0);
                token.End.ShouldEqual(3);
                token.TokenValue.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldNotFailOnIdentifyingCharactersAsToken()
            {
                const string Code = "\'A";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.Character);
                token.Start.ShouldEqual(0);
                token.End.ShouldEqual(2);
                token.TokenValue.ShouldEqual(Code);
            }

            [Fact]
            public void ShouldIdentifyUnlexedAsThemselves()
            {
                const string Code = " < ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual('<');
            }

            [Fact]
            public void ShouldRemoveSingleLineComments()
            {
                const string Code = " { // This is removed \n } ";

                var lexer = new ScriptLexer(Code);

                var token = lexer.GetToken();   
                token.Token.ShouldEqual('{');

                token = lexer.GetToken();   
                token.Token.ShouldEqual('}');

                token = lexer.GetToken();   
                token.Token.ShouldEqual(Token.Eof);
            }

            [Fact]
            public void ShouldRemoveMultiLineComments()
            {
                const string Code = " { /* This is \n removed */ } ";

                var lexer = new ScriptLexer(Code);

                var token = lexer.GetToken();   
                token.Token.ShouldEqual('{');

                token = lexer.GetToken();   
                token.Token.ShouldEqual('}');

                token = lexer.GetToken();   
                token.Token.ShouldEqual(Token.Eof);
            }

            [Fact]
            public void ShouldReturnTokenRegion()
            {
                const string Code = " id<T> ";

                var lexer = new ScriptLexer(Code);
                var result = lexer.GetToken();   

                result.Start.ShouldEqual(1);
                result.End.ShouldEqual(3);

                result = lexer.GetToken();   //<
                result.Start.ShouldEqual(3);
                result.End.ShouldEqual(4);
            }

            [Fact]
            public void ShouldIdentifyIDoAsToken()
            {
                const string Code = "do { } while(true); ";

                var lexer = new ScriptLexer(Code);
                var token = lexer.GetToken();   

                token.Token.ShouldEqual(Token.Do);
                token.TokenValue.ShouldEqual("do");
            }
        }
    }
}
