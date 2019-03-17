using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tashan.Json
{
    partial class JsonObject
    {
        public T As<T>()
        {
            return (T)GetValue( typeof( T ) );
        }

        public void Update(object obj)
        {
            var type = obj.GetType();
            var attributeType = typeof( JsonKeyAttribute );
            foreach (var field in type.GetFields())
            {
                if (!( field.GetCustomAttributes( attributeType, false ) is JsonKeyAttribute[ ] attrs ) || attrs.Length != 1)
                {
                    continue;
                }
                var attr = attrs[ 0 ];
                if (ContainsKey( attr.KeyName ))
                {
                    Console.WriteLine( "setting file:{0}", field.Name );
                    var target = this[ attr.KeyName ];
                    var vl = target.GetValue( field.FieldType );
                    if (vl != null)
                    {
                        field.SetValue( obj, vl );
                    }
                    else
                    {
                        target.Update( field.GetValue( obj ) );
                    }
                }
            }
        }

        private object GetValue(Type type)
        {
            if (type == typeof( string ))
            {
                return Value;
            }

            var args = new Type[ ] { typeof( string ) };
            var parse = type.GetMethod( "Parse", BindingFlags.Static | BindingFlags.Public, null, args, null );
            if (parse != null)
            {
                return parse.Invoke( type, new object[ ] { Value } );
            }

            if (type.BaseType == typeof( List<bool> ).BaseType && type.GenericTypeArguments.Length == 1)
            {
                var asm = type.Assembly;
                var list = asm.CreateInstance( type.FullName );

                foreach (var pair in this)
                {
                    object vl = pair.Value.GetValue( type.GenericTypeArguments[ 0 ] );
                    if (vl != null)
                    {
                        type.InvokeMember( "Add", BindingFlags.InvokeMethod, null, list, new object[ ] { vl } );
                    }
                    else
                    {
                        var constructor = type.GenericTypeArguments[ 0 ].GetConstructor( new Type[ ] { } );
                        if (constructor == null)
                        {
                            throw new NotImplementedException();
                        }
                        vl = constructor.Invoke( new object[ ] { } );
                        pair.Value.Update( vl );
                        type.InvokeMember( "Add", BindingFlags.InvokeMethod, null, list, new object[ ] { vl } );
                    }
                }
                return list;
            }
            return null;
        }
    }
}
