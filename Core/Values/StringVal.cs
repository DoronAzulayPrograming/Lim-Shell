using LimOnDotNetCore.Core.Errors;

namespace LimOnDotNetCore.Core.Values
{
    public class StringVal:Value
    {
        public string Value { get; set; }
        public StringVal(string value)
        {
            Value = ValueStr = value;
        }

        public override Value Add(Value other, out IError error)
        {
            if (typeof(StringVal).IsInstanceOfType(other))
            {
                error = null;
                var otherString = other as StringVal;
                return new StringVal(Value + otherString.Value).SetContext(Context).SetPosition(PositionStart,PositionEnd);
            }
            else if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var number = other as Number;
                return new StringVal(Value + number.ValueStr).SetContext(Context).SetPosition(PositionStart,PositionEnd);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Mul(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var otherNumber = other as Number;

                var newString = Value;
                for (int i = 0; i < otherNumber.IntValue; i++) newString += Value;

                return new StringVal(newString).SetContext(Context).SetPosition(PositionStart,PositionEnd);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override bool IsTrue()
        {
            return !string.IsNullOrEmpty(Value);
        }

        public override Value Clone() =>
            new StringVal(Value)
            .SetContext(Context)
            .SetPosition(PositionStart, PositionEnd);
        public override string Type() => "String";
        public override string ToString() => $"\"{Value}\"";
        public override string ToRrepresent() => $"{Value}";
    }
}
