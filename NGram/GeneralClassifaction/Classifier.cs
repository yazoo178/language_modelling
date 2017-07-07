using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class GeneralClassifier
    {
        public string Name { private set; get; }

        public double Count { private set; get; }

        public string Data { get; set; }

        public double CurrentProbablity { get; set; }

        public GeneralClassifier(string dataSource, string nameOfClassifier, IEnumerable<IClassifierParse> parsers)
        {
            Init(dataSource, nameOfClassifier, parsers);
        }

        public GeneralClassifier (string dataSource, string nameOfClassifier, ClassifierParseFlags parseFlags)
        {
            var parsers = ResolveClassifiersParsersFromFlags(parseFlags);
            Init(dataSource, nameOfClassifier, parsers);
        }

        public void Init(string dataSource, string nameOfClassifier, IEnumerable<IClassifierParse> parsers)
        {
            Data = File.ReadAllText(dataSource);
            parsers.ToList().ForEach(x => Data = x.Parse(Data));
            Name = nameOfClassifier;
            Count = (double)Data.Split(' ').Count();
        }


        public double ProbablityOfThisClass(double totalCounts)
        {
            return Count / totalCounts;
        }

        private static IEnumerable<IClassifierParse> ResolveClassifiersParsersFromFlags(ClassifierParseFlags flags)
        {
            foreach(var val in Enum.GetValues(typeof(ClassifierParseFlags)))
            {
                if(flags.HasFlag((Enum)val))
                {
                    var type = Type.GetType("NGram." + val.ToString());
                    var instance = Activator.CreateInstance(type, new[] { "Pros" });
                    yield return instance as IClassifierParse;
                    
                }

            }
        }
    }
}
