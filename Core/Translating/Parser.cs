using LimOnDotNetCore.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using LimOnDotNetCore.Core.Results;
using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Enums;

namespace LimOnDotNetCore.Core.Translating
{
    public delegate ParseResult INodeCallBack();
    public class Parser
    {
        public int TokenIndex { get; set; }
        public Token CurrentToken { get; set; }
        public IEnumerable<Token> Tokens { get; set; }
        public Parser(IEnumerable<Token> tokens)
        {
            Tokens = tokens;
            TokenIndex = -1;
            Advance();
        }

        public ParseResult Parse()
        {
            var res = Statements();
            if (res.Error == null && CurrentToken.Type != TokenType.Eof)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected '+', '-', '*' '/', '^', '==', '!=', '<', '>', '<=', '>=', 'and', 'or'"));

            return res;
        }
        Token Advance()
        {
            TokenIndex++;
            UpdateCurrentToken();

            return CurrentToken;
        }
        Token Reverse(int amount)
        {
            TokenIndex -= amount;
            UpdateCurrentToken();

            return CurrentToken;
        }
        void UpdateCurrentToken()
        {
            if (TokenIndex >= 0 && TokenIndex < Tokens.Count())
                CurrentToken = Tokens.ElementAt(TokenIndex);
        }


        ParseResult Statements()
        {
            var res = new ParseResult();
            var statements = new List<Node>();
            var pos_start = CurrentToken.PositionStart.Clone();

            while(CurrentToken.Type == TokenType.NewLine)
            {
                res.RegisterAdvancement();
                Advance();
            }

            var statement = res.Register(Statement());
            if (res.HasError()) return res;
            statements.Add(statement);

            bool more_statements = true;
            while (true)
            {
                int lineCount = 0;
                while (CurrentToken.Type == TokenType.NewLine)
                {
                    res.RegisterAdvancement();
                    Advance();
                    lineCount++;
                }
                if (lineCount == 0) more_statements = false;

                if (!more_statements) break;
                statement = res.TryRegister(Statement());
                if(statement == null)
                {
                    Reverse(res.ToReverseCont);
                    more_statements = false;
                    continue;
                }
                statements.Add(statement);
            }

            return res.Success(new ListNode(statements, pos_start, CurrentToken.PositionStart.Clone(), true));
        }
        ParseResult Statement()
        {
            var res = new ParseResult();
            var pos_start = CurrentToken.PositionStart.Clone();

            if (CurrentToken.Matches(TokenType.Keyword, "return"))
            {
                res.RegisterAdvancement();
                Advance();

                var expr = res.TryRegister(Expr());
                if (expr == null) Reverse(res.ToReverseCont);

                return res.Success(new ReturnNode(expr, pos_start, CurrentToken.PositionStart.Clone()));
            }

            if (CurrentToken.Matches(TokenType.Keyword, "continue"))
            {
                res.RegisterAdvancement();
                Advance();

                return res.Success(new ContinueNode(pos_start, CurrentToken.PositionStart.Clone()));
            }

            if (CurrentToken.Matches(TokenType.Keyword, "break"))
            {
                res.RegisterAdvancement();
                Advance();

                return res.Success(new BreakNode(pos_start, CurrentToken.PositionStart.Clone()));
            }

            var exprr = res.Register(Expr());
            if (res.HasError())
                return res.Failure(new InvalidSyntaxError(
                CurrentToken.PositionStart, CurrentToken.PositionEnd,
                "Expected 'return', 'continue', 'break', 'var', 'if', 'for', 'while', 'fun', int, float, identifier, '+', '-', '(' ,'[' or 'not'"));

            return res.Success(exprr);
        }
        ParseResult IfExprCases(string keyword)
        {
            var res = new ParseResult();
            Node else_case = null;
            Dictionary<Node, KeyValuePair<Node,bool>> cases = new Dictionary<Node, KeyValuePair<Node, bool>>();

            if (!CurrentToken.Matches(TokenType.Keyword, keyword))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    $"Expected '{keyword}'"));

            res.RegisterAdvancement();
            Advance();

            var condition = res.Register(Expr());
            if (res.HasError()) return res;

