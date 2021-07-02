using LimOnDotNetCore.Core.Values;
using System.Collections.Generic;
using System.Linq;

namespace LimOnDotNetCore.Core
{
    public class SymbolTable
    {
        public Dictionary<string, Value> Symbols { get; set; }
        public SymbolTable Parent { get; set; }

        public SymbolTable()
        {
            Symbols = new Dictionary<string, Value>();
        }
        public SymbolTable(SymbolTable parent)
        {
            Parent = parent;
            Symbols = new Dictionary<string, Value>();
        }

        public Value Get(string name)
        {
            var symbol = Symbols.SingleOrDefault(s => s.Key.Equals(name));
            if (string.IsNullOrEmpty(symbol.Key) && Parent != null)
                return Parent.Get(name);

            return symbol.Value;
        }

        public void Set(string name, Value value)
        {
            var symbol = Symbols.SingleOrDefault(s => s.Key.Equals(name));
            if (string.IsNullOrEmpty(symbol.Key))
            {
                Symbols.Add(name, value);
                return;
            }
            else
            {
                if (BuilInFunction.FunctionList.Contains(name))
                    return;
            }
            Symbols.Remove(symbol.Key);
            Symbols.Add(name, value);
        }

        public void Remove(string name)
        {
            var symbol = Symbols.SingleOrDefault(s => s.Key.Equals(name));
            if (string.IsNullOrEmpty(symbol.Key)) return;
            Symbols.Remove(symbol.Key);
        }
    }
}
