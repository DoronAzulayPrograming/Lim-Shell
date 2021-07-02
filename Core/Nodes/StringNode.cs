namespace LimOnDotNetCore.Core.Nodes
{
    public class StringNode : Node
    {
        public Token Token { get; set; }

        public StringNode(Token token)
        {
            Token = token;

            PositionStart = Token.PositionStart;
            PositionEnd = Token.PositionEnd;
        }

        public override Node Clone() => new StringNode(Token);
        public override string ToString() => $"{Token}";
        public override string ToCSharpCode() => $"new String(\"{Token.Value}\")";
    }
}
