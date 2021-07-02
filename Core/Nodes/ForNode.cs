using LimOnDotNetCore.Extentions;

namespace LimOnDotNetCore.Core.Nodes
{
    public class ForNode : Node
    {
        public Token VarNameToken { get; set; }
        public Node StartValueNode { get; set; }
        public Node EndValueNode { get; set; }
        public Node StepValueNode { get; set; }
        public Node BodyNode { get; set; }

        public bool ShouldReturnNull { get; set; }

        public ForNode(Token varNameToken, Node StartValue, Node endValueNode, Node stepValueNode, Node bodyNode, bool shouldReturnNull)
        {
            VarNameToken = varNameToken;
            StartValueNode = StartValue;
            EndValueNode = endValueNode;
            StepValueNode = stepValueNode;
            BodyNode = bodyNode;
            ShouldReturnNull = shouldReturnNull;

            PositionStart = VarNameToken.PositionStart;
            PositionEnd = BodyNode.PositionEnd;
        }
        public override Node Clone() => new ForNode(VarNameToken, StartValueNode, EndValueNode, StepValueNode, BodyNode, ShouldReturnNull);

        public override string ToCSharpCode()
        {
            string code = "";
            StepValueNode = new NumberNode(new Token(Enums.TokenType.Int, "1"));
            code += $"for(Value {VarNameToken.Value} = {StartValueNode.ToCSharpCode().RemoveComma()};{VarNameToken.Value}.GetComparisonLT({EndValueNode.ToCSharpCode().RemoveComma()}).IsTrue();{VarNameToken.Value} = {VarNameToken.Value}.Add({StepValueNode.ToCSharpCode().RemoveComma()}))\n{{\n{BodyNode.ToCSharpCode().RemoveLimTags()}\n}}\n";
            return code;
        }
    }

}
