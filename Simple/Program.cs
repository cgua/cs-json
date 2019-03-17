using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Properties;
using Tashan.Json;

namespace Simple
{
    class PrettyLog
    {
        [JsonKey( KeyName = "Tone" )]
        public float One= default(float);

        [JsonKey( KeyName = "Tbool" )]
        public bool isTrue;

        [JsonKey(KeyName ="emitters")]
        public List<Emitter> Emitters = new List<Emitter>();

        public List<string> Listener = new List<string>();

        public List<string> Formatter = new List<string>();

        [JsonKey(KeyName ="project-path")]
        public string ProjectPath;

        public override string ToString()
        {
            return $"ProjectPath:{ProjectPath}\nemitter:{Emitters}\nListener:{Listener}\nFormatter:{Formatter}\n";
        }
    }

    class Emitter
    {
        [JsonKey(KeyName ="name")]
        public string Name;

        [JsonKey(KeyName ="prefix")]
        public string Prefix;

        [JsonKey(KeyName ="listeners")]
        public List<string> Listeners;
    }

    class Program
    {
        static void Main(string[ ] args)
        {
            var str = Encoding.UTF8.GetString( Resources.client_log );
            var conf = new PrettyLog();
            var obj = new JsonObject( null,  str );

            obj.Update(conf);
            Console.WriteLine( conf );

            Console.ReadKey();
        }
    }
}
