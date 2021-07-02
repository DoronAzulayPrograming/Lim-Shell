namespace LimOnDotNetCore.Core.Nodes
{
    public class BreakNode : Node
    {
        public BreakNode(Position positionStart, Position positionEnd)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override Node Clone() =>
            new BreakNode(PositionStart, PositionEnd);
        public override string ToCSharpCode() => $"break;";
    }
}
