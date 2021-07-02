using LimOnDotNetCore.Core.Enums;
using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Nodes;
using LimOnDotNetCore.Core.Results;
using LimOnDotNetCore.Core.Values;
using System;
using System.Collections.Generic;

namespace LimOnDotNetCore.Core.Translating
{
    public class Interpeter
    {
        public RunTimeResult Visit(Node node, Context context)
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

            else return new RunTimeResult().Failure(new RunTimeError(
                new Position(),new Position(),
                "Exeption non visit method difiend", context));
        }

        public RunTimeResult Visit_NumberNode(NumberNode node, Context context)
        {
            return new RunTimeResult().Success(new Number(node.Token.Value).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_VarAccessNode(VarAccessNode node, Context context)
        {
            var res = new RunTimeResult();
            var var_name = node.VarNameToken.Value;
            var value = context.SymbolTable.Get(var_name);

            if (value == null)
                return res.Failure(new RunTimeError(
                    node.PositionStart, node.PositionEnd,
                    $"{var_name} is not defined", context));

            return res.Success(value.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_VarAssignNode(VarAssignNode node, Context context)
        {
            var res = new RunTimeResult();
            var var_name = node.VarNameToken.Value;
            var value = res.Register(Visit(node.ValueNode,context));
            if (res.ShouldReturn()) return res;

            context.SymbolTable.Set(var_name, value);
            return res.Success(value.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_BinOpNode(BinOpNode node, Context context)
        {
            var res = new RunTimeResult();
            var left = res.Register(Visit(node.Left, context));
            if (res.ShouldReturn()) return res;
            var right = res.Register(Visit(node.Right, context));
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
            else if (node.OpToken.Matches(TokenType.Keyword,"and"))
                number = left.And(right, out error);
            else if (node.OpToken.Matches(TokenType.Keyword, "or"))
                number = left.Or(right, out error);

            if (error != null) return res.Failure(error);
            if (number == null) return res.Failure(new RunTimeError(
                node.PositionStart,node.PositionEnd,
                "Unknone opration",context));

            return res.Success(number.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_UnaryOpNode(UnaryOpNode node, Context context)
        {
            var res = new RunTimeResult();
            var number = res.Register(Visit(node.Node, context));
            if (res.ShouldReturn()) return res;

            IError error = null;
            if (node.Token.Type == TokenType.Minus)
                number = number.Mul(new Number(-1), out error);
            else if (node.Token.Matches(TokenType.Keyword,"not"))
                number = number.Not(out error);

            return res.Success(number.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_IfNode(IfNode node, Context context)
        {
            var res = new RunTimeResult();

            foreach (var c in node.Cases)
            {
                var condition_value = res.Register(Visit(c.Key, context));
                if (res.ShouldReturn()) return res;

                if (condition_value.IsTrue())
                {
                    var expr_value = res.Register(Visit(c.Value.Key, context));
                    return res.ShouldReturn() ? res : res.Success((c.Value.Value ? Number.Null : expr_value).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
                }
            }

            if(node.ElseCase != null)
            {
                var else_value = res.Register(Visit(node.ElseCase, context));
                return res.ShouldReturn() ? res : res.Success(else_value.SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
            }

            return res.Success(Number.Null.SetContext(context).SetPosition(node.PositionStart,node.PositionEnd));
        }
        public RunTimeResult Visit_ForNode(ForNode node, Context context)
        {
            var res = new RunTimeResult();
            var elements = new List<Value>();

            var start_value = res.Register(Visit(node.StartValueNode, context)) as Number;
            if (res.ShouldReturn()) return res;

            var end_value = res.Register(Visit(node.EndValueNode, context)) as Number;
            if (res.ShouldReturn()) return res;

            Number step_value = new Number(1);
            if (node.StepValueNode != null)
            {
                step_value = res.Register(Visit(node.StepValueNode, context)) as Number;
                if (res.ShouldReturn()) return res;
            }

            var i = start_value.IntValue;

            Func<int, int, bool> condition;
            if(step_value.IntValue >= 0)
                condition = (s, e) => s < e;
            else
                condition = (s, e) => s > e;

            Value value;
            while (condition(i, end_value.IntValue))
            {
                context.SymbolTable.Set(node.VarNameToken.Value, new Number(i));
                i += step_value.IntValue;

                value = res.Register(Visit(node.BodyNode, context));
                if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak) return res;

                if (res.LoopShouldContinue) continue;
                if (res.LoopShouldBreak) break;

                elements.Add(value);
            }
            var val = node.ShouldReturnNull ? Number.Null.Clone() : new List(elements);
            return res.Success(val.SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_WhileNode(WhileNode node, Context context)
        {
            var res = new RunTimeResult();
            var elements = new List<Value>();

            Value value;
            while (true)
            {
                var condition = res.Register(Visit(node.ConditionNode, context));
                if (res.ShouldReturn()) return res;

                if (!condition.IsTrue()) break;
                value = res.Register(Visit(node.BodyNode, context));

                if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak) return res;

                if (res.LoopShouldContinue) continue;
                if (res.LoopShouldBreak) break;

                elements.Add(value);
            }

            return res.Success(node.ShouldReturnNull ? Number.Null : new List(elements).SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_FunctionDefNode(FuncDefNode node, Context context)
        {
            var res = new RunTimeResult();
            var func_name = node.VarNameToken?.Value;
            var body_node = node.BodyNode;
            var arg_names = new List<string>();
            foreach (var tok in node.ArgsTokens) 
                arg_names.Add(tok.Value);
            var func_value = new Function(func_name, body_node, arg_names, node.ShouldAutoReturn)
                .SetContext(context).SetPosition(node.PositionStart,node.PositionEnd);

            if (node.VarNameToken != null)
                context.SymbolTable.Set(func_name, func_value);

            return res.Success(func_value);
        }
        public RunTimeResult Visit_CallNode(CallNode node, Context context)
        {
            var res = new RunTimeResult();
            var args = new List<Value>();

            var value_to_call = res.Register(Visit(node.NodeToCall, context));
            if (res.ShouldReturn()) return res;
            value_to_call = value_to_call.Clone().SetPosition(node.PositionStart,node.PositionEnd);

            foreach (var arg in node.ArgsNodes)
            {
                args.Add(res.Register(Visit(arg, context)));
                if (res.ShouldReturn()) return res;
            }

            var return_value = res.Register(value_to_call.Execute(args));
            return res.ShouldReturn() ? res : res.Success(return_value.Clone().SetContext(context).SetPosition(node.PositionStart,node.PositionEnd));
        }
        public RunTimeResult Visit_StringNode(StringNode node, Context context)
        {
            return new RunTimeResult()
                .Success(new StringVal(node.Token.Value)
                .SetContext(context)
                .SetPosition(node.PositionStart,node.PositionEnd));
        }
        public RunTimeResult Visit_ListNode(ListNode node, Context context)
        {
            var res = new RunTimeResult();
            var elements = new List<Value>();

            foreach (var element_node in node.Elements)
            {
                elements.Add(res.Register(Visit(element_node, context)));
                if (res.ShouldReturn()) return res;
            }

            return res.Success(new List(elements).SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_ReturnNode(ReturnNode node, Context context)
        {
            var res = new RunTimeResult();
            Value value;
            if (node.Node != null)
            {
                value = res.Register(Visit(node.Node, context));
                if (res.ShouldReturn()) return res;
            }
            else value = Number.Null;

            return res.SuccessReturn(value.SetContext(context)
                .SetPosition(node.PositionStart, node.PositionEnd));
        }
        public RunTimeResult Visit_ContinueNode(ContinueNode node, Context context)=> new RunTimeResult().SuccessContinue();
        public RunTimeResult Visit_BreakNode(BreakNode node, Context context)=> new RunTimeResult().SuccessBreak();
    }
}
