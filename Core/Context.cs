
namespace LimOnDotNetCore.Core
{
    public class Context
    {
        public string DisplayName { get; set; }
        public bool IsRrepresentMode { get; set; }
        public Context Parent { get; set; }
        public Position ParentEntryPosition { get; set; }
        public SymbolTable SymbolTable { get; set; }
        public Context(string displayName, Context parent = null, Position parentEntryPosition = null)
        {
            DisplayName = displayName;
            Parent = parent;
            ParentEntryPosition = parentEntryPosition;
        }
    }
}
