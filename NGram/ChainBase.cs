using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public abstract class ChainBase : IChainItem
    {
        public abstract string CalcNextGram(ref int count, string inputWord, Model m);
        

        public ChainBase() { }

        public IChainItem Next {  get;  set; }

        protected string[] SplitOnType(string input)
        {
            var type = this.GetType();
            var splitInto = Level - 1;
            var resultString = new List<string>();
            var totalInput = input.Split(' ');

            for(int i = 1; i <= splitInto; i++)
            {
                try
                {
                    resultString.Add(totalInput[totalInput.Length - i]);
                }

                catch (Exception)
                {
                    resultString.Add(String.Empty);
                }
            }

            return resultString.ToArray();
        }

        protected T GenerateRandomWord<T>(IEnumerable<Unigram> grams, double count) where T : Unigram
        {
            WriteLogPercentageData(grams,count);
            var GramSortAsc = grams.OrderBy(x => x.Probablity(count)).ToList();
            var nextWordGram = TriGram.SelectRand(GramSortAsc, count);
            return nextWordGram as T;

        }

        protected void WriteLogPercentageData(IEnumerable<Unigram> grams, double count)
        {
            using (var wrtier = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" +"William" + ".txt"))
            {
                double sum = 0;
                foreach (var gram in grams)
                {
                    double val = gram.Probablity(count);
                    sum += val;

                    wrtier.WriteLine("Probablity of next word '{0}' - {1}", gram.NextWord(), val);
                }

                wrtier.WriteLine("---------- Sum is ---------- {0}", sum.ToString());
            }
        }
            



        public int Level { get; set; }
        
            
    }
}
