using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Nodes
{
    public class ListNode: Node
    {
        public bool WrapperMode { get; set; }
        public IEnumerable<Node> Elements { get; set; }

        /*public ListNode(IEnumerable<INode> elements)
        {
            Elements = elements;
        }*/
        public ListNode(IEnumerable<Node> elements, Position posStart, Position posEnd, bool wrapperMode = false)
        {
            Elements = elements;
            WrapperMode = wrapperMode;

            PositionStart = posStart;
            PositionEnd = posEnd;
        }

        public override Node Clone() => new ListNode(Elements, PositionStart, PositionEnd);

        public override string ToString() => $"[{string.Join(",", Elements.Select(e => $"{e}"))}]";
        public override string ToCSharpCode()
        {
            if (WrapperMode)
                return $"{string.Join("\n", Elements.Select(e => $"{e.ToCSharpCode()}"))}";
            else
                return $"new List({string.Join(",", Elements.Select(e => $"{e.ToCSharpCode()}"))})";
        }
    }
}
