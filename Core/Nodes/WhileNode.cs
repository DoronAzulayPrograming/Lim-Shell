
namespace LimOnDotNetCore.Core.Nodes
{
    public class WhileNode : Node
    {
        public Node ConditionNode;
        public Node BodyNode;

        public bool ShouldReturnNull { get; set; }

        public WhileNode(Node conditionNode, Node bodyNode, bool shouldReturnNull)
        {
            ConditionNode = conditionNode;
            BodyNode = bodyNode;
            ShouldReturnNull = shouldReturnNull;

            PositionStart = ConditionNode.PositionStart;
            PositionEnd = BodyNode.PositionEnd;
        }

        public override Node Clone() => new WhileNode(ConditionNode, BodyNode, ShouldReturnNull);
    }
}
