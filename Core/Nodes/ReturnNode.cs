using LimOnDotNetCore.Extentions;

namespace LimOnDotNetCore.Core.Nodes
{
    public class ReturnNode : Node
    {
        public Node Node { get; set; }

        public ReturnNode(Node node, Position positionStart, Position positionEnd)
        {
            Node = node;

            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override Node Clone() =>
            new ReturnNode(Node, PositionStart, PositionEnd);

        public override string ToCSharpCode()
        {
            return $"return {Node.ToCSharpCode().RemoveComma()};";
        }
    }
}
