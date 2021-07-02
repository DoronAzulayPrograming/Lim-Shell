using LimOnDotNetCore.Extentions;

namespace LimOnDotNetCore.Core.Nodes
{
    public class BinOpNode : Node
    {
        public Node Left { get; set; }
        public Token OpToken { get; set; }
        public Node Right { get; set; }

        public BinOpNode(Node left, Token opToken, Node right)
        {
            Left = left;
            OpToken = opToken;
            Right = right;

            PositionStart = Left.PositionStart;
            PositionEnd = Left.PositionStart;
        }
        public override Node Clone() => new BinOpNode(Left, OpToken, Right);
        public override string ToString() => $"({Left}, {OpToken}, {Right})";
        string GetOp(string op)
        {
            if(op.Equals("and"))
                return "And";
            else if (op.Equals("or"))
                return "Or";
            return op;
        }
        public override string ToCSharpCode() => $"{Left.ToCSharpCode().RemoveComma()}.{GetOp(OpToken.Value)}({Right.ToCSharpCode().RemoveComma()});";
    }
}
