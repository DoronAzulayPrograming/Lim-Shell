using LimOnDotNetCore.Core.Enums;
using LimOnDotNetCore.Core.Errors;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Translating
{
    public class Lexer
    {
        public string Text { get; set; }
        public Position Position { get; set; }
        public char CurrentChar { get; set; }
        public Lexer(string fileName, string fileText)
        {
            Text = fileText;
            Position = new Position(-1,0,-1, fileName, fileText);
            CurrentChar = Program.CharNone;
            Advance();
        }

        void Advance()
        {
            Position.Advance(CurrentChar);
            CurrentChar = Position.Index < Text.Length ? Text[Position.Index] : Program.CharNone;
        }

        public  IEnumerable<Token> MakeTokens(out IError error)
        {
            List<Token> tokens = new List<Token>();

            while (CurrentChar != Program.CharNone)
            {
                if (CurrentChar.Equals(' ') || CurrentChar.Equals('\t') || CurrentChar.Equals('\r'))
                    Advance();
                else if(CurrentChar.Equals('#'))
                    SkipComment();
                else if (CurrentChar.Equals(';') || CurrentChar.Equals('\n'))
                {
                    tokens.Add(new Token(TokenType.NewLine, Position));
                    Advance();
                }
                else if (Program.LETTERS.Contains(CurrentChar))
                {
                    tokens.Add(MakeIdentifier());
                }
                else if (Program.DIGITS.Contains(CurrentChar))
                {
                    tokens.Add(MakeNumber());
                }
                else if (CurrentChar == '"')
                {
                    var token = MakeString(out var inerError);
                    if (inerError != null)
                    {
                        error = inerError;
                        return new List<Token>();
                    }
                    tokens.Add(token);
                    Advance();
                }
                else if (CurrentChar == '+')
                {
                    tokens.Add(new Token(TokenType.Plus,"Add",Position));
                    Advance();
                }
                else if (CurrentChar == '-')
                {
                    tokens.Add(MakeMinusOrArrow());
                }
                else if (CurrentChar == '*')
                {
                    tokens.Add(new Token(TokenType.Mul, "Mul", Position));
                    Advance();
                }
                else if (CurrentChar == '/')
                {
                    tokens.Add(new Token(TokenType.Div, "Div", Position));
                    Advance();
                }
                else if (CurrentChar == '^')
                {
                    tokens.Add(new Token(TokenType.Pow, "Pow", Position));
                    Advance();
                }
                else if (CurrentChar == '(')
                {
                    tokens.Add(new Token(TokenType.Lparem, Position));
                    Advance();
                }
                else if (CurrentChar == ')')
                {
                    tokens.Add(new Token(TokenType.Rparem, Position));
                    Advance();
                }
                else if (CurrentChar == '[')
                {
                    tokens.Add(new Token(TokenType.LSQparem, Position));
                    Advance();
                }
                else if (CurrentChar == ']')
                {
                    tokens.Add(new Token(TokenType.RSQparem, Position));
                    Advance();
                }
                else if (CurrentChar == '!')
                {
                    var token = MakeNotEquels(out var inerError);
                    if(inerError != null)
                    {
                        error = inerError;
                        return new List<Token>();
                    }
                    tokens.Add(token);
                }
                else if (CurrentChar == '=')
                    tokens.Add(MakeEquels());
                else if (CurrentChar == '<')
                    tokens.Add(MakeLessThen());
                else if (CurrentChar == '>')
                    tokens.Add(MakeGreaterThen());
                else if (CurrentChar == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, Position));
                    Advance();
                }
                else
                {
                    var pos_start = Position.Clone();
                    char c = CurrentChar;
                    Advance();
                    error = new IllegalCharError(pos_start, Position,$"'{c}'");
                    return new List<Token>();
                }
            }
            error = null;
            tokens.Add(new Token(TokenType.Eof, Position));
            return tokens;
        }

        Token MakeNotEquels(out IError error)
        {
            error = null;
            var pos_start = Position.Clone();
            Advance();

            if(CurrentChar == '=')
            {
                Advance();
                return new Token(TokenType.NE, "GetComparisonNE", pos_start, Position);
            }
            Advance();
            error = new ExpectedCharError(pos_start, Position, "'=' (after '!')");
            return null;
        }
        Token MakeEquels()
        {
            var strVal = "=";
            var token_type = TokenType.EQ;
            var pos_start = Position.Clone();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                strVal = "GetComparisonEE";
                token_type = TokenType.EE;
            }
            return new Token(token_type, strVal, pos_start, Position);
        }
        Token MakeLessThen()
        {
            var strVal = "<";
            var token_type = TokenType.LT;
            var pos_start = Position.Clone();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                strVal = "<=";
                token_type = TokenType.LTE;
            }
            return new Token(token_type, strVal, pos_start, Position);
        }
        Token MakeGreaterThen()
        {
            var strVal = ">";
            var token_type = TokenType.GT;
            var pos_start = Position.Clone();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                strVal = ">=";
                token_type = TokenType.GTE;
            }
            return new Token(token_type, strVal, pos_start, Position);
        }
        Token MakeIdentifier()
        {
            var id_str = "";
            var pos_start = Position.Clone();

            while (CurrentChar != Program.CharNone && (Program.LETTERS_DIGITS + "_").Contains(CurrentChar))
            {
                id_str += CurrentChar;
                Advance();
            }

            var type = TokenType.Identifier;
            if (Program.KEYWORDS.Contains(id_str)) type = TokenType.Keyword;
            return new Token(type, id_str, pos_start, Position);
        }
        Token MakeNumber()
        {
            var num_str = "";
            int dot_count = 0;
            var pos_start = Position.Clone();

            while (CurrentChar != Program.CharNone && (Program.DIGITS + '.').Contains(CurrentChar))
            {
                if(CurrentChar == '.')
                {
                    if (dot_count == 1) break;
                    dot_count++;
                    num_str += '.';
                }
                else num_str += CurrentChar;

                Advance();
            }

            if (dot_count == 0)
                return new Token(TokenType.Int, num_str, pos_start, Position);

            return new Token(TokenType.Float, num_str, pos_start, Position);
        }

        Token MakeMinusOrArrow()
        {
            var strVal = "Sub";
            var token_type = TokenType.Minus;
            var pos_start = Position.Clone();
            Advance();

            if (CurrentChar == '>')
            {
                Advance();
                strVal = "->";
                token_type = TokenType.Arrow;
            }
            return new Token(token_type, strVal,pos_start, Position);
        }
        Token MakeString(out IError error)
        {
            var str = "";
            bool endStrFound = false;
            var pos_start = Position.Clone();
            var escape_character = false;
            Advance();

            var escape_characters = new Dictionary<char,char>();
            escape_characters.Add('n', '\n');
            escape_characters.Add('t', '\t');

            while (CurrentChar != Program.CharNone && (!(endStrFound = CurrentChar == '"') || escape_character))
            {
                if (escape_character)
                {
                    char temp = escape_characters.SingleOrDefault(c => c.Key.Equals(CurrentChar)).Value;
                    str += temp.Equals(default(char)) ? CurrentChar : temp;
                }
                else
                {
                    if (CurrentChar == '\\')
                        escape_character = true;
                    else
                        str += CurrentChar;
                }
                Advance();
                escape_character = false;
            }
            if (!endStrFound)
            {
                error = new ExpectedCharError(pos_start, Position, " end of string '\"'");
                return null;
            }
            error = null;
            return new Token(TokenType.String, str, pos_start, Position);
        }

        void SkipComment()
        {
            Advance();
            while (CurrentChar != '\n') Advance();
            Advance();
        }
    }
}
