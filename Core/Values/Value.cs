using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Results;
using System;
using System.Collections.Generic;

namespace LimOnDotNetCore.Core.Values
{
    public class Value
    {
        public string ValueStr { get; set; }
        public Context Context { get; set; }
        public Position PositionStart { get; set; }
        public Position PositionEnd { get; set; }

        public Value SetPosition(Position positionStart, Position positionEnd)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
            return this;
        }
        public Value SetContext(Context context)
        {
            Context = context;
            return this;
        }

        public virtual Value Add(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Sub(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Mul(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Div(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Pow(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }

        public virtual Value GetComparisonEE(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value GetComparisonNE(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value GetComparisonLT(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        
        public virtual Value GetComparisonGT(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value GetComparisonLTE(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value GetComparisonGTE(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
               
        public virtual Value And(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Or(Value other, out IError error)
        {
            error = IllegalOperation(other);
            return null;
        }
        public virtual Value Not(out IError error)
        {
            error = IllegalOperation(this);
            return null;
        }

        public virtual RunTimeResult Execute(IEnumerable<Value> args)
        {
            return new RunTimeResult().Failure(IllegalOperation(this));
        }
        public virtual Value Clone() => throw new Exception("Non clone method defiend");
        public virtual string Type() => "";
        public virtual bool IsTrue() => false;
        public virtual bool IsZero { get => false; }

        public virtual string ToRrepresent() => string.Empty;

        public virtual RunTimeError IllegalOperation(Value other)
        {
            if (other != null)
            {
                other = this;
                return null;
            }

            return new RunTimeError(PositionStart, PositionEnd, "Illegal operation", Context);
        }
    }
}