            if (!CurrentToken.Matches(TokenType.Keyword, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'then'"));

            res.RegisterAdvancement();
            Advance();

            if(CurrentToken.Type == TokenType.NewLine)
            {
                res.RegisterAdvancement();
                Advance();

                var statements = res.Register(Statements());
                if (res.HasError()) return res;

                cases.Add(condition, new KeyValuePair<Node,bool>(statements,true));

                if (CurrentToken.Matches(TokenType.Keyword, "end"))
                {
                    res.RegisterAdvancement();
                    Advance();
                }
                else
                {
                    var all_cases = res.Register(IfExpr_B_or_C()) as IfNode;
                    if (res.HasError()) return res;
                    else_case = all_cases.ElseCase;
                    foreach (var c in all_cases.Cases) 
                        cases.Add(c.Key, c.Value);
                }
            }
            else
            {
                var expr = res.Register(Statement());
                if (res.HasError()) return res;
                cases.Add(condition, new KeyValuePair<Node, bool>(expr,false));

                var all_cases = res.Register(IfExpr_B_or_C()) as IfNode;
                if (res.HasError()) return res;
                else_case = all_cases.ElseCase;
                foreach (var c in all_cases.Cases)
                    cases.Add(c.Key, c.Value);
            }

            return res.Success(new IfNode(cases, else_case));
        }
        ParseResult IfExpr()
        {
            var res = new ParseResult();
            var all_cases = res.Register(IfExprCases("if"));
            if (res.HasError()) return res;

            return res.Success(all_cases);
        }
        ParseResult IfExprB()
        {
            return IfExprCases("elif");
        }
        ParseResult IfExprC()
        {
            var res = new ParseResult();
            KeyValuePair<Node, bool> else_case = new KeyValuePair<Node, bool>(null, false);

            if (CurrentToken.Matches(TokenType.Keyword, "else"))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type == TokenType.NewLine)
                {
                    res.RegisterAdvancement();
                    Advance();

                    var statments = res.Register(Statements());
                    if (res.HasError()) return res;

                    else_case = new KeyValuePair<Node, bool>(statments, true);

                    if (CurrentToken.Matches(TokenType.Keyword, "end"))
                    {
                        res.RegisterAdvancement();
                        Advance();
                    }
                    else return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected 'end'"));
                }
                else
                {
                    var expr = res.Register(Expr());
                    if (res.HasError()) return res;

                    else_case = new KeyValuePair<Node,bool>(expr,false);
                }
            }
            return res.Success(else_case);
        }
        ParseResult IfExpr_B_or_C()
        {
            var res = new ParseResult();
            Node else_case = null;
            Dictionary<Node, KeyValuePair<Node, bool>> cases = new Dictionary<Node, KeyValuePair<Node,bool>>();
            if (CurrentToken.Matches(TokenType.Keyword, "elif"))
            {
                var all_cases = res.Register(IfExprB()) as IfNode;
                if (res.HasError()) return res;
                cases = all_cases.Cases;
                else_case = all_cases.ElseCase;
            }
            else
            {
                else_case = res.Register(IfExprC());
                if (res.HasError()) return res;
            }
            return res.Success(new IfNode(cases,else_case));
    }

        /*ParseResult IfExpr()
        {
            var res = new ParseResult();
            Node else_case = null;
            Dictionary<Node, Node> cases = new Dictionary<Node, Node>();

            if (!CurrentToken.Matches(TokenType.Keyword, "if"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'if'"));

            res.RegisterAdvancement();
            Advance();

            var condition = res.Register(Expr());
            if (res.HasError()) return res;

            if (!CurrentToken.Matches(TokenType.Keyword, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'then'"));

            res.RegisterAdvancement();
            Advance();

            var expr = res.Register(Expr());
            if (res.HasError()) return res;
            cases.Add(condition, expr);

            while(CurrentToken.Matches(TokenType.Keyword, "elif"))
            {
                res.RegisterAdvancement();
                Advance();
                condition = res.Register(Expr());
                if (res.HasError()) return res;

                if (!CurrentToken.Matches(TokenType.Keyword, "then"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected 'then'"));

                res.RegisterAdvancement();
                Advance();

                expr = res.Register(Expr());
                if (res.HasError()) return res;
                cases.Add(condition, expr);
            }

            if (CurrentToken.Matches(TokenType.Keyword, "else"))
            {
                res.RegisterAdvancement();
                Advance();

                else_case = res.Register(Expr());
                if (res.HasError()) return res;
            }
            return res.Success(new IfNode(cases, else_case));
        }*/
        ParseResult ForExpr()
        {
            var res = new ParseResult();

            if (!CurrentToken.Matches(TokenType.Keyword, "for"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'for'"));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type != TokenType.Identifier)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'identifier'"));

            var var_name = CurrentToken;
            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type != TokenType.EQ)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected '='"));

            res.RegisterAdvancement();
            Advance();

            var start_value = res.Register(Expr());
            if (res.HasError()) return res;

            if (!CurrentToken.Matches(TokenType.Keyword, "to"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'to'"));

            res.RegisterAdvancement();
            Advance();

            var end_value = res.Register(Expr());
            if (res.HasError()) return res;

            Node step_value = null;
            if (CurrentToken.Matches(TokenType.Keyword, "step")) {
                res.RegisterAdvancement();
                Advance();
                step_value = res.Register(Expr());
                if (res.HasError()) return res;
            }

            if (!CurrentToken.Matches(TokenType.Keyword, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'then'"));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type == TokenType.NewLine)
            {
                res.RegisterAdvancement();
                Advance();

                var body = res.Register(Statements());
                if (res.HasError()) return res;

                if (!CurrentToken.Matches(TokenType.Keyword, "end"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected 'end'"));

                res.RegisterAdvancement();
                Advance();

                return res.HasError() ? res : res.Success(new ForNode(var_name, start_value, end_value, step_value, body, true));
            } 
            else
            {
                var body = res.Register(Statement());

                return res.HasError() ? res : res.Success(new ForNode(var_name, start_value, end_value, step_value, body, false));
            }
        }
        ParseResult WhileExpr()
        {
            var res = new ParseResult();

            if (!CurrentToken.Matches(TokenType.Keyword, "while"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'while'"));

            res.RegisterAdvancement();
            Advance();

            var condition = res.Register(Expr());
            if (res.HasError()) return res;

            if (!CurrentToken.Matches(TokenType.Keyword, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'then'"));

            res.RegisterAdvancement();
            Advance();


            if (CurrentToken.Type == TokenType.NewLine)
            {
                res.RegisterAdvancement();
                Advance();

                var body = res.Register(Statements());
                if (res.HasError()) return res;

                if (!CurrentToken.Matches(TokenType.Keyword, "end"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected 'end'"));

                res.RegisterAdvancement();
                Advance();

                return res.HasError() ? res : res.Success(new WhileNode(condition, body, true));
            }
            else
            {
                var body = res.Register(Statement());
                return res.HasError() ? res : res.Success(new WhileNode(condition, body, false));
            }
        }
        ParseResult FuncDef()
        {
            var res = new ParseResult();

            if (!CurrentToken.Matches(TokenType.Keyword, "func"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'func'"));

            res.RegisterAdvancement();
            Advance();

            Token var_name_token = null;
            if(CurrentToken.Type == TokenType.Identifier)
            {
                var_name_token = CurrentToken;
                res.RegisterAdvancement();
                Advance();
                if(CurrentToken.Type != TokenType.Lparem)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected '('"));
            }
            else
            {
                if (CurrentToken.Type != TokenType.Lparem)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected identifier or '('"));
            }

            res.RegisterAdvancement();
            Advance();

            var args_tokens = new List<Token>();
            if (CurrentToken.Type == TokenType.Identifier)
            {
                args_tokens.Add(CurrentToken);
                res.RegisterAdvancement();
                Advance();

                while(CurrentToken.Type == TokenType.Comma)
                {
                    res.RegisterAdvancement();
                    Advance();

                    if (CurrentToken.Type != TokenType.Identifier)
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart, CurrentToken.PositionEnd,
                            "Expected identifier"));

                    args_tokens.Add(CurrentToken);
                    res.RegisterAdvancement();
                    Advance();

                }

                if (CurrentToken.Type != TokenType.Rparem)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected ',' or ')'"));
            }
            else
            {
                if (CurrentToken.Type != TokenType.Rparem)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected identifier or ')'"));
            }

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type == TokenType.Arrow)
            {

                res.RegisterAdvancement();
                Advance();

                var node_to_retrun = res.Register(Expr());
                return res.HasError() ? res : res.Success(new FuncDefNode(
                    var_name_token,
                    args_tokens,
                    node_to_retrun,
                    true
                    ));
            }

            if (CurrentToken.Type != TokenType.NewLine)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected '->' newline"));

            res.RegisterAdvancement();
            Advance();

            var body = res.Register(Statements());
            if (res.HasError()) return res;

            if (!CurrentToken.Matches(TokenType.Keyword, "end"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected 'end'"));

            res.RegisterAdvancement();
            Advance();

            return res.HasError() ? res : res.Success(new FuncDefNode(
                var_name_token,
                args_tokens,
                body,
                false
                ));
        }
        ParseResult Call()
        {
            var res = new ParseResult();
            var atom = res.Register(Atom());
            if (res.HasError()) return res;

            if (CurrentToken.Type == TokenType.Lparem)
            {
                res.RegisterAdvancement();
                Advance();
                var args_nodes = new List<Node>();
                if(CurrentToken.Type == TokenType.Rparem)
                {
                    res.RegisterAdvancement();
                    Advance();
                }
                else
                {
                    args_nodes.Add(res.Register(Expr()));
                    if (res.HasError())
                        return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected ')', 'var', 'if', 'for', 'while', 'func', int, float, identifier, '+', '-', '(', '[' or 'not'"));

                    while(CurrentToken.Type == TokenType.Comma) 
                    {
                        res.RegisterAdvancement();
                        Advance();

                        args_nodes.Add(res.Register(Expr()));
                        if (res.HasError()) return res;
                    }

                    if(CurrentToken.Type != TokenType.Rparem)
                        return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected ',' or ')'"));

                    res.RegisterAdvancement();
                    Advance();
                }
                return res.Success(new CallNode(atom, args_nodes));
            }
            return res.Success(atom);
        }
        ParseResult ListExpr()
        {
            var res = new ParseResult();
            var elements = new List<Node>();
            var pos_start = CurrentToken.PositionStart.Clone();

            if (CurrentToken.Type != TokenType.LSQparem)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected '['"));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type == TokenType.RSQparem)
            {
                res.RegisterAdvancement();
                Advance();
            }
            else
            {
                elements.Add(res.Register(Expr()));
                if (res.HasError())
                    return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected ']', 'var', 'if', 'for', 'while', 'func', int, float, identifier, '+', '-', '(', '[' or 'not'"));

                while (CurrentToken.Type == TokenType.Comma)
                {
                    res.RegisterAdvancement();
                    Advance();

                    elements.Add(res.Register(Expr()));
                    if (res.HasError()) return res;
                }

                if (CurrentToken.Type != TokenType.RSQparem)
                    return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart, CurrentToken.PositionEnd,
                    "Expected ',' or ')'"));

                res.RegisterAdvancement();
                Advance();
            }
            return res.Success(new ListNode(
                elements,
                pos_start,
                CurrentToken.PositionEnd.Clone()));
        }
        ParseResult Atom()
        {
            var res = new ParseResult();
            var token = CurrentToken;

            if (new TokenType[] { TokenType.Int, TokenType.Float }.Contains(token.Type))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new NumberNode(token));
            }

            else if(token.Type == TokenType.String)
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new StringNode(token));
            }
            else if (token.Type.Equals(TokenType.Identifier))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new VarAccessNode(token));
            }
            else if(token.Type == TokenType.Lparem)
            {
                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expr());
                if (res.HasError()) return res;

                if (token.Type == TokenType.Lparem)
                {
                    res.RegisterAdvancement();
                    Advance();
                    return res.Success(expr);
                }
                else return res.Failure(new InvalidSyntaxError(
                    token.PositionStart, token.PositionEnd,
                    "Expected ')'"));
            }
            else if (token.Type == TokenType.LSQparem)
            {
                var list_expr = res.Register(ListExpr());
                return res.HasError() ? res : res.Success(list_expr);
            }
            else if (token.Matches(TokenType.Keyword, "if"))
            {
                var if_expr = res.Register(IfExpr());
                return res.HasError() ? res : res.Success(if_expr);
            }
            else if (token.Matches(TokenType.Keyword, "for"))
            {
                var for_expr = res.Register(ForExpr());
                return res.HasError() ? res : res.Success(for_expr);
            }
            else if (token.Matches(TokenType.Keyword, "while"))
            {
                var while_expr = res.Register(WhileExpr());
                return res.HasError() ? res : res.Success(while_expr);
            }
            else if (token.Matches(TokenType.Keyword, "func"))
            {
                var func_def = res.Register(FuncDef());
                return res.HasError() ? res : res.Success(func_def);
            }

            return res.Failure(new InvalidSyntaxError(
                token.PositionStart,token.PositionEnd,
                "Expected int, float, identifier, '+', '-', '(', '[', 'if', 'for', 'while', 'fun'"));
        }
        ParseResult Factor()
        {
            var res = new ParseResult();
            var token = CurrentToken;

            if (new TokenType[] { TokenType.Plus, TokenType.Minus }.Contains(token.Type))
            {
                res.RegisterAdvancement();
                Advance();
                var factor = res.Register(Factor());
                
                return res.HasError() ? res : res.Success(new UnaryOpNode(token, factor));
            }

            return Power();
        }
        ParseResult CompExpr()
        {
            var res = new ParseResult();

            if (CurrentToken.Matches(TokenType.Keyword, "not"))
            {
                var op_token = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                var n = res.Register(CompExpr());
                return res.HasError() ? res : res.Success(new UnaryOpNode(op_token, n));
            }

            var node = res.Register(BinOp(new INodeCallBack(ArithExpr),
                new TokenType[] { TokenType.EE, TokenType.NE, TokenType.LT, TokenType.GT, TokenType.LTE, TokenType.GTE }));
            if (res.HasError()) return res.Failure(new InvalidSyntaxError(
                 CurrentToken.PositionStart, CurrentToken.PositionEnd,
                 "Expected int, float, identifier, '+', '-' or '(', '[' or 'not'"));

            return res.Success(node);
        }
        ParseResult Expr() {
            var res = new ParseResult();

            if (CurrentToken.Matches(TokenType.Keyword, "var"))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type != TokenType.Identifier)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected identifier"));

                var var_name = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type != TokenType.EQ)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart, CurrentToken.PositionEnd,
                        "Expected '='"));

                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expr());
                if (res.HasError()) return res;
                return res.Success(new VarAssignNode(var_name, expr));
            }
            else if(CurrentToken.Type == TokenType.Identifier) {
                var var_name = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type == TokenType.EQ)
                {
                    res.RegisterAdvancement();
                    Advance();
                    var expr = res.Register(Expr());
                    if (res.HasError()) return res;
                    return res.Success(new VarAssignNode(var_name, expr,false));
                }
                else Reverse(1);
            }
            var tokens = new Token[] {
                new Token(TokenType.Keyword, "or"),
                new Token(TokenType.Keyword, "and") };

            var node = res.Register(BinOp(new INodeCallBack(CompExpr), tokens));

            if (res.HasError())
                return res.Failure(new InvalidSyntaxError(
                CurrentToken.PositionStart, CurrentToken.PositionEnd,
                "Expected 'var', 'if', 'for', 'while', 'func', int, float, identifier, '+', '-', '(' ,'[' or 'not'"));

            return res.Success(node);
        }
        ParseResult Term() => BinOp(new INodeCallBack(Factor), new TokenType[] { TokenType.Mul, TokenType.Div });
        ParseResult ArithExpr() => BinOp(new INodeCallBack(Term), new TokenType[] { TokenType.Plus, TokenType.Minus });
        ParseResult Power() => BinOp(new INodeCallBack(Call), new TokenType[] { TokenType.Pow, }, new INodeCallBack(Factor));

        ParseResult BinOp(INodeCallBack callBackA, Token[] keywords, INodeCallBack callBackB = null)
        {
            if (callBackB == null)
                callBackB = callBackA;

            var res = new ParseResult();
            var left = res.Register(callBackA());
            if (res.Error != null) return res;

            while (keywords.SingleOrDefault(k=> k.Type == CurrentToken.Type && k.Value == CurrentToken.Value) != null)
            {
                var op_token = CurrentToken;
                res.RegisterAdvancement();
                Advance();
                var right = res.Register(callBackB());
                if (res.Error != null) return res;

                left = new BinOpNode(left, op_token, right);
            }

            return res.Success(left);
        }
        ParseResult BinOp(INodeCallBack callBackA, TokenType[] tokenTypes, INodeCallBack callBackB = null)
        {
            if (callBackB == null)
                callBackB = callBackA;

            var res = new ParseResult();
            var left = res.Register(callBackA());
            if (res.Error != null) return res;

            while (tokenTypes.Contains(CurrentToken.Type))
            {
                var op_token = CurrentToken;
                res.RegisterAdvancement();
                Advance();
                var right = res.Register(callBackB());
                if (res.Error != null) return res;

                left = new BinOpNode(left, op_token, right);
            }

            return res.Success(left);
        }
    }
}
