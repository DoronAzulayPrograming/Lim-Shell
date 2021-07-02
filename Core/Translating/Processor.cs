using HtmlAgilityPack;
using LimOnDotNetCore.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Translating
{
    public class Processor
    {
        public CsProject Proccess(string cs_html_code,string namespace_name)
        {
            List<string> vars = new List<string>();
            List<string> funcs = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(cs_html_code);

            string str = "";
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("lim"))
            {
                var code = node.InnerText;
                var attrs = node.GetAttributes();
                var type = attrs.Single(a => a.Name.Equals("type"));
                if (type.Value.Equals("funcdef")) funcs.Add("public "+code);
                else if (type.Value.Equals("vardef")) vars.Add("public " + code);

                str += "\n"+code;
            }
            str = str.Remove(0, 1);

            string freeCode = doc.DocumentNode.InnerText.Replace(str, string.Empty);
            string varsCode = string.Join("\n", vars.Select(v=>v));
            string funcsCode = string.Join("\n", funcs.Select(f=>f));

            string cs_file = $"namespace {namespace_name}{{\npublic class Entry{{\n{varsCode}\n{funcsCode}\npublic void Start(){{\n{freeCode}\n}}\n}}\n}}";
            string cs_program = $"namespace {namespace_name}{{\npublic class Program{{\nstatic void Main(string[] args){{\nEntry entry = new Entry();\nentry.Start();\n}}\n}}\n}}";

            CsProject project = new CsProject();
            project.Name = namespace_name;
            project.Entry = cs_file;
            project.Program = cs_program;
            project.Values = Values.Replace("LimFramework", namespace_name);

            return project;
        }



        string Values = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace LimFramework
{
    public class Value
    {
        public string ValueStr { get; set; }

        public virtual Value Add(Value other)
        {
            throw new Exception($""Illegal operation('{GetType().ToString().Split('.')[1]} + {other.GetType().ToString().Split('.')[1]}')"");
        }
    public virtual Value Sub(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value Div(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} + {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value Mul(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value GetComparisonEE(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value GetComparisonNE(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value GetComparisonLT(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value GetComparisonGT(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }

    public virtual Value And(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value Or(Value other)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]} - {other.GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value Not()
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]}')"");
    }

    public virtual Value Execute(params Value[] args)
    {
        throw new Exception($""Illegal operation ('{GetType().ToString().Split('.')[1]}')"");
    }
    public virtual Value Clone() => throw new Exception(""Non clone method defiend"");
    public virtual string Type() => """";
    public virtual bool IsTrue() => false;
    public override string ToString() => $""{ValueStr}"";
}

public class Number : Value
{
    /// <summary>
    ///  return instance of Number class with value of null
    /// </summary>
    public static Number Empty { get; } = new Number(0);

    public bool IsInt { get; set; }
    public int IntValue { get; set; } //  1 0001 + 0001 = 0010
    public float FloatValue { get; set; }

    public Number(bool value)
    {
        IsInt = true;
        FloatValue = IntValue = value ? 1 : 0;
        ValueStr = IntValue.ToString();
    }
    public Number(int value)
    {
        IsInt = true;
        FloatValue = IntValue = value;
        ValueStr = IntValue.ToString();
    }
    public Number(float value)
    {
        FloatValue = value;
        IntValue = (int)value;
        ValueStr = FloatValue.ToString();
    }
    public Number(string value)
    {
        FloatValue = !string.IsNullOrEmpty(value) ? float.Parse(value) : 0;
        IntValue = (int)FloatValue;
        ValueStr = value;
        IsInt = int.TryParse(value, out var intval);
        if (IsInt)
            IntValue = intval;
    }
    public override Value Add(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;
            return new Number(IntValue + number.IntValue);
        }
        else if (typeof(String).IsInstanceOfType(other))
        {
            return new String(ValueStr + other.ValueStr);
        }
        return default(Value);
    }
    public override Value Sub(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;
            return new Number(IntValue - number.IntValue);
        }

        return base.Sub(other);
    }


    public override Value GetComparisonEE(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;

            return (IsInt ? number.IsInt ? new Number(IntValue == number.IntValue) : new Number(IntValue == number.FloatValue) :
                number.IsInt ? new Number(FloatValue == number.IntValue) : new Number(FloatValue == number.FloatValue));
        }

        return base.And(other);
    }
    public override Value GetComparisonNE(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;

            return (IsInt ? number.IsInt ? new Number(IntValue != number.IntValue) : new Number(IntValue != number.FloatValue) :
                number.IsInt ? new Number(FloatValue != number.IntValue) : new Number(FloatValue != number.FloatValue));
        }

        return base.And(other);
    }
    public override Value GetComparisonLT(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;

            return (IsInt ? number.IsInt ? new Number(IntValue < number.IntValue) : new Number(IntValue < number.FloatValue) :
                number.IsInt ? new Number(FloatValue < number.IntValue) : new Number(FloatValue < number.FloatValue));
        }

        return base.And(other);
    }
    public override Value GetComparisonGT(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;

            return (IsInt ? number.IsInt ? new Number(IntValue > number.IntValue) : new Number(IntValue > number.FloatValue) :
                number.IsInt ? new Number(FloatValue > number.IntValue) : new Number(FloatValue > number.FloatValue));
        }

        return base.And(other);
    }

    public override Value And(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;
            return new Number(IntValue & number.IntValue);
        }

        return base.And(other);
    }
    public override Value Or(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;

            return new Number(IntValue | number.IntValue);
        }

        return base.Or(other);
    }
    public override Value Not()
    {
        return new Number(IntValue == 0 ? 1 : 0);
    }

    public override bool IsTrue() => IntValue > 0;
}
public class String : Value
{
    public String(string value)
    {
        ValueStr = value;
    }
    public override Value Add(Value other)
    {
        return new String(ValueStr + other.ValueStr);
    }
    public override bool IsTrue() => string.IsNullOrWhiteSpace(ValueStr);
}

public class List : Value
{
    public List<Value> Elements { get; set; }
    public int Count { get => Elements.Count; }

    public List()
    {
        Elements = new List<Value>();
    }
    public List(List<Value> elements)
    {
        Elements = elements;
    }
    public List(params Value[] elements)
    {
        Elements = new List<Value>();
        Elements.AddRange(elements);
    }

    public override Value Add(Value other)
    {
        var newList = Clone() as List;
        newList.Elements.Add(other);
        return newList;
    }
    public override Value Sub(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;
            var newList = Clone() as List;
            try
            {
                newList.Elements.RemoveAt(number.IntValue);
                return newList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        return base.Sub(other);
    }
    public override Value Div(Value other)
    {
        if (typeof(Number).IsInstanceOfType(other))
        {
            var number = other as Number;
            var newList = Clone() as List;
            try
            {
                var element = newList.Elements.ElementAt(number.IntValue);
                return element;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        return base.Sub(other);
    }
    public override Value Mul(Value other)
    {
        if (typeof(List).IsInstanceOfType(other))
        {
            var newList = Clone() as List;
            newList.Elements.AddRange((other as List).Elements);
            return newList;
        }
        return base.Sub(other);
    }

    public override Value Clone()
    {
        return new List(Elements);
    }
    public override string ToString() => $""[{string.Join("","", Elements.Select(e => $""{e}""))}]"";
}

public class Function0 : Value
{
    Func<Value> Fun;

    public Function0(Func<Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        Fun();
        return Number.Empty;
    }
}
public class Function1 : Value
{
    Func<Value, Value> Fun;

    public Function1(Func<Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0]);
    }
}
public class Function2 : Value
{
    Func<Value, Value, Value> Fun;

    public Function2(Func<Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1]);
    }
}
public class Function3 : Value
{
    Func<Value, Value, Value, Value> Fun;

    public Function3(Func<Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2]);
    }
}
public class Function4 : Value
{
    Func<Value, Value, Value, Value, Value> Fun;

    public Function4(Func<Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3]);
    }
}
public class Function5 : Value
{
    Func<Value, Value, Value, Value, Value, Value> Fun;

    public Function5(Func<Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4]);
    }
}
public class Function6 : Value
{
    Func<Value, Value, Value, Value, Value, Value, Value> Fun;

    public Function6(Func<Value, Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4], args[5]);
    }
}
public class Function7 : Value
{
    Func<Value, Value, Value, Value, Value, Value, Value, Value> Fun;

    public Function7(Func<Value, Value, Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
    }
}
public class Function8 : Value
{
    Func<Value, Value, Value, Value, Value, Value, Value, Value, Value> Fun;

    public Function8(Func<Value, Value, Value, Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
    }
}
public class Function9 : Value
{
    Func<Value, Value, Value, Value, Value, Value, Value, Value, Value, Value> Fun;

    public Function9(Func<Value, Value, Value, Value, Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
    }
}
public class Function10 : Value
{
    Func<Value, Value, Value, Value, Value, Value, Value, Value, Value, Value, Value> Fun;

    public Function10(Func<Value, Value, Value, Value, Value, Value, Value, Value, Value, Value, Value> fun)
    {
        Fun = fun;
    }

    public override Value Execute(params Value[] args)
    {
        return Fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
    }
}


public static class Buildin
{

    static public Value print(Value value)
    {
        Console.WriteLine(value.ValueStr);
        return Number.Empty;
    }
    static public Value len(Value list)
    {
        if (!typeof(List).IsInstanceOfType(list))
            throw new Exception(""Argument must be list"");

        return new Number((list as List).Count);
    }
    static public Value append(Value list, Value value)
    {
        if (!typeof(List).IsInstanceOfType(list))
            throw new Exception(""First argument must be list"");

        var listVal = (list as List);
        listVal.Elements.Add(value);

        return Number.Empty;
    }
}


}
";
    }
}
