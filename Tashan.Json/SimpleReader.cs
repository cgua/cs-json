using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tashan.Json
{
    class SimpleReader:IEnumerable<char>
    {
        /// <summary>
        /// 文本长度
        /// </summary>
        private readonly int _length;

        /// <summary>
        /// 文本内容
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// 当前游标位置
        /// </summary>
        private int _cursor = 0;

        /// <summary>
        /// 实例化新的简单的 JSON 专用读取器
        /// </summary>
        /// <param name="str"></param>
        public SimpleReader(string str)
        {
            Source = str;
            _length = str.Length;
        }

        /// <summary>
        /// 列号
        /// </summary>
        public int Column { get; private set; } = 0;

        /// <summary>
        /// 行号
        /// </summary>
        public int Line { get; private set; } = 0;

        /// <summary>
        /// 跳过空白字符 " " , "\r" , "\n"
        /// </summary>
        public void JumpSpace()
        {
            while(_cursor < _length)
            {
                switch (Source[ _cursor ])
                {
                    case '\n':
                        MoveLine();
                        break;
                    case '\r':
                        MoveChar();
                        break;
                    case ' ':
                        MoveChar();
                        break;
                    case '\t':
                        MoveChar();
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// 获取下一个字符，光标将会跳过空白字符，但不会跳过其它字符
        /// </summary>
        /// <returns></returns>
        public char? Peek(bool ignoreSpace)
        {
            if(ignoreSpace)
            {
                JumpSpace();
            }
            if( _cursor < _length) {
                return Source[ _cursor ];
            }
            return null;
        }

        public char Char()
        {
            if (_cursor < _length)
            {
                _cursor++;
                return Source[ _cursor - 1 ];
            }
            throw new Exception( "out of range." );
        }

        /// <summary>
        /// 移动一次光标
        /// </summary>
        private void MoveChar(int count = 1)
        {
            _cursor += count;
            Column += count;
        }

        /// <summary>
        /// 称动光标并换行
        /// </summary>
        private void MoveLine()
        {
            _cursor++;
            Line++;
            Column = 0;
        }

        /// <summary>
        /// 测试接下来的单词是否与给定的单词相同
        /// </summary>
        /// <param name="str">给定的单词</param>
        /// <param name="move">当测试成功时，是否同时移动光标</param>
        /// <returns></returns>
        public bool TestString(string str, bool move )
        {
            var cursor = _cursor;
            foreach(var c in str)
            {
                if( c != Source[ cursor ])
                {
                    return false;
                }
                cursor++;
            }
            if(move)
            {
                _cursor = cursor;
            }
            return true;
        }

        /// <summary>
        /// 获取游标附近的字符串
        /// </summary>
        /// <returns></returns>
        public string Near
        {
            get
            {
                var cursor = _cursor - 20;
                cursor = cursor >= 0 ? cursor : 0;
                var sb = new StringBuilder();
                while (cursor < _length && sb.Length < 60)
                {
                    var c = Source[ cursor ];
                    if (cursor == _cursor)
                    {
                        sb.Append( "+++" );
                        sb.Append( c );
                        sb.Append( "+++" );
                    }
                    else
                    {
                        sb.Append( c );
                    }
                    cursor++;
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 回退一格。
        /// 注癔：回退换行将会导至　行号与列号　整体失真。不到万不行已请不要回退换行
        /// </summary>
        public void Back()
        {
            _cursor--;
            Column--;
            if(_cursor < 0 )
            {
                throw new Exception( "out of range." );
            }
        }

        /// <summary>
        /// 遍历剩下的字符。
        /// 此方法会移动光标。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<char> GetEnumerator()
        {
            while (_cursor < _length)
            {
                if (Source[ _cursor ] == '\n')
                {
                    MoveLine();
                }
                else
                {
                    MoveChar();
                }
                yield return Source[ _cursor - 1 ];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
