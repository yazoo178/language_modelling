using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NGram
{
    class ClassifactionModel
    {
        private IEnumerable<GeneralClassifier> _classifiers;
        private Lazy<IList<Word>> _lexicons = new Lazy<IList<Word>>(() => new List<Word>());

        public IList<Word> Lexicons
        {
            get
            {
                return _lexicons.Value;
            }
        }

        public ClassifactionModel(IEnumerable<GeneralClassifier> classif, IEnumerable<string> sourceToLexicons)
        {
            this._classifiers = classif;

            foreach(var lexSource in sourceToLexicons)
            {
                if (File.Exists(lexSource))
                {
                    var words = LoadLexiconsSystemFile(lexSource).ToList();
                    words.ForEach(x => Lexicons.Add(x));
                }
            }
        }

        private IEnumerable<Word> LoadLexiconsSystemFile(string file)
        {
            var lex = File.ReadAllLines(file);

            foreach (var x in lex)
            {
                if (!Lexicons.Select(y => y.WordString).Contains(x))
                {
                    yield return new Word() { WordString = x };
                }
            }
        }

        public string ClassifyData(string dataSource)
        {
            var wordMatches = new List<Word>();

            foreach (var word in Lexicons) //Every positive and negative word in out dataset
            {
                if (dataSource.Contains(word.WordString)) //Check if the word is in the document
                {
                    var timesOccur = Regex.Matches(dataSource, word.WordString).Count; // occurences

                    while (timesOccur != 0)
                    {
                        wordMatches.Add(word);
                        timesOccur--;
                    }
                }
            }

            var vocab = string.Join(" ", _classifiers.Select(x => x.Data)).Split(' ').Distinct().Count();

            _classifiers.ToList().ForEach(x => x.CurrentProbablity = x.ProbablityOfThisClass(_classifiers.Sum(y => y.Count)));

            foreach (var keyword in wordMatches) //Each word we matched
            {
                foreach(var classif in _classifiers)
                {
                    var hitCount = classif.Data.Split(' ').Count(x => x == keyword.WordString) + 1;

                    classif.CurrentProbablity = classif.CurrentProbablity * (hitCount / (classif.Count + vocab));
                }
            }

            return _classifiers.OrderByDescending(x => x.CurrentProbablity).FirstOrDefault().Name;
            
        }
    }

    class Word
    {
        public ClassOfDocument ClassType { get; set; }

        public string WordString { get; set; }
    }
}
