using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Nodes
{
    public class NumberNode : Node
    {
        public Token Token { get; set; }

        public NumberNode(Token token)
        {
            Token = token;

            PositionStart = Token.PositionStart;
            PositionEnd = Token.PositionEnd;
        }

        public override Node Clone() => new NumberNode(Token);
        public override string ToString() => $"{Token}";
        public override string ToCSharpCode() => $"new Number({Token.Value})";
    }
}
