
namespace LimOnDotNetCore.Core.Nodes
{
    public class ContinueNode : Node
    {
        public ContinueNode(Position positionStart, Position positionEnd)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override Node Clone() =>
            new ContinueNode(PositionStart, PositionEnd);
        public override string ToCSharpCode() => $"continue;";
    }
}
