using LimOnDotNetCore.Core.Enums;
using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Nodes;
using LimOnDotNetCore.Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Translating
{
    public class TranspilerResult
    {
        public string Code { get; set; }
        public Value Value { get; set; }
        public IError Error { get; set; }
        public Value FunctionReturnValue { get; set; }
        public bool LoopShouldContinue { get; set; }
        public bool LoopShouldBreak { get; set; }

        public void Reset()
        {
            Code = null;
            Value = null;
            Error = null;
            FunctionReturnValue = null;
            LoopShouldContinue = false;
            LoopShouldBreak = false;
        }
        public Value Register(TranspilerResult result)
        {
            Error = result.Error;
            FunctionReturnValue = result.FunctionReturnValue;
            LoopShouldContinue = result.LoopShouldContinue;
            LoopShouldBreak = result.LoopShouldBreak;
            Code = result.Code;
            return result.Value;
        }
        public TranspilerResult Success(string code, Value value)
        {
            Reset();
            Code = code;
            Value = value;
            return this;
        }
        public TranspilerResult SuccessReturn(string code, Value value)
        {
            Reset();
            Code = code;
            FunctionReturnValue = value;
            return this;
        }
        public TranspilerResult SuccessContinue(string code)
        {
            Reset();
            Code = code;
            LoopShouldContinue = true;
            return this;
        }
        public TranspilerResult SuccessBreak(string code)
        {
            Reset();
            Code = code;
            LoopShouldBreak = true;
            return this;
        }
        public TranspilerResult Failure(IError error)
        {
            Reset();
            Error = error;
            return this;
        }

        public bool ShouldReturn() =>
            (Error != null || FunctionReturnValue != null);
    }

    public class CsTranspiler
    {
        public TranspilerResult Visit(Node node, Context context)
        {
            if (typeof(NumberNode).IsInstanceOfType(node))
                return Visit_NumberNode(node as NumberNode, context);

            else if (typeof(BinOpNode).IsInstanceOfType(node))
                return Visit_BinOpNode(node as BinOpNode, context);

            else if (typeof(UnaryOpNode).IsInstanceOfType(node))
                return Visit_UnaryOpNode(node as UnaryOpNode, context);

            else if (typeof(VarAccessNode).IsInstanceOfType(node))
                return Visit_VarAccessNode(node as VarAccessNode, context);

            else if (typeof(VarAssignNode).IsInstanceOfType(node))
                return Visit_VarAssignNode(node as VarAssignNode, context);

            else if (typeof(IfNode).IsInstanceOfType(node))
                return Visit_IfNode(node as IfNode, context);

            else if (typeof(ForNode).IsInstanceOfType(node))
                return Visit_ForNode(node as ForNode, context);

            else if (typeof(WhileNode).IsInstanceOfType(node))
                return Visit_WhileNode(node as WhileNode, context);

            else if (typeof(FuncDefNode).IsInstanceOfType(node))
                return Visit_FunctionDefNode(node as FuncDefNode, context);

            else if (typeof(CallNode).IsInstanceOfType(node))
                return Visit_CallNode(node as CallNode, context);

            else if (typeof(StringNode).IsInstanceOfType(node))
                return Visit_StringNode(node as StringNode, context);

            else if (typeof(ListNode).IsInstanceOfType(node))
                return Visit_ListNode(node as ListNode, context);

            else if (typeof(ReturnNode).IsInstanceOfType(node))
                return Visit_ReturnNode(node as ReturnNode, context);

            else if (typeof(ContinueNode).IsInstanceOfType(node))
                return Visit_ContinueNode(node as ContinueNode, context);

            else if (typeof(BreakNode).IsInstanceOfType(node))
                return Visit_BreakNode(node as BreakNode, context);

            else return new TranspilerResult().Success("Exeption non visit method difiend", Number.Null);
        }

        public TranspilerResult Visit_NumberNode(NumberNode node, Context context)
        {
            var res = new TranspilerResult();
            Value number = null;
            number = new Number(node.Token.Value).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd);
            return res.Success($"new Number({node.Token.Value})", number);
        }
        public TranspilerResult Visit_VarAccessNode(VarAccessNode node, Context context)
        {
            var res = new TranspilerResult();
            var var_name = node.VarNameToken.Value;
            var value = context.SymbolTable.Get(var_name);

            if (value == null)
                return res.Failure(new RunTimeError(
                    node.PositionStart, node.PositionEnd,
                    $"{var_name} is not defined", context));

            return res.Success(var_name, value.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_VarAssignNode(VarAssignNode node, Context context)
        {
            var res = new TranspilerResult();
            var var_name = node.VarNameToken.Value;
            var value = res.Register(Visit(node.ValueNode, context));
            if (res.ShouldReturn()) return res;

            context.SymbolTable.Set(var_name, value);
            var code = "";
            if (value.Type().StartsWith("List<"))
                code = $"{(node.InitMode ? value.Type() : "")} {var_name} = new {value.Type()}(){{{res.Code.Remove(0, 1).Remove(res.Code.Length - 2, 1)}}};\n";
            else
                code = $"{(node.InitMode ? "Value" : "")} {var_name} = {res.Code};\n";
            return res.Success(code, value.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_BinOpNode(BinOpNode node, Context context)
        {
            var res = new TranspilerResult();
            var left = res.Register(Visit(node.Left, context));
            var left_code = res.Code;
            if (res.ShouldReturn()) return res;
            var right = res.Register(Visit(node.Right, context));
            var right_code = res.Code;
            if (res.ShouldReturn()) return res;

            IError error = null;
            Value number = null;
            if (node.OpToken.Type == TokenType.Plus)
                number = left.Add(right, out error);
            else if (node.OpToken.Type == TokenType.Minus)
                number = left.Sub(right, out error);
            else if (node.OpToken.Type == TokenType.Mul)
                number = left.Mul(right, out error);
            else if (node.OpToken.Type == TokenType.Div)
                number = left.Div(right, out error);
            else if (node.OpToken.Type == TokenType.Pow)
                number = left.Pow(right, out error);
            else if (node.OpToken.Type == TokenType.EE)
                number = left.GetComparisonEE(right, out error);
            else if (node.OpToken.Type == TokenType.NE)
                number = left.GetComparisonNE(right, out error);
            else if (node.OpToken.Type == TokenType.LT)
                number = left.GetComparisonLT(right, out error);
            else if (node.OpToken.Type == TokenType.GT)
                number = left.GetComparisonGT(right, out error);
            else if (node.OpToken.Type == TokenType.LTE)
                number = left.GetComparisonLTE(right, out error);
            else if (node.OpToken.Type == TokenType.GTE)
                number = left.GetComparisonGTE(right, out error);
            else if (node.OpToken.Matches(TokenType.Keyword, "and"))
                number = left.And(right, out error);
            else if (node.OpToken.Matches(TokenType.Keyword, "or"))
                number = left.Or(right, out error);

            return res.Success($"({left_code}.{node.OpToken.Value}({right_code}))", number.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_UnaryOpNode(UnaryOpNode node, Context context)
        {
            var res = new TranspilerResult();
            var number = res.Register(Visit(node.Node, context));
            if (res.ShouldReturn()) return res;

            IError error = null;
            if (node.Token.Type == TokenType.Minus)
                number = number.Mul(new Number(-1), out error);
            else if (node.Token.Matches(TokenType.Keyword, "not"))
                number = number.Not(out error);

            return res.Success(number.ValueStr, number.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_IfNode(IfNode node, Context context)
        {
            var res = new TranspilerResult();
            var code = "";
            var cases = node.Cases.ToArray();
            if (cases.Length > 0)
            {
                var if_case = cases[0];
                var condition_value = res.Register(Visit(if_case.Key, context));
                var condition_code = res.Code;
                code += $"if({condition_code})" + "\n{\n";
                if (res.ShouldReturn()) return res;

                var expr_value = res.Register(Visit(if_case.Value.Key, context));
                code += $" {res.Code}" + "\n}\n";
                if (cases.Length > 1)
                {
                    for (int i = 1; i < cases.Length; i++)
                    {
                        var c = cases[i];
                        condition_value = res.Register(Visit(c.Key, context));
                        condition_code = res.Code;
                        code += $"else if({condition_code})" + "\n{\n";
                        if (res.ShouldReturn()) return res;

                        expr_value = res.Register(Visit(c.Value.Key, context));
                        code += $" {res.Code}" + "\n}\n";
                    }
                }
            }

            if (node.ElseCase != null)
            {
                var else_value = res.Register(Visit(node.ElseCase, context));
                code += "else\n{\n " + res.Code + "\n}\n";
            }

            return res.ShouldReturn() ? res : res.Success(code, Number.Empty);
        }
        public TranspilerResult Visit_ForNode(ForNode node, Context context)
        {
            return new TranspilerResult().Success("ForNode", Number.Empty);
        }
        public TranspilerResult Visit_WhileNode(WhileNode node, Context context)
        {
            var res = new TranspilerResult();
            var elements = new List<Value>();

            string code = "";
            Value value;

            var condition = res.Register(Visit(node.ConditionNode, context));
            code = $"while({res.Code})\n";

            value = res.Register(Visit(node.BodyNode, context));
            code += $"{{\n{res.Code}\n}}\n";

            if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak) return res;

            return res.Success(code,node.ShouldReturnNull ? Number.Null : new List(elements).SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_FunctionDefNode(FuncDefNode node, Context context)
        {
            var res = new TranspilerResult();
            var func_name = node.VarNameToken?.Value;
            var body_node = node.BodyNode;
            var arg_names = new List<string>();
            foreach (var tok in node.ArgsTokens)
                arg_names.Add(tok.Value);
            var func_value = new Function(func_name, body_node, arg_names, node.ShouldAutoReturn)
                .SetContext(context).SetPosition(node.PositionStart, node.PositionEnd);

            if (node.VarNameToken != null)
                context.SymbolTable.Set(func_name, func_value);

            var code = "";
            if (node.ShouldAutoReturn)
            {
                code = $"Value {func_name}({string.Join(",", arg_names.Select(a => $"Value {a}"))})=>{res.Code}{(res.Code.Trim()[res.Code.Trim().Length - 1] != ';' ? ";" : "")}\n";
                //string args_str = string.Join(",", func.ArgNames.Select(a => $"{a}"));
                //string args_type = string.Join(",", args.Select(a => $"{a.Type()}"));
                //code = $"Func<{(string.IsNullOrEmpty(args_type) ? "" : $"{ args_type },")}{body.Type()}> {func.Name} = ({args_str}) => {res.Code};\n";
            }
            else
            {
                code = $"Value {func_name}({string.Join(",", arg_names.Select(a => $"Value {a}"))})\n{{\n {res.Code}}}\n";
                //string args_str = string.Join(",", func.ArgNames.Select(a => $"{a}"));
                //string args_type = string.Join(",", args.Select(a => $"{a.Type()}"));
                //code = $"Func<{args_type},{body.Type()}> {func.Name} = ({args_str}) => {{{res.Code}; \nreturn new Action(() => {{ }});}}\n";
            }

            return res.Success(code, func_value);
        }
        public TranspilerResult Visit_CallNode(CallNode node, Context context)
        {
            var res = new TranspilerResult();
            var args = new List<Value>();

            var value_to_call = res.Register(Visit(node.NodeToCall, context));
            if (res.ShouldReturn()) return res;
            value_to_call = value_to_call.Clone().SetPosition(node.PositionStart, node.PositionEnd);

            foreach (var arg in node.ArgsNodes)
            {
                args.Add(res.Register(Visit(arg, context)));
                if (res.ShouldReturn()) return res;
            }

            var code = "";
            var func = value_to_call as Function;
            if (func != null)
            {
                var exe_ctx = new Context(func.Name, func.Context, func.PositionStart);
                exe_ctx.SymbolTable = new SymbolTable(exe_ctx.Parent.SymbolTable);
                BaseFunction.PopulateArgs(func.ArgNames, args, ref exe_ctx);

                /*var body = res.Register(Visit(func.Body, exe_ctx));
                if (res.Error != null) return res;
                if (body == null) body = res.FunctionReturnValue;

                var shouldReturn = res.Code.Contains("return");
                int i = 0;
                if (func.ShouldAutoReturn)
                {
                    if(body.IsTrue())
                        code = $"{body.Type()} {func.Name}({string.Join(",", func.ArgNames.Select(a => $"{args[i++].Type()} {a}"))})=>{res.Code}{(res.Code.Trim()[res.Code.Trim().Length - 1] != ';' ? ";" : "")}\n";
                    else
                        code = $"void {func.Name}({string.Join(",", func.ArgNames.Select(a => $"{args[i++].Type()} {a}"))})=>{res.Code}\n";
                    //string args_str = string.Join(",", func.ArgNames.Select(a => $"{a}"));
                    //string args_type = string.Join(",", args.Select(a => $"{a.Type()}"));
                    //code = $"Func<{(string.IsNullOrEmpty(args_type) ? "" : $"{ args_type },")}{body.Type()}> {func.Name} = ({args_str}) => {res.Code};\n";
                }
                else
                {
                    if (shouldReturn)
                        code = $"Value {func.Name}({string.Join(",", func.ArgNames.Select(a => $"Value {a}"))})\n{{\n {res.Code}}}\n";
                    else
                        code = $"void {func.Name}({string.Join(",", func.ArgNames.Select(a => $"{args[i++].Type()} {a}"))})\n{{\n {res.Code}}}\n";
                    //string args_str = string.Join(",", func.ArgNames.Select(a => $"{a}"));
                    //string args_type = string.Join(",", args.Select(a => $"{a.Type()}"));
                    //code = $"Func<{args_type},{body.Type()}> {func.Name} = ({args_str}) => {{{res.Code}; \nreturn new Action(() => {{ }});}}\n";
                }*/
                code += $"{func.Name}({string.Join(",", args.Select(a => $"{a}"))});\n";
            }
            else
            {
                var bifunc = value_to_call as BuilInFunction;
                code += $"{bifunc.Name}({string.Join(",", args.Select(a => $"{a}"))});\n";
            }


            //var return_value = res.Register(value_to_call.Execute(args));
            return res.Success(code, Number.Empty.Clone().SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_StringNode(StringNode node, Context context)
        {
            return new TranspilerResult()
                .Success($"new StringVal(\"{node.Token.Value}\")", new StringVal(node.Token.Value)
                .SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_ListNode(ListNode node, Context context)
        {
            var res = new TranspilerResult();
            var elements = new List<Value>();
            var _code = "";
            string spliter = node.WrapperMode ? "\n" : ",";

            foreach (var element_node in node.Elements)
            {
                elements.Add(res.Register(Visit(element_node, context)));
                _code += spliter + res.Code;
                if (res.ShouldReturn()) return res;
            }
            _code = _code.Remove(0,1);
            _code = node.WrapperMode ? _code : $"[{_code}]";

            return res.Success(_code, new List(elements).SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public TranspilerResult Visit_ReturnNode(ReturnNode node, Context context)
        {
            var res = new TranspilerResult();
            Value value;
            if (node.Node != null)
            {
                value = res.Register(Visit(node.Node, context));
                if (res.ShouldReturn()) return res;
            }
            else value = Number.Null;

            return res.SuccessReturn($"return {res.Code};\n",value);
        }
        public TranspilerResult Visit_ContinueNode(ContinueNode node, Context context) => new TranspilerResult().SuccessContinue("continue;");
        public TranspilerResult Visit_BreakNode(BreakNode node, Context context) => new TranspilerResult().SuccessBreak("break;");
    }
}
