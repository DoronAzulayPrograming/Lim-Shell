using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Values;

namespace LimOnDotNetCore.Core.Results
{
    public class RunTimeResult
    {
        public Value Value { get; set; }
        public IError Error { get; set; }
        public Value FunctionReturnValue { get; set; }
        public bool LoopShouldContinue { get; set; }
        public bool LoopShouldBreak { get; set; }

        public void Reset()
        {
            Value = null;
            Error = null;
            FunctionReturnValue = null;
            LoopShouldContinue = false;
            LoopShouldBreak = false;
        }

        public Value Register(RunTimeResult result)
        {
            Error = result.Error;
            FunctionReturnValue = result.FunctionReturnValue;
            LoopShouldContinue = result.LoopShouldContinue;
            LoopShouldBreak = result.LoopShouldBreak;
            return result.Value;
        }
        public RunTimeResult Success(Value value)
        {
            Reset();
            Value = value;
            return this;
        }
        public RunTimeResult SuccessReturn(Value value)
        {
            Reset();
            FunctionReturnValue = value;
            return this;
        }
        public RunTimeResult SuccessContinue()
        {
            Reset();
            LoopShouldContinue = true;
            return this;
        }
        public RunTimeResult SuccessBreak()
        {
            Reset();
            LoopShouldBreak = true;
            return this;
        }
        public RunTimeResult Failure(IError error)
        {
            Reset();
            Error = error;
            return this;
        }

        public bool ShouldReturn() =>
            (Error != null || FunctionReturnValue != null || LoopShouldContinue || LoopShouldBreak);
    }
}
