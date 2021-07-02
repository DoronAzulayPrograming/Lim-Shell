namespace LimOnDotNetCore.Core.Nodes
{
    public class VarAccessNode : Node
    {
        public Token VarNameToken { get; set; }

        public VarAccessNode(Token varNameToken)
        {
            VarNameToken = varNameToken;

            PositionStart = VarNameToken.PositionStart;
            PositionEnd = VarNameToken.PositionEnd;
        }

        public override Node Clone() => new VarAccessNode(VarNameToken);
        public override string ToString() => $"{VarNameToken}";
        public override string ToCSharpCode() => $"{VarNameToken.Value}";
    }
}
