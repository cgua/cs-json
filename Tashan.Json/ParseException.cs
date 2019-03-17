using System;

namespace Tashan.Json
{
    internal class ParseException:Exception
    {
        public ParseException(string msg, SimpleReader reader)
            : base( $"{msg}，行：{reader.Line}，列：{reader.Column} .\n附近内容：{reader.Near}\n源内容：{reader.Source}\n" )
        {
        }
    }
}
