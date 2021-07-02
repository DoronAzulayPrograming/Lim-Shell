using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Nodes;
using LimOnDotNetCore.Core.Results;
using LimOnDotNetCore.Core.Translating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LimOnDotNetCore.Core.Values
{
    public class BaseFunction : Value
    {
        public string Name { get; set; }
        public BaseFunction(string name = null)
        {
            Name = string.IsNullOrEmpty(name) ? "<anonymos>" : name;
        }

        public Context GenerateNewContext()
        {
            var context = new Context(Name, Context, PositionStart);
            context.SymbolTable = new SymbolTable(context.Parent.SymbolTable);
            return context;
        }
        public RunTimeResult CheckArgs(IEnumerable<string> arg_names, IEnumerable<Value> args)
        {
            var res = new RunTimeResult();
            if (args.Count() > arg_names.Count())
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    $"{args.Count() - arg_names.Count()} to many args passed into '{Name}'",
                    Context));

            if (args.Count() < arg_names.Count())
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    $"{arg_names.Count() - args.Count()} to few args passed into '{Name}'",
                    Context));

            return res.Success(null);
        }

        public void PopulateArgs(IEnumerable<string> arg_names, IEnumerable<Value> args, Context exec_ctx)
        {
            for (int i = 0; i < args.Count(); i++)
            {
                var arg_name = arg_names.ElementAt(i);
                var arg_value = args.ElementAt(i);
                arg_value.SetContext(exec_ctx);
                exec_ctx.SymbolTable.Set(arg_name, arg_value);
            }
        }
        public static void PopulateArgs(IEnumerable<string> arg_names, IEnumerable<Value> args, ref Context exec_ctx)
        {
            for (int i = 0; i < args.Count(); i++)
            {
                var arg_name = arg_names.ElementAt(i);
                var arg_value = args.ElementAt(i);
                arg_value.SetContext(exec_ctx);
                exec_ctx.SymbolTable.Set(arg_name, arg_value);
            }
        }

        public RunTimeResult CheckAndPopulateArgs(IEnumerable<string> arg_names, IEnumerable<Value> args, Context exec_ctx)
        {
            var res = new RunTimeResult();
            res.Register(CheckArgs(arg_names,args));
            if (res.ShouldReturn()) return res;
            PopulateArgs(arg_names, args, exec_ctx);
            return res.Success(null);
        }
    }

    public class BuilInFunction : BaseFunction
    {
        public static readonly string[] FunctionList = new string[] { "print" , "len", "append" };
        public static BuilInFunction Print { get; } = new BuilInFunction("print");
        public static BuilInFunction PrintReturn { get; } = new BuilInFunction("printr");
        public static BuilInFunction Input { get; } = new BuilInFunction("input");
        public static BuilInFunction InputInt { get; } = new BuilInFunction("inputi");
        public static BuilInFunction Clear { get; } = new BuilInFunction("clear");
        public static BuilInFunction IsNumber { get; } = new BuilInFunction("is_number");
        public static BuilInFunction IsString { get; } = new BuilInFunction("is_string");
        public static BuilInFunction IsList { get; } = new BuilInFunction("is_list");
        public static BuilInFunction IsFunction { get; } = new BuilInFunction("is_function");
        public static BuilInFunction Append { get; } = new BuilInFunction("append");
        public static BuilInFunction Pop { get; } = new BuilInFunction("pop");
        public static BuilInFunction Extend { get; } = new BuilInFunction("extend");
        public static BuilInFunction Length { get; } = new BuilInFunction("len");
        public static BuilInFunction Run { get; } = new BuilInFunction("run");

        public BuilInFunction(string name):base(name)
        {

        }
      
        public override RunTimeResult Execute(IEnumerable<Value> args)
        {
            var res = new RunTimeResult();
            var exec_context = GenerateNewContext();

            Value return_value = null;
            if (Name.Equals("print"))
            {
                res.Register(CheckAndPopulateArgs(new string[]{ "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimPrint(exec_context));
            }
            else if (Name.Equals("printr"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimPrintReturn(exec_context));
            }
            else if (Name.Equals("input"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimInput());
            }
            else if (Name.Equals("inputi"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimInputInt());
            }
            else if (Name.Equals("is_number"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimIsNumber(exec_context));
            }
            else if (Name.Equals("is_string"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimIsString(exec_context));
            }
            else if (Name.Equals("is_list"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimIsList(exec_context));
            }
            else if (Name.Equals("is_function") || Name.Equals("is_func"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimIsFunction(exec_context));
            }
            else if (Name.Equals("append"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "list", "value" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimAppend(exec_context));
            }
            else if (Name.Equals("pop"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "list", "index" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimPop(exec_context));
            }
            else if (Name.Equals("extend"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "listA", "listB" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimExtend(exec_context));
            }
            else if (Name.Equals("len"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "list" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimLength(exec_context));
            }
            else if (Name.Equals("run"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { "file_name" }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimExecuteRun(exec_context));
            }
            else if (Name.Equals("cls") || Name.Equals("clear"))
            {
                res.Register(CheckAndPopulateArgs(new string[] { }, args, exec_context));
                if (res.ShouldReturn()) return res;
                return_value = res.Register(LimClearConsole());
            }

            return res.ShouldReturn() ? res : res.Success(return_value);
        }
      

        public RunTimeResult LimInput()
        {
            var text = Console.ReadLine();
            return new RunTimeResult().Success(new StringVal(text));
        }
        public RunTimeResult LimInputInt()
        {
            var res = new RunTimeResult();
            int value = 0;
            while (true)
            {
                try
                {
                    var text = Console.ReadLine();
                    if (!int.TryParse(text, out value))
                    {
                        Console.WriteLine("must be an integer. try again!");
                    }
                    else break;
                }
                catch (Exception)
                {
                    Console.WriteLine("must be an integer. try again!");
                }
            }
            return res.Success(new Number(value));
        }
        public RunTimeResult LimClearConsole()
        {
            Console.Clear();
            return new RunTimeResult().Success(Number.Empty);
        }
        public RunTimeResult LimPrint(Context exec_ctx)
        {
            var value = exec_ctx.SymbolTable.Get("value");
            string str = value.ToRrepresent();

            Console.WriteLine(string.IsNullOrWhiteSpace(str) ? $"{value}" : str);
            return new RunTimeResult().Success(Number.Empty);
        }
        public RunTimeResult LimPrintReturn(Context exec_ctx)
        {
            return new RunTimeResult().Success(new StringVal($"{exec_ctx.SymbolTable.Get("value")}"));
        }
        public RunTimeResult LimIsNumber(Context exec_ctx)
        {
            var is_number = typeof(Number).IsInstanceOfType(exec_ctx.SymbolTable.Get("value"));
            return new RunTimeResult().Success(new Number(is_number));
        }
        public RunTimeResult LimIsString(Context exec_ctx)
        {
            var is_number = typeof(StringVal).IsInstanceOfType(exec_ctx.SymbolTable.Get("value"));
            return new RunTimeResult().Success(new Number(is_number));
        }
        public RunTimeResult LimIsList(Context exec_ctx)
        {
            var is_number = typeof(List).IsInstanceOfType(exec_ctx.SymbolTable.Get("value"));
            return new RunTimeResult().Success(new Number(is_number));
        }
        public RunTimeResult LimIsFunction(Context exec_ctx)
        {
            var is_number = typeof(BaseFunction).IsInstanceOfType(exec_ctx.SymbolTable.Get("value"));
            return new RunTimeResult().Success(new Number(is_number));
        }
        public RunTimeResult LimAppend(Context exec_ctx)
        {
            var res = new RunTimeResult();
            var list = exec_ctx.SymbolTable.Get("list");
            var value = exec_ctx.SymbolTable.Get("value");

            if (!typeof(List).IsInstanceOfType(list))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "First argument must be list", exec_ctx));

            var listVal = (list as List);
            listVal.Elements.Add(value);

            /*(list as ListVal).Add(value, out var error);
            if (error != null) return res.Failure(error);*/

            return res.Success(Number.Empty);
        }
        public RunTimeResult LimPop(Context exec_ctx)
        {
            var res = new RunTimeResult();
            var list = exec_ctx.SymbolTable.Get("list");
            var index = exec_ctx.SymbolTable.Get("index");

            if (!typeof(List).IsInstanceOfType(list))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "First argument must be list", exec_ctx));

            if (!typeof(Number).IsInstanceOfType(index))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "Second argument must be number", exec_ctx));

            /*var number = (index as Number);
            var listVal = (list as ListVal);
            var element = listVal.Elements.ElementAt(number.IntValue);*/

            var element = (list as List).Div(index, out var error);
            if (error != null) return res.Failure(error);

            return res.Success(element);
        }
        public RunTimeResult LimExtend(Context exec_ctx)
        {
            var res = new RunTimeResult();
            var listA = exec_ctx.SymbolTable.Get("listA");
            var listB = exec_ctx.SymbolTable.Get("listB");

            if (!typeof(List).IsInstanceOfType(listA))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "First argument must be list", exec_ctx));

            if (!typeof(List).IsInstanceOfType(listB))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "Second argument must be list", exec_ctx));

            /*var number = (index as Number);
            var listVal = (list as ListVal);
            var element = listVal.Elements.ElementAt(number.IntValue);*/

            (listA as List).Mul(listB, out var error);
            if (error != null) return res.Failure(error);

            return res.Success(Number.Empty);
        }
        public RunTimeResult LimLength(Context exec_ctx)
        {
            var res = new RunTimeResult();
            var list = exec_ctx.SymbolTable.Get("list");

            if (!typeof(List).IsInstanceOfType(list))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "Argument must be list", exec_ctx));

            /*var number = (index as Number);
            var listVal = (list as ListVal);
            var element = listVal.Elements.ElementAt(number.IntValue);*/


            return res.Success(new Number((list as List).Elements.Count));
        }

        public RunTimeResult LimExecuteRun(Context exec_ctx) //file_name
        {
            var res = new RunTimeResult();
            var value = exec_ctx.SymbolTable.Get("file_name");

            if (!typeof(StringVal).IsInstanceOfType(value))
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    "Argument must be string", exec_ctx));

            var file_name = (value as StringVal).ValueStr;
            try
            {
                StreamReader reader = new StreamReader(file_name);
                var limScript = reader.ReadToEnd();

                Basic.Run(file_name, limScript, out IError error);
                reader.Close();

                if (error != null)
                    return res.Failure(new RunTimeError(
                        PositionStart, PositionEnd,
                        $"Failed to finish executeing lim script \"{file_name}\"\n{error}", exec_ctx));
            }
            catch (Exception ex)
            {
                return res.Failure(new RunTimeError(
                    PositionStart, PositionEnd,
                    $"Failed to load file \"{file_name}\"\nDetails: {ex.Message}", exec_ctx));
            }

            return res.Success(Number.Null);
        }

        public override Value Clone() =>
            new BuilInFunction(Name)
            .SetContext(Context)
            .SetPosition(PositionStart,PositionEnd);

        public override string ToString() => $"<built-in function {Name}>";
    }

    public class Function: BaseFunction
    {
        public Node Body { get; set; }
        public bool ShouldAutoReturn { get; set; }
        public IEnumerable<string> ArgNames { get; set; }
        public Function(string name, Node body, IEnumerable<string> arg_names, bool shouldAutoReturn) :base(name)
        {
            Body = body;
            ArgNames = arg_names;
            ShouldAutoReturn = shouldAutoReturn;
        }

        public override Value Clone() => 
            new Function(Name, Body, ArgNames, ShouldAutoReturn)
            .SetContext(Context)
            .SetPosition(PositionStart,PositionEnd);

        public override RunTimeResult Execute(IEnumerable<Value> args)
        {
            var res = new RunTimeResult();
            var interpeter = new Interpeter();
            var exec_context = GenerateNewContext();

            res.Register(CheckAndPopulateArgs(ArgNames, args, exec_context));
            if (res.ShouldReturn()) return res;

            var value = res.Register(interpeter.Visit(Body, exec_context));
            if (res.ShouldReturn() && res.FunctionReturnValue == null) return res;

            Value ret_value = Number.Null;
            if (ShouldAutoReturn) ret_value = value;
            else if (res.FunctionReturnValue != null) ret_value = res.FunctionReturnValue;

            return res.Success(ret_value);
        }

        public override string ToString() => $"<function {Name}>";
    }
}
