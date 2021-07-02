namespace LimOnDotNetCore.Core
{
    public class Position
    {
        public string FileName { get; set; }
        public string FileText { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Position()
        {
            Index = -1;
            Line = -1;
            Column = -1;
        }
        public Position(int index,int line,int column, string fileName, string fileText)
        {
            Index = index;
            Line = line;
            Column = column;
            FileName = fileName;
            FileText = fileText;
        }

        public Position Advance(char current_char = '\0')
        {
            Index++;
            Column++;

            if(current_char == '\n')
            {
                Line++;
                Column = 0;
            }

            return this;
        }

        public Position Clone() => new Position(Index, Line, Column, FileName, FileText);
    }
}
