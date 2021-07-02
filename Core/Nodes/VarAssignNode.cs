using LimOnDotNetCore.Extentions;

namespace LimOnDotNetCore.Core.Nodes
{
    public class VarAssignNode : Node
    {
        public Node ValueNode { get; set; }
        public Token VarNameToken { get; set; }
        public bool InitMode { get; set; }

        public VarAssignNode(Token varNameToken, Node node, bool initMode = true)
        {
            InitMode = initMode;
            ValueNode = node;
            VarNameToken = varNameToken;

            PositionStart = VarNameToken.PositionStart;
            PositionEnd = ValueNode.PositionEnd;
        }

        public override Node Clone() => new VarAssignNode(VarNameToken, ValueNode, InitMode);
        public override string ToString() => $"({VarNameToken}, {ValueNode})";
        public override string ToCSharpCode()
        {
            string code;
            if (typeof(FuncDefNode).IsInstanceOfType(ValueNode))
            {
                var func = ValueNode as FuncDefNode;
                code = ValueNode.ToCSharpCode();
                if (InitMode)
                {
                    code += $"Value {VarNameToken.Value} = {func.VarNameToken.Value};".CreateLimTag("vardef");
                }
                else
                {
                    code += $"{VarNameToken.Value} = {func.VarNameToken.Value};";
                }
            }
            else
            {
                if (InitMode)
                {
                    code = $"{(InitMode ? "Value " : "")}{VarNameToken.Value} = {ValueNode.ToCSharpCode().RemoveLimTags().RemoveComma()};".CreateLimTag("vardef");
                }
                else
                {
                    code = $"{VarNameToken.Value} = {ValueNode.ToCSharpCode().RemoveLimTags().RemoveComma()};";
                }
            }
            return code;
        }
    }
}
