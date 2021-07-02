using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Translating;
using LimOnDotNetCore.Core.Values;

namespace LimOnDotNetCore.Core
{
    public class Basic
    {

        public static Value Run(string fileName,string fileText, out IError error)
        {
            // Generate tokens
            Lexer lexer = new Lexer(fileName, fileText);
            var tokens = lexer.MakeTokens(out error);

            if (error != null) return null;

            // Generate AST
            Parser parser = new Parser(tokens);
            var ast = parser.Parse();

            if (ast.Error != null) { error = ast.Error; return null; }

            var cs_code = ast.Node.ToCSharpCode();
            Processor processor = new Processor();
            CsProject project = processor.Proccess(cs_code, "LimFramework");

            // Run program
            Context context = new Context("<program>");
            context.SymbolTable = Program.GlobalSymbolTable;

            Interpeter interpeter = new Interpeter();
            var result = interpeter.Visit(ast.Node, context);

            error = result.Error;
            return result.Value;
        }
    }
}
