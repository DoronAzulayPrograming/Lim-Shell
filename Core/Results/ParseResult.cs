using LimOnDotNetCore.Core.Errors;
using LimOnDotNetCore.Core.Nodes;
using System.Collections.Generic;

namespace LimOnDotNetCore.Core.Results
{
    public class ParseResult
    {
        public Node Node { get; set; }
        public IError Error { get; set; }
        public int AdvanceCount { get; set; } = 0;
        public int ToReverseCont { get; set; } = 0;
        public int LastRegisterdAvanceCont { get; set; } = 0;

        public void RegisterAdvancement()
        {
            LastRegisterdAvanceCont = 1;
            AdvanceCount++;
        }
        public Node Register(ParseResult result)
        {
            LastRegisterdAvanceCont += result.AdvanceCount;
            AdvanceCount += result.AdvanceCount;
            if (result.HasError()) Error = result.Error;
            return result.Node;
        }
        public Node TryRegister(ParseResult result)
        {
            if (result.HasError())
            {
                ToReverseCont = result.AdvanceCount;
                return null;
            }
            return Register(result);
        }

        public bool HasError() => Error != null;
        public ParseResult Success(KeyValuePair<Node,bool> node)
        {
            Node = node.Key;
            return this;
        }
        public ParseResult Success(Node node)
        {
            Node = node;
            return this;
        }

        public ParseResult Failure(IError error)
        {
            if(Error == null || AdvanceCount == 0)
                Error = error;
            return this;
        }
    }
}
