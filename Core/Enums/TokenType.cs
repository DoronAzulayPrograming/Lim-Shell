namespace LimOnDotNetCore.Core.Enums
{
    public enum TokenType
    {
        Identifier,        //   asd
        Keyword,           //   var,func,for,while,if,then,elif,else
        EQ,                //   =
        String,            //   "text"
        Int,               //   1
        Float,             //   1.0
        Plus,              //   +
        Minus,             //   -
        Mul,               //   *
        Div,               //   /
        Pow,               //   ^
        Lparem,            //   (
        Rparem,            //   )
        LSQparem,          //   ]
        RSQparem,          //   [
        EE,                //   ==
        NE,                //   !=
        GT,                //   >
        LT,                //   <
        GTE,               //   >=
        LTE,               //   <=
        Comma,             //   ,
        Arrow,             //   ->
        NewLine,           //   ;
        Eof,               //   end of file
    }
}
