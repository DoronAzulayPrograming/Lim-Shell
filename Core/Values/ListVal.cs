using LimOnDotNetCore.Core.Errors;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core.Values
{
    public class List: Value
    {
        public List<Value> Elements { get; set; }
        public List(List<Value> elements)
        {
            Elements = elements;
        }

        public override Value Add(Value other, out IError error)
        {
            error = null;
            var newList = Clone() as List;
            newList.Elements.Add(other);
            return newList;
        }
        public override Value Sub(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                var newList = Clone() as List;
                try
                {
                    newList.Elements.RemoveAt(number.IntValue);
                    error = null;
                    return newList;
                }
                catch (System.Exception)
                {
                    error = new RunTimeError(
                        other.PositionStart, other.PositionEnd,
                        "Element at this index could not be removed from the list beause index is out of bounds", Context);
                    return null;
                }
            }
            error = IllegalOperation(other);
            return null;
        }
        public override Value Div(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                var newList = Clone() as List;
                try
                {
                    var element = newList.Elements.ElementAt(number.IntValue);
                    error = null;
                    return element;
                }
                catch (System.Exception)
                {
                    error = new RunTimeError(
                        other.PositionStart, other.PositionEnd,
                        "Element at this index could not be found beause index is out of bounds", Context);
                    return null;
                }
            }
            error = IllegalOperation(other);
            return null;
        }
        public override Value Mul(Value other, out IError error)
        {
            if (typeof(List).IsInstanceOfType(other))
            {
                error = null;
                var newList = Clone() as List;
                newList.Elements.AddRange((other as List).Elements);
                return newList;
            }
            error = IllegalOperation(other);
            return null;
        }

        public override Value Clone() =>
            new List(Elements.Select(e=>e.Clone()).ToList())
            .SetContext(Context)
            .SetPosition(PositionStart,PositionEnd);
        public override string Type() => Elements.Count > 0 ? $"List<{Elements[0].Type()}>" : "";

        public override string ToString() => $"[{string.Join(",", Elements.Select(e => $"{e}"))}]";
        public override string ToRrepresent() => string.Join(",", Elements.Select(e => string.IsNullOrEmpty(e.ToRrepresent()) ? e.ToString(): e.ToRrepresent()));//$"{}";
    }
}
