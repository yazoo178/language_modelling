using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class BiChain : ChainBase
    {
        public override string CalcNextGram(ref int count, string inputWord, Model m)
        {
            var words = this.SplitOnType(inputWord);

            var biGramMatches = m.BiGrams.AsParallel().Where(x => x.FirstWord == words[0]).ToList();

            var biGramMatchCount = biGramMatches.Sum(x => x.Occurences);
            
            if (biGramMatchCount == 0)
            {
                return Next.CalcNextGram(ref count, inputWord, m);
            }

            else
            {
                return inputWord + " " + GenerateRandomWord<BiGram>(biGramMatches, biGramMatchCount).SecondWord;
            }
        }
    }
}
