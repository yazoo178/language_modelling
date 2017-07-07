using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NGram
{
    static class Ex
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Action<TSource, TSource> OnAddFailed)
        {
            Dictionary<TKey, TSource> seenKeys = new Dictionary<TKey, TSource>();
            
            foreach (TSource element in source)
            {
                var key = keySelector(element);

                if (seenKeys.ContainsKey(key))
                {
                    OnAddFailed(seenKeys[keySelector(element)], element);
                }

                else
                {
                    seenKeys.Add(keySelector(element), element);
                    yield return element;
                }
            }
        }

        public static IEnumerable<string> Tokenize(this string text, string separators)
        {
            int startIdx = 0;
            int currentIdx = 0;

            while (currentIdx < text.Length)
            {
                // found a separator?
                if (separators.Contains(text[currentIdx]))
                {
                    // yield a substring, if it's not empty
                    if (currentIdx > startIdx)
                        yield return text.Substring(startIdx, currentIdx - startIdx);

                    // yield the separator
                    yield return text.Substring(currentIdx, 1);

                    // mark the beginning of the next token
                    startIdx = currentIdx + 1;
                }

                currentIdx++;
            }
        }
    }
    public class Model
    {
        public Model()
        {
            FourGrams = new List<FourGram>();
            TriGrams = new List<TriGram>();
            BiGrams = new List<BiGram>();
            UniGrams = new List<Unigram>();
            FiveGrams = new List<FiveGram>();

        }

        private Lazy<double> uniGrams;

        public double SumOfUnigrams
        {
            get { return uniGrams.Value; }
        }

        private Lazy<double> biGrams;

        public double SumOfBigrams
        {
            get { return biGrams.Value; }
        }

        public void CleanUpNGrams()
        {
            
            BiGrams = BiGrams.DistinctBy(x => x.FirstWord + x.SecondWord, (y, z) => y.Occurences +=z.Occurences).ToList();
            UniGrams = UniGrams.DistinctBy(x => x.FirstWord, (y, z) => y.Occurences += z.Occurences).ToList();
            TriGrams = TriGrams.DistinctBy(x => x.FirstWord + x.SecondWord + x.ThirdWord, (z, y) => y.Occurences += z.Occurences).ToList();
            FourGrams = FourGrams.DistinctBy(x => x.FirstWord + x.SecondWord + x.ThirdWord + x.FourthWord, (z, y) => y.Occurences += z.Occurences).ToList();

            uniGrams = new Lazy<double>(() => UniGrams.Sum(x => x.Occurences));
            biGrams = new Lazy<double>(() => BiGrams.Sum(x => x.Occurences));
        }

        public IList<FourGram> FourGrams { get; private set; }
        public IList<TriGram> TriGrams { get; private set; }
        public IList<BiGram> BiGrams { get; private set; }

        public IList<Unigram> UniGrams { get; private set; }

        public IList<FiveGram> FiveGrams { get; private set; }

    }

    class Program
    {
       
        static Model GenerateModel()
        {
            
            //var result = TriGram.PoissionDist(3, 1);

            var model = new Model();
            
            var assembly = Assembly.GetExecutingAssembly();
            var lines = new List<string>();
            var resourceName = "NGram.w5_.txt";
            int ngram = 5;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }



            foreach(var line in lines)
            {
                var words = line.Split('\t');
                
                model.FiveGrams.Add(new FiveGram()
                    {
                        Occurences = double.Parse(words[0]),
                        FirstWord = words[1],
                        SecondWord = words[2],
                        ThirdWord = words[3],
                        FourthWord = words[4],
                        FifthWord = words[5]
                    });

                model.FourGrams.Add(new FourGram()
                {
                    Occurences = double.Parse(words[0]),
                    FirstWord = words[1],
                    SecondWord = words[2],
                    ThirdWord = words[3],
                    FourthWord = words[4]
                });

                model.FourGrams.Add(new FourGram()
                {
                    Occurences = double.Parse(words[0]),
                    FirstWord = words[2],
                    SecondWord = words[3],
                    ThirdWord = words[4],
                    FourthWord = words[5]
                });

                for (int i = 1; i < ngram - 1; i++)
                {
                    model.TriGrams.Add(new TriGram()
                    {
                        Occurences = double.Parse(words[0]),
                        FirstWord = words[i],
                        SecondWord = words[i + 1],
                        ThirdWord = words[i + 2]
                    });
                }

                for (int i = 1; i < ngram; i++)
                {
                    model.BiGrams.Add(new BiGram()
                    {
                        Occurences = double.Parse(words[0]),
                        FirstWord = words[1],
                        SecondWord = words[i + 1]
                    });
                }

                double oc = double.Parse(words[0]);
                foreach(var word in words)
                {
                    double d = 0;
                    if(!double.TryParse(word, out d))
                    {
                        model.UniGrams.Add(new Unigram() { FirstWord = word, Occurences = oc });
                    }
                }

            }

            model.CleanUpNGrams();
            return model;
        }

        /// <summary>
        /// Assumes a bigram model
        /// </summary>
        /// <param name="m"></param>
        /// <param name="inputWord"></param>
        static void GenerateTestPerplexity(Model m, string inputWord)
        {
            var inputWords = inputWord.Split(' ');
            double probablity = 1;

            for(int i = 1; i < inputWords.Length; i ++)
            {
                var first = inputWords[i - 1];
                var of = inputWords[i];

                var elements = m.BiGrams.Where(x => x.FirstWord == first);
                var sum = elements.Sum(x => x.Occurences);

                var bigram = m.BiGrams.FirstOrDefault(x => x.FirstWord == first && x.SecondWord == of);

                probablity *= bigram.Probablity(sum);
                
            }

            Console.WriteLine(1 / NthRoot(probablity, inputWords.Length));

        }

        static double NthRoot(double A, int N)
        {
            return Math.Pow(A, 1.0 / N);
        }

        public static string LinearRegression(LinearRegressionModel mod, double xValue)
        {

            var set = mod.DataSet;

            var xMean = set.Sum(x => x.X) / set.Count();

            var yMean = set.Sum(x => x.Y) / set.Count();

            var difference = new List<Tuple<double, double, double>>();

            set.ToList().ForEach(x =>
            {
                difference.Add(new Tuple<double, double, double>(x.X - xMean, x.Y - yMean
                    ,Math.Pow((x.X - xMean), 2)));
            });


            var b1 = difference.Sum(x => x.Item1 * x.Item2) / difference.Sum(y => y.Item3);
            var b0 = yMean - (xMean * b1);

            return String.Format("Expected grade:{0}", b0 + (b1) * xValue);

        }

        static void Main(string[] args)
        {
            //var Conf = new ConfusionMatrix();
            //Conf.LoadData("NGram.spell-errors.txt");

            var linMod = new LinearRegressionModel();
           // linMod.SetDataSet("{1,2}|{2,4}|{3,5}|{4,4}|{5,5}|{6,51}|{7,55}|{8,59}|{9,63}");
             linMod.SetDataSet("{1,2}|{2,3}|{3,4}|{4,5}|{5,6}");
            Console.WriteLine(linMod.Formula);
            Console.WriteLine(linMod.RegressionLineEstimate(8));
            Console.WriteLine(linMod.RSquared());
            Console.WriteLine(linMod.StandardError());
            //return;

            WebClient client = new WebClient();
            var data = client.DownloadStringTaskAsync("http://www.nytimes.com/2016/07/22/movies/review-star-trek-beyond.html");
                

           // var cla = new Classifier();
            //cla.LoadData("NGram.KeyWords.txt");

          // var result = cla.GetClassOfInputEx(
           //    "i thought the product was quite bad. it had some poor features like dynamic zoom camera and auto clean up. there were some other intresting things like the screen was of broken quality", true);



           var clas = new List<GeneralClassifier>()
            {
                new GeneralClassifier(@"C:\Users\Will\Downloads\pros-cons\IntegratedPros.txt", "Positive", new List<IClassifierParse>(){ new XmlClassiferParse("Pros", new List<string>() { "goodreviews"})}),
                new GeneralClassifier(@"C:\Users\Will\Downloads\pros-cons\IntegratedCons.txt", "Negative", new List<IClassifierParse>(){ new XmlClassiferParse("Cons", new List<string>() { "goodreviews"})})
            };

            var cm = new ClassifactionModel(clas, new List<string>(){ @"C:\Users\Will\Downloads\opinion-lexicon-English\positive-words.txt", @"C:\Users\Will\Downloads\opinion-lexicon-English\negative-words.txt"});

            var rm1 =  cm.ClassifyData("i thought the product was quite bad. it had some poor features like dynamic zoom camera and auto clean up. there were some other intresting things like the screen was of broken quality");

            var modelTask = new Func<Task<Model>>(async () =>
                {
                    var mo = await Task.Run(() => GenerateModel());
                    return mo;
                }).Invoke();

            

            string inputWord = Console.ReadLine();
            
            Mode m = inputWord.EndsWith(@"/B") ? Mode.Bigram : Mode.Trigram;
            var resourceName = String.Format("NGram.w{0}_.txt", m == Mode.Bigram ? "2" : "3");
            inputWord = inputWord.Split('/')[0];

            string dot = ".";
            while(!modelTask.IsCompleted)
            {
                
                Console.WriteLine("Loading Data" + dot);
                dot = dot.Length == 5 ? "." : dot + ".";
                Thread.Sleep(500);
                Console.Clear();

            }

            
            var model = modelTask.Result;

            var sentanceTest = inputWord.Split(' ');

            var demo = new SpellChecker().PerformSpellCheck(inputWord, model);

            Console.WriteLine(demo);


            /*
            foreach(var val in demo)
            {
                Console.WriteLine("Did you mean? - {0}", val.FirstWord);
            }

            var bigramMatches = model.BiGrams.Where(x => x.FirstWord == sentanceTest[0] && demo.Select(y => y.FirstWord).Contains(x.SecondWord)).OrderByDescending(z => z.Occurences);
            var nextBigGramMatches = model.BiGrams.Where(x => x.SecondWord == sentanceTest[2] && demo.Select(y => y.FirstWord).Contains(x.FirstWord)).OrderByDescending(z => z.Occurences);

            var sum = bigramMatches.Sum(x => x.Occurences);

            foreach (var biMatch in bigramMatches)
            {
                Console.WriteLine("Outcome likelihood - {0}", biMatch.Probablity(sum) * nextBigGramMatches.Where(x => x.FirstWord == biMatch.SecondWord).FirstOrDefault().Probablity(sum));
            }

            */
           // return;
           // var gramsOnSpellCheck = model.BiGrams.Where(x => demo.Select(y => y.FirstWord).Contains(x.SecondWord));



           // GenerateTestPerplexity(model, inputWord) ;
          //  return;
            //GenerateTestPerplexity(model, inputWord);
          //  return;

            if (m == Mode.Bigram)
            {
                var firstWordMatchBigrams = model.BiGrams.Where(x => x.FirstWord == inputWord);
                var totalFirstWordMatches = firstWordMatchBigrams.Sum(x => x.Occurences);


                firstWordMatchBigrams.ToList().ForEach((x) =>
                {
                    Console.WriteLine(String.Format("Proablity of Next Word being {0} = {1}", x.SecondWord, x.Occurences / totalFirstWordMatches));
                });

            }


                
        
            else
            {
                int co = 5;
                var chain = new ChainStart();
                chain.ConsructBasedOnNGram();
                chain.TrancendChain(ref co, inputWord, model);
                //GenerateTriGram(ref co, inputWord, model);
            }
            
        }

        public static void GenerateTriGram( 
             ref int count, string inputWord, Model m)
        {
            for (int i = 0; i < 100; i ++)
            {

                var inputWords = inputWord.Split(' ');

                //Take the last two words
                var previousWord = inputWords[inputWords.Length - 1];
                var previousWordNext = inputWords.Length > 1 ? inputWords[inputWords.Length - 2] : String.Empty;

                //var mostLikely = new TriGram() { Occurences = -1 };

                //Get the trigrams where the last two words match
                var triGramMatches = m.TriGrams.AsParallel().Where(x => x.FirstWord == previousWordNext && x.SecondWord == previousWord).ToList();
                var triGramCounts = triGramMatches.AsParallel().Sum(x => x.Occurences);

                if (triGramCounts == 0)
                {
                    //If we couldn't get any, then just get the trigram where the last word matched (n -1)
                    var biGramMatches = m.BiGrams.AsParallel().Where(x => x.FirstWord == previousWord).ToList();

                    var biGramMatchCount = biGramMatches.Sum(x => x.Occurences);

                    if (biGramMatchCount == 0)
                    {
                        return;
                    }

                    var biGramSortAsc = biGramMatches.OrderBy(x => x.Probablity(biGramMatchCount)).ToList();

                    var nextWordBiGram = TriGram.SelectRand(biGramSortAsc, biGramMatchCount);

                    inputWord = inputWord + " " + nextWordBiGram.SecondWord;

                    continue;
                    
                }


                //Sort the probablies from low to high
                var triGramSortedAsc = triGramMatches.OrderBy(x => x.Probablity(triGramCounts)).ToList();

                /*
                triGramMatches.ToList().ForEach((x) =>
                {
                    //double probablity = x.Occurences / triGramCounts;
                   // mostLikely = probablity > mostLikely.Occurences / triGramCounts ? x : mostLikely;
                    //Console.WriteLine(String.Format("Proablity of Next Word being {0} = {1}", x.ThirdWord, probablity));

                });
                 */
                var nextWordGram = TriGram.SelectRand(triGramSortedAsc, triGramCounts);
                inputWord = inputWord + " " + ((nextWordGram.FirstWord == previousWord) ? nextWordGram.SecondWord : nextWordGram.ThirdWord);
            }

            using (var writer = File.CreateText(Environment.CurrentDirectory + string.Format(@"\output{0}.txt", DateTime.Now.Second)))
            {
                writer.Write(inputWord);
            }
            Console.WriteLine(inputWord + ".");
            Console.WriteLine(String.Empty); ;

            if (count > 0)
            {
                count--;
                GenerateTriGram(ref count, inputWord.Split(' ')[0], m);
            }
        }

        
    }

    

    public class Unigram
    {
        public string FirstWord { get; set; }

        public double Occurences { get; set; }

        /// <summary>
        /// Caluclates the trigram proablity dividing the trigram count (Occurences) by the total bigram count (counts, first two words)
        /// </summary>
        /// <param name="counts"></param>
        /// <returns></returns>
        public double Probablity (double counts)
        {
            return Occurences / counts;
        }

        public string NextWord()
        {
            var type = this.GetType();

            if(type.BaseType != typeof(object))
            {
                var prop = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name.Contains("Word"));
                return prop.GetValue(this) as string;
            }

            return FirstWord;
        }

        public double SumOfUnigrams(IEnumerable<Unigram> grams)
        {
            return grams.Sum(x => x.Occurences);
        }

        
    }

    public class FourGram : TriGram
    {
        public string FourthWord { get; set; }
    }

    public class FiveGram : FourGram
    {
        public string FifthWord { get; set; }
    }

    public class BiGram : Unigram
    {
        public string SecondWord { get; set; }

    }

    public class TriGram : BiGram
    {
        public string ThirdWord { get; set; }

        public static T SelectRand<T>(IList<T> grams, double total) where T : Unigram
        {
            Random rand = new Random();

            var randNum = rand.NextDouble();

            double sum = 0;

            foreach (var element in grams)
            {
                if ((sum = sum + element.Probablity(total)) >= randNum)
                {
                    return element;
                }
            }

            return null;


        }

        public static double PoissionDist(double mean, double value)
        {
            double result = 0;
            for (double i = value; i >= 0; i--)
            {
                var mul = Math.Pow(mean, i);
                var o = Math.Pow(Math.E, -mean);

                double fact = Factorial(i);

                result += (mul * (o / fact));
            }

            return result;
        }


        static double Factorial(double i)
        {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }
    }

    enum Mode
    {
        Trigram,
        Bigram
    }
}
