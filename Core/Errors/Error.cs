using LimOnDotNetCore.Extentions;
using System;

namespace LimOnDotNetCore.Core.Errors
{
    public class Error : IError
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public Position PositionStart { get; set; }
        public Position PositionEnd { get; set; }

        public Error(Position positionStart, Position positionEnd, string name, string details)
        {
            Name = name;
            Details = details;
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override string ToString()
        {
            var result = $"\n{Name}: {Details}\n";
            result += $"File {PositionStart.FileName}, Line {PositionStart.Line+1}, Column {PositionStart.Column +1}";
            result += $"\n\n {StringWithArrows(PositionStart.FileText, PositionStart,PositionEnd)}\n";
            return result;
        }

        protected string StringWithArrows(string text, Position pos_start, Position pos_end)
        {
            string result = "";

            // Calculate indexes
            int index_start = Math.Max(text.IndexOf('\n', pos_start.Index), 0);
            int index_end = text.IndexOf('\n', index_start + 1);
            if (index_end < 0) index_end = text.Length-1;

            // Generate each line
            int line_count = pos_end.Line - pos_start.Line + 1;
            for (int i = 0; i < line_count; i++)
            {
                // Calculate line columns
                string line = text.SmSubstring(index_start, index_end).Replace("\t","").Replace("\r\n","").Replace("\n","");
                if (string.IsNullOrWhiteSpace(line.Replace("\n", "").Replace("\r", "").Trim()))
                {
                    // Re-calculate indexes
                    index_start = index_end;
                    index_end = text.IndexOf('\n', index_start + 1);
                    if (index_end < 0) index_end = text.Length;
                    i--;
                    continue;
                }
                int col_start = 0;
                int col_end = line.Length;
                if (i == 0)
                    col_start = pos_start.Column;
                if (i == line_count - 1)
                    col_end = pos_end.Column;

                // Append to result
                result += line + '\n';
                for (int j = 0; j <= col_start; j++) result += ' ';
                for (int j = 0; j < col_end - col_start; j++) result += '^';
                //result += '^';

                // Re-calculate indexes
                index_start = index_end;
                index_end = text.IndexOf('\n', index_start + 1);
                if (index_end < 0) index_end = text.Length;
            }
            while (text.Contains('\t')) text = text.Replace("\t", string.Empty);

            return result;
        }
    }

    public class ExpectedCharError : Error
    {
        public ExpectedCharError(Position positionStart, Position positionEnd, string details)
            :base(positionStart, positionEnd, "Expected Character", details)
        {
        }
    }
    public class IllegalCharError : Error
    {
        public IllegalCharError(Position positionStart, Position positionEnd, string details)
            :base(positionStart, positionEnd, "Illegal Character", details)
        {
        }
    }
    public class InvalidSyntaxError : Error
    {
        public InvalidSyntaxError(Position positionStart, Position positionEnd, string details)
            :base(positionStart, positionEnd, "Invalid Syntax", details)
        {
        }
    }
    public class RunTimeError : Error
    {
        public Context Context { get; set; }
        public RunTimeError(Position positionStart, Position positionEnd, string details, Context context)
            :base(positionStart, positionEnd, "RunTime Error", details)
        {
            Context = context;
        }

        public override string ToString()
        {
            string result = $"\n{GenerateTraceback()}";
            result += $"\n{Name}: {Details}\n";
            result += $"\n\n {StringWithArrows(PositionStart.FileText, PositionStart, PositionEnd)}\n";
            return result;
        }

        private string GenerateTraceback()
        {
            string result = "";
            var pos = PositionStart;
            var ctx = Context;

            while(ctx != null)
            {
                result += $"  File {pos.FileName}, Line {pos.Line + 1}, Column {pos.Column + 1}, in {ctx.DisplayName}\n" + result;
                pos = ctx.ParentEntryPosition;
                ctx = ctx.Parent;
            }
            return "Traceback (most recent call list)" + result;
        }
    }

}
