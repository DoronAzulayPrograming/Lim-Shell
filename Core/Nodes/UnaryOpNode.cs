namespace LimOnDotNetCore.Core.Nodes
{
    public class UnaryOpNode : Node
    {
        public Node Node { get; set; }
        public Token Token { get; set; }

        public UnaryOpNode(Token opToken, Node node)
        {
            Node = node;
            Token = opToken;

            PositionStart = Token.PositionStart;
            PositionEnd = Node.PositionStart;

        }
        public override Node Clone() => new UnaryOpNode(Token, Node);
        public override string ToString() => $"({Token}, {Node})";
        public override string ToCSharpCode() => $"{Node.ToCSharpCode().Replace("(","(-")}";
    }
}
