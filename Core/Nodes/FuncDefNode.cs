using LimOnDotNetCore.Extentions;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Nodes
{
    public class FuncDefNode : Node
    {
        public Token VarNameToken { get; set; }
        public IEnumerable<Token> ArgsTokens { get; set; }
        public Node BodyNode { get; set; }

        public bool ShouldAutoReturn { get; set; }

        public FuncDefNode(Token varNameToken, IEnumerable<Token> argsTokens, Node bodyNode, bool shouldAutoReturn)
        {
            VarNameToken = varNameToken;
            ArgsTokens = argsTokens;
            BodyNode = bodyNode;
            ShouldAutoReturn = shouldAutoReturn;

            if (VarNameToken != null)
                PositionStart = VarNameToken.PositionStart;
            else if (ArgsTokens.Count() > 0)
                PositionStart = ArgsTokens.First().PositionStart;
            else
                PositionStart = BodyNode.PositionStart;

            PositionEnd = BodyNode.PositionEnd;
        }
        public override Node Clone() => new FuncDefNode(VarNameToken, ArgsTokens, BodyNode, ShouldAutoReturn);
        public override string ToCSharpCode()
        {
            string bodyCode = BodyNode.ToCSharpCode(); 
            bool shouldReturn = bodyCode.Contains("return");

            if (ShouldAutoReturn)
                return $"Value {VarNameToken.Value} = new Function{ArgsTokens.Count()}(({string.Join(",", ArgsTokens.Select(a => $"{a.Value.RemoveComma(true)}"))})=>{bodyCode.RemoveLimTags().RemoveComma()});".CreateLimTag("funcdef");
            else
                if (shouldReturn)
                return $"Value {VarNameToken.Value} = new Function{ArgsTokens.Count()}(({string.Join(",", ArgsTokens.Select(a => $"{a.Value.RemoveComma(true)}"))})=>\n{{\n{bodyCode.RemoveLimTags()}\n}});".CreateLimTag("funcdef");
            else
                return $"Value {VarNameToken.Value} = new Function{ArgsTokens.Count()}(({string.Join(",", ArgsTokens.Select(a => $"{a.Value.RemoveComma(true)}"))})=>\n{{\n{bodyCode.RemoveLimTags()}\nreturn Number.Empty;\n}});".CreateLimTag("funcdef");
        }
    }
}
