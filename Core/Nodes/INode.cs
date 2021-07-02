namespace LimOnDotNetCore.Core.Nodes
{
    public interface INode
    {
        public Position PositionStart { get; set; }
        public Position PositionEnd { get; set; }
        //public INode Clone();
        public string ToString();
    }

    public class Node : INode
    {
        public Position PositionStart { get; set; }
        public Position PositionEnd { get; set; }
        public virtual Node Clone() => default(Node);
        public virtual string ToCSharpCode() => "";
    }
}
