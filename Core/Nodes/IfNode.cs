using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Nodes
{
    public class IfNode : Node
    {
        public Node ElseCase;

        public Dictionary<Node, KeyValuePair<Node, bool>> Cases { get; set; }


        public IfNode(Dictionary<Node, KeyValuePair<Node, bool>> cases, Node elseCase)
        {
            Cases = cases;
            ElseCase = elseCase;

            if(Cases.Count > 0)
            {
                PositionStart = Cases.First().Key.PositionStart;
                PositionEnd = elseCase != null ? elseCase.PositionEnd : Cases.Last().Value.Key.PositionEnd;
            }
        }
        public override Node Clone() => new IfNode(Cases, ElseCase);
        public override string ToString() => $"{Cases}";
        public override string ToCSharpCode()
        {
            if (Cases.Count == 0 && ElseCase == null) 
                return "";

            string code = "";
            var cases = Cases.ToArray();
            if (cases.Length > 0)
            {
                var firstCase = cases[0];
                code = $"if({firstCase.Key.ToCSharpCode().Replace(";","")}.IsTrue())\n{{\n{firstCase.Value.Key.ToCSharpCode()}\n}}\n";

                if (cases.Length > 1)
                {
                    for (int i = 1; i < cases.Length; i++)
                    {
                        var c = cases[i];
                        code += $"else if({c.Key.ToCSharpCode().Replace(";", "")}.IsTrue())\n{{\n{c.Value.Key.ToCSharpCode()}\n}}\n";
                    }
                }
            }

            if(ElseCase != null)
            {
                code += $"else\n{{\n{ElseCase.ToCSharpCode()}\n}}\n";
            }

            return code;
        }
    }
}
