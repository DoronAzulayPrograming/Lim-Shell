using LimOnDotNetCore.Core.Enums;
using System.Collections.Generic;

namespace LimOnDotNetCore.Core
{
    public class TokenList : List<Token>
    {
        public TokenList()
        {
            
        }
        public TokenList(IEnumerable<Token> tokens)
        {
            AddRange(tokens);
        }

        public override string ToString()
        {
            var result = "[";
            for (int i = 0; i < Count - 1; i++)
                result += $"{this[i]}, ";
            result += $"{this[Count - 1]}]";
            
            return result;
        }
    }
    public class Token
    {
        public Position PositionStart { get; set; }
        public Position PositionEnd { get; set; }
        public TokenType Type { get; set; }
        public string Value { get; set; }

        public Token(TokenType type, Position positionStart = null, Position positionEnd = null)
        {
            Type = type;
            Value = "";

            if (positionStart != null)
            {
                PositionStart = positionStart.Clone();
                PositionEnd = positionStart.Clone();
                PositionEnd.Advance();
            }

            if (positionEnd != null)
                PositionEnd = positionEnd.Clone();

        }
        public Token(TokenType type, string value = "", Position positionStart = null, Position positionEnd = null)
        {
            Type = type;
            Value = value;

            if(positionStart != null)
            {
                PositionStart = positionStart.Clone();
                PositionEnd = positionStart.Clone();
                PositionEnd.Advance();
            }

            if(positionEnd != null) 
                PositionEnd = positionEnd.Clone();

        }

        public bool Matches(TokenType type, string value) => Type == type && Value == value;
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value)) return $"{Type}";
            if(Type == TokenType.String)
                return $"{Type}:\"{Value}\"";
            else
                return $"{Type}:{Value}";
        }
    }
}
