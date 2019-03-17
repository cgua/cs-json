using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tashan.Json
{
    public partial class JsonObject
    {
        /// <summary>
        /// 键名
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// 数据类型
        /// </summary>
        public readonly ObjectTypes Type;

        /// <summary>
        /// 键值
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// 子元素键名，字符串值 映射
        /// </summary>
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();

        /// <summary>
        /// 实例化新的 JsonObject
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值字符串</param>
        public JsonObject(string key, string value)
        {
            Key = key;
            Value = value;
            if (string.IsNullOrEmpty( value ))
            {
                Type = ObjectTypes.Empty;
                return;
            }
            var reader = new SimpleReader( value );

            Type = TestValueType( reader );
            switch (Type)
            {
                case ObjectTypes.Object:
                    ReadObject( reader );
                    break;
                case ObjectTypes.Array:
                    ReadArray( reader );
                    break;
                case ObjectTypes.String:
                    break;
                case ObjectTypes.Number:
                    break;
                case ObjectTypes.Boolean:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public JsonObject this[ string key ]
        {
            get
            {
                var value = "";
                if (_dict.ContainsKey( key ))
                {
                    value = _dict[ key ];
                }
                return new JsonObject( key, value );
            }
        }

        public JsonObject this[ int index ]
        {
            get
            {
                var key = index.ToString();
                var value = "";
                if (_dict.ContainsKey( key ))
                {
                    value = _dict[ key ];
                }
                return new JsonObject( index.ToString(), _dict[ index.ToString() ] );
            }
        }

        /// <summary>
        /// @inhertdoc
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value;

        /// <summary>
        /// 读取由 "{" 与 "}" 包围的字符串。
        /// 要求光标被放在开头的 "{" 位置上。
        /// 读取结果将包含开头的 "{" 与结尾的 "}"。
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private void ReadArray(SimpleReader reader)
        {
            const char head = '[';
            const char tail = ']';
            if (reader.Peek( false ) == null || reader.Char() != head)
            {
                throw new ParseException( "非法调用 ReadArrayValue", reader );
            }

            var key = 0;
            while (true)
            {
                var valueType = TestValueType( reader );
                string valueStr;
                switch (valueType)
                {
                    case ObjectTypes.Array:
                        valueStr = ReadPairValue( reader, '[', ']' );
                        break;
                    case ObjectTypes.Object:
                        valueStr = ReadPairValue( reader, '{', '}' );
                        break;
                    case ObjectTypes.String:
                        valueStr = '"' + ReadStringValue( reader ) + '"';
                        break;
                    case ObjectTypes.Number:
                        valueStr = ReadNumberValue( reader );
                        break;
                    default:
                        throw new NotImplementedException();
                }
                _dict.Add( key.ToString(), valueStr );
                key++;
                if (reader.Peek( true ) == null)
                {
                    break;
                }
                var c = reader.Char();
                if (c == ',')
                {
                    continue;
                }
                if (c == tail)
                {
                    break;
                }
                throw new ParseException( " ReadArrayValue 解析异常。", reader );
            }
        }

        /// <summary>
        /// 读取 true/false 字符串。
        /// 要求光标必须放在 t 或是 false 上面
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private string ReadBooleanValue(SimpleReader reader)
        {
            if (reader.TestString( "true", true ))
            {
                return "true";
            }
            else if (reader.TestString( "false", true ))
            {
                return "false";
            }
            throw new ParseException( "ReadBooleanValue异常", reader );
        }

        /// <summary>
        /// 读取数字。
        /// 要求光标必须在第一个数字之上
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private string ReadNumberValue(SimpleReader reader)
        {
            var tmp = "";
            var hasDot = false;
            foreach (var c in reader)
            {
                if( c == '-' )
                {
                    if( tmp.Length == 0)
                    {
                        tmp += c;
                        continue;
                    }
                    throw new ParseException( $"数字格式化错误:{tmp}", reader );
                }
                if( c == '.')
                {
                    if( tmp.Length > 0 && tmp[tmp.Length -1] != '-' && !hasDot )
                    {
                        hasDot = true;
                        tmp += c;
                        continue;
                    }
                    throw new ParseException( $"数字格式化错误:{tmp}", reader );
                }

                if (c >= '0' && c <= '9')
                {
                    tmp += c;
                    continue;
                }
                reader.Back();
                break;
            }
            var reg = new Regex( @"^-?\d+(\.\d+)?$" );
            if (reg.IsMatch( tmp ))
            {
                return tmp;
            }
            throw new ParseException( $"数字格式化错误:{tmp}", reader );
        }

        /// <summary>
        /// 读取字符串并存入 dict
        /// </summary>
        /// <param name="str"></param>
        /// <param name="offset"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        private void ReadObject(SimpleReader reader)
        {
            const char head = '{';
            const char tail = '}';
            if (reader.Char() != head)
            {
                throw new ParseException( "异常调用，类型不符", reader );
            }

            while (true)
            {
                reader.JumpSpace();
                var key = ReadStringValue( reader );
                if (key == null)
                {
                    break;
                }
                reader.JumpSpace();
                if (reader.Char() != ':')
                {
                    throw new ParseException( "ReadObject 找不到分隔符", reader );
                }

                var valueType = TestValueType( reader );
                string valueStr;
                switch (valueType)
                {
                    case ObjectTypes.Array:
                        valueStr = ReadPairValue( reader, '[', ']' );
                        break;
                    case ObjectTypes.Object:
                        valueStr = ReadPairValue( reader, '{', '}' );
                        break;
                    case ObjectTypes.String:
                        valueStr = '"' + ReadStringValue( reader ) + '"';
                        break;
                    case ObjectTypes.Boolean:
                        valueStr = ReadBooleanValue( reader );
                        break;
                    case ObjectTypes.Number:
                        valueStr = ReadNumberValue( reader );
                        break;
                    default:
                        throw new NotImplementedException();
                }
                _dict.Add( key, valueStr );
                if (reader.Peek( true ) == null || reader.Char() == tail)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 读取由 "{" 与 "}" 包围的字符串。
        /// 要求光标被放在开头的 "{" 位置上。
        /// 读取结果将包含开头的 "{" 与结尾的 "}"。
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private string ReadPairValue(SimpleReader reader, char head, char tail)
        {
            var lv = 0;
            var isInQuote = false;
            char? last = null;
            var sb = new StringBuilder();
            if (reader.Peek( false ) != head)
            {
                throw new ParseException( "非法调用 ReadObjectValue", reader );
            }
            foreach (var c in reader)
            {
                sb.Append( c );
                if (c == head && !isInQuote)
                {
                    lv++;
                    continue;
                }
                if (c == tail && !isInQuote)
                {
                    lv--;
                    if (lv < 1)
                    {
                        goto End;
                    }
                    continue;
                }
                if (c == '"' && ( !isInQuote || ( isInQuote && last != '\\' ) ))
                {
                    isInQuote = !isInQuote;
                    continue;
                }
            }

            throw new ParseException( "ReadObjectValue 解析异常", reader );
            End: return sb.ToString();
        }

        /// <summary>
        /// 读取由双引号["]包围的字符串内容，开头必须有 ["] 并且结果的 ["] 也将被跳过,读取内容不包含双引号
        /// 要求当标必须被放在 开头的["] 上面
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private string ReadStringValue(SimpleReader reader)
        {
            if (reader.Char() != '"')
            {
                throw new ParseException( "非法调用 ReadStringValue。", reader );
            }
            var key = new StringBuilder();
            char? last = null;
            foreach (var c in reader)
            {
                switch (c)
                {
                    case '\"':
                        if (last == '\\')
                        {
                            key.Append( '\"' );
                            break;
                        }
                        return key.ToString();
                    case '\n':
                        throw new ParseException( "ReadStringValue 解析异常。", reader );
                    case '\r':
                        throw new ParseException( "ReadStringValue 解析异常。", reader );
                    default:
                        key.Append( c );
                        break;
                }
                last = c;
            }
            throw new ParseException( "ReadStringValue 解析异常。", reader );
        }

        /// <summary>
        /// 测试值的类型,光标将会跳过空白，但不会跳过值开始第一个字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="offset"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private ObjectTypes TestValueType(SimpleReader reader)
        {
            var c = reader.Peek( true );
            switch (c)
            {
                case '[':
                    return ObjectTypes.Array;
                case '{':
                    return ObjectTypes.Object;
                case '\"':
                    return ObjectTypes.String;
                default:
                    if (c == 't' && reader.TestString( "true", false ))
                    {
                        return ObjectTypes.Boolean;
                    }
                    else if (c == 'f' && reader.TestString( "false", false ))
                    {
                        return ObjectTypes.Boolean;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        return ObjectTypes.Number;
                    }
                    break;
            }

            throw new ParseException( "无法识别的类型", reader );
        }
    }
}
