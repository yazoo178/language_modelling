using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NGram
{
    class Classifier
    {

        private IEnumerable<Classifaction> _results;
        private IEnumerable<Word> PositiveWords = new List<Word>();
        private IEnumerable<Word> NegativeWords = new List<Word>();

        string goodStuff = String.Empty;
        string badStuff = String.Empty;

        public void LoadData(string _path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var lines = new List<string>();

            IEnumerable<Classifaction> _classif = new List<Classifaction>();

            using (Stream stream = assembly.GetManifestResourceStream(_path))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            _classif = lines.Select(line =>
            {
                var splitter = line.Split('\t');
                return new Classifaction()
                {
                    Word = splitter[0],
                    ProbablityInPositive = double.Parse(splitter[1]),
                    ProbablityInNegative = double.Parse(splitter[2])
                };

            });

            var file = File.ReadAllLines(@"C:\Users\Will\Downloads\opinion-lexicon-English\positive-words.txt");

            PositiveWords = file.Select((x) =>
                {
                    return new Word() { ClassType = ClassOfDocument.Positive, WordString = x };
                });

            file = File.ReadAllLines(@"C:\Users\Will\Downloads\opinion-lexicon-English\negative-words.txt");

            NegativeWords = file.Select((x) =>
            {
                return new Word() { ClassType = ClassOfDocument.Negative, WordString = x };
            });

            goodStuff = File.ReadAllText(@"C:\Users\Will\Downloads\pros-cons\IntegratedPros.txt");
            badStuff = File.ReadAllText(@"C:\Users\Will\Downloads\pros-cons\IntegratedCons.txt");

           // var word = Console.ReadLine();
           // var demoGood = (double) goodStuff.Split(' ').Count(x => x == word) + 1;
          //  var demoBad = (double)badStuff.Split(' ').Count(x => x == word) + 1;

          //  var totalco = goodStuff.Split(' ').Length + goodStuff.Split(' ').Distinct().Count() + badStuff.Distinct().Count();

          ///  Console.WriteLine("Probablity of being in good review: {0}", demoGood / totalco);
          //  Console.WriteLine("Probablity of being in bad review: {0}", demoBad / totalco);

            _results = _classif;
        }

        public ClassOfDocument GetClassOfInput(string input, bool useBooleanClassifaction)
        {
            var Words = input.Split(' ');
            var keyWords = new List<string>();

            foreach (var word in Words)
            {
                if (_results.Select(x => x.Word).Contains(word))
                {
                    keyWords.Add(word);
                }
            }

            double probOfPositive = 1;

            double probOfNegative = 1;

            foreach (var keyword in keyWords)
            {
                var classif = _results.FirstOrDefault(x => x.Word == keyword);
                if (classif == null) continue;
                probOfPositive = probOfPositive*classif.ProbablityInPositive;
                probOfNegative = probOfNegative*classif.ProbablityInNegative;
            }

            return probOfPositive > probOfNegative ? ClassOfDocument.Positive : ClassOfDocument.Negative;
        }

        public ClassOfDocument GetClassOfInputEx(string inputDocument, bool useBooleanClassifaction)
        {
            if(!useBooleanClassifaction)
            {
                goodStuff = goodStuff.Replace("<Pros>", "").Replace(",", "").Replace("</Pros>", "").ToLower();
                badStuff = badStuff.Replace("<Cons>", "").Replace(",", "").Replace("</Cons>", "").ToLower();
            }

            else
            {
                XDocument goodDoc = XDocument.Parse(goodStuff);
                XDocument badDoc = XDocument.Parse(badStuff);
                
                goodDoc.Descendants("Pros").ToList().ForEach(x =>
                    {
                        x.Value = string.Join(" ",x.Value.Split(' ').Distinct());
                    });

                badDoc.Descendants("Cons").ToList().ForEach(x =>
                {
                      x.Value = string.Join(" ", x.Value.Split(' ').Distinct());
                });

                goodStuff = goodDoc.ToString().Replace("<Pros>", "").Replace(",", "").Replace("</Pros>", "").ToLower();
                badStuff = badDoc.ToString().Replace("<Cons>", "").Replace(",", "").Replace("</Cons>", "").ToLower();
            }

            var combinedWordLookers = PositiveWords.Concat(NegativeWords).Distinct(); //Our Lexicons

            var keyWords = new List<Word>();

            foreach (var word in combinedWordLookers) //Every positive and negative word in out dataset
            {
                if (inputDocument.Contains(word.WordString)) //Check if the word is in the document
                {
                    if (!useBooleanClassifaction)
                    {
                        var timesOccur = Regex.Matches(inputDocument, word.WordString).Count; // occurences

                        while (timesOccur != 0)
                        {
                            keyWords.Add(word);
                            timesOccur--;
                        }
                    }

                    else
                    {
                        keyWords.Add(word);
                    }
                }
            }

            double numOfOfPositiveReviews = goodStuff.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Count();
            double numOfOfNegativeReviews = badStuff.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Count();

            double probOfPositive = numOfOfPositiveReviews / (numOfOfPositiveReviews + numOfOfNegativeReviews); // P(+) MLE
            double probOfNegative = numOfOfNegativeReviews / (numOfOfNegativeReviews + numOfOfPositiveReviews); // P(-) MLE

            var totalcoGood = goodStuff.Split(' ').Length;
            var totalcoBad = badStuff.Split(' ').Length;

            var vocab = goodStuff.Split(' ').Concat(badStuff.Split(' ')).Distinct().Count();

            foreach (var keyword in keyWords) //Each word we matched
            {
                var demoGood = (double)goodStuff.Split(' ').Count(x => x == keyword.WordString) + 1; //Probablity of the word occuring in a positive document + 1 smoothing
                var demoBad = (double)badStuff.Split(' ').Count(x => x == keyword.WordString) + 1; ////Probablity of the word occuring in a negative document + 1 smoothing

                probOfPositive = probOfPositive * (demoGood / (totalcoGood + vocab));
                probOfNegative = probOfNegative * (demoBad / (totalcoBad + vocab));
            }


            probOfPositive = Math.Log(probOfPositive);
            probOfNegative = Math.Log(probOfNegative);

            return probOfPositive > probOfNegative ? ClassOfDocument.Positive : ClassOfDocument.Negative;
        }

    }

    enum ClassOfDocument
    {
        Positive,
        Negative
    }

    

    class Classifaction
    {
        public string Word { get; set; }

        public double ProbablityInPositive { get; set; }

        public double ProbablityInNegative { get; set; }
    }

    
}
