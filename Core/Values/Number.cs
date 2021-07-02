using LimOnDotNetCore.Core.Errors;
using System;

namespace LimOnDotNetCore.Core.Values
{
    public class Number: Value
    {
        /// <summary>
        ///  return instance of Number class with value of null
        /// </summary>
        public static Number Empty { get; } = new Number(0);
        /// <summary>
        ///  return instance of Number class with value of 0
        /// </summary>
        public static Number Null { get; } = new Number(0);
        /// <summary>
        ///  return instance of Number class with value of 0
        /// </summary>
        public static Number False { get; } = new Number(0);
        /// <summary>
        ///  return instance of Number class with value of 1
        /// </summary>
        public static Number True { get; } = new Number(1);
        /// <summary>
        ///  return instance of Number class with value of PI from Math.PI
        /// </summary>
        public static Number PI { get; } = new Number(MathF.PI);

        public bool IsInt { get; set; }
        public int IntValue { get; set; } //  1 0001 + 0001 = 0010
        public float FloatValue { get; set; }

        public Number(bool value)
        {
            IsInt = true;
            FloatValue = IntValue = value ? 1:0;
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
            IsInt = int.TryParse(value,out var intval);
            if(IsInt)
                IntValue = intval;
        }

        public override Value Add(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var number = other as Number;
                return (IsInt ? number.IsInt ? new Number(IntValue + number.IntValue) : new Number(IntValue + number.FloatValue) :
                    number.IsInt ? new Number(FloatValue + number.IntValue) : new Number(FloatValue + number.FloatValue)).SetContext(Context);
            }
            else if (typeof(StringVal).IsInstanceOfType(other))
            {
                error = null;
                var stringVal = other as StringVal;
                return new StringVal(ValueStr + stringVal.ValueStr).SetContext(Context).SetPosition(PositionStart, PositionEnd);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Sub(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var number = other as Number;
                return (IsInt ? number.IsInt ? new Number(IntValue - number.IntValue) : new Number(IntValue - number.FloatValue) :
                    number.IsInt ? new Number(FloatValue - number.IntValue) : new Number(FloatValue - number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Mul(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var number = other as Number;
                return (IsInt ? number.IsInt ? new Number(IntValue * number.IntValue) : new Number(IntValue * number.FloatValue) :
                    number.IsInt ? new Number(FloatValue * number.IntValue) : new Number(FloatValue * number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Div(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;
                if (number.IsZero)
                {
                    error = new RunTimeError(other.PositionStart, other.PositionEnd, "Division by zero", Context);
                    return null;
                }
                return (IsInt ? number.IsInt ? new Number(IntValue / number.IntValue) : new Number(IntValue / number.FloatValue) :
                    number.IsInt ? new Number(FloatValue / number.IntValue) : new Number(FloatValue / number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Pow(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                error = null;
                var number = other as Number;

                return (IsInt ? (number.IsInt ? new Number((float)Math.Pow(IntValue, number.IntValue)) : new Number((float)Math.Pow(IntValue, number.FloatValue))) :
                (number.IsInt ? new Number((float)Math.Pow(FloatValue, number.IntValue)) : new Number((float)Math.Pow(FloatValue, number.FloatValue)))).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonEE(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue == number.IntValue) : new Number(IntValue == number.FloatValue) :
                    number.IsInt ? new Number(FloatValue == number.IntValue) : new Number(FloatValue == number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonNE(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue != number.IntValue) : new Number(IntValue != number.FloatValue) :
                    number.IsInt ? new Number(FloatValue != number.IntValue) : new Number(FloatValue != number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonLT(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue < number.IntValue) : new Number(IntValue < number.FloatValue) :
                    number.IsInt ? new Number(FloatValue < number.IntValue) : new Number(FloatValue < number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonGT(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue > number.IntValue) : new Number(IntValue > number.FloatValue) :
                    number.IsInt ? new Number(FloatValue > number.IntValue) : new Number(FloatValue > number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonLTE(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue <= number.IntValue) : new Number(IntValue <= number.FloatValue) :
                    number.IsInt ? new Number(FloatValue <= number.IntValue) : new Number(FloatValue <= number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value GetComparisonGTE(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return (IsInt ? number.IsInt ? new Number(IntValue >= number.IntValue) : new Number(IntValue >= number.FloatValue) :
                    number.IsInt ? new Number(FloatValue >= number.IntValue) : new Number(FloatValue >= number.FloatValue)).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value And(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return new Number(IntValue & number.IntValue).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Or(Value other, out IError error)
        {
            if (typeof(Number).IsInstanceOfType(other))
            {
                var number = other as Number;
                error = null;

                return new Number(IntValue | number.IntValue).SetContext(Context);
            }

            error = IllegalOperation(other);
            return null;
        }
        public override Value Not(out IError error)
        {
            error = null;
            return new Number(IntValue == 0 ? 1 : 0).SetContext(Context);
        }

        public override Value Clone() => new Number(ValueStr)
            .SetContext(Context)
            .SetPosition(PositionStart, PositionEnd);
        public override bool IsTrue() => !IsZero;
        public override string Type() => "Number";
        public override bool IsZero { get => ValueStr.Equals("0"); }
        public override string ToString() => ValueStr;

    }
}
