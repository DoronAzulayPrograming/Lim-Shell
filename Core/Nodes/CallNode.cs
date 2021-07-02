using LimOnDotNetCore.Core.Values;
using LimOnDotNetCore.Extentions;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Nodes
{
    public class CallNode : Node
    {
        public Node NodeToCall { get; set; }
        public IEnumerable<Node> ArgsNodes { get; set; }

        public CallNode(Node nodeToCall, IEnumerable<Node> argsNodes)
        {
            NodeToCall = nodeToCall;
            ArgsNodes = argsNodes;

            PositionStart = nodeToCall.PositionStart;

            if (ArgsNodes.Count() > 0)
                PositionEnd = ArgsNodes.Last().PositionEnd;
            else
                PositionEnd = nodeToCall.PositionEnd;
        }
        public override Node Clone() => new CallNode(NodeToCall, ArgsNodes);

        public override string ToCSharpCode()
        {
            bool isBuiltIn = BuilInFunction.FunctionList.Contains(NodeToCall.ToCSharpCode());
            if (isBuiltIn)
            {
                return $"Buildin.{NodeToCall.ToCSharpCode()}({string.Join(",", ArgsNodes.Select(a => a.ToCSharpCode().RemoveComma()))});";
            }

            return $"{NodeToCall.ToCSharpCode()}.Execute({string.Join(",", ArgsNodes.Select(a => a.ToCSharpCode().RemoveComma()))});";
        }
    }
}
