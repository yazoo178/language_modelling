using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class FourChain : ChainBase
    {
        public  override string  CalcNextGram(ref int count, string inputWord, Model m)
        {
            var words = this.SplitOnType(inputWord);

            var fourGramMatches = m.FourGrams.AsParallel().Where(x => x.FirstWord == words[2] && x.SecondWord
                == words[1] && x.ThirdWord == words[0]).ToList();

            var fourGramCounts = fourGramMatches.AsParallel().Sum(x => x.Occurences);

            if (fourGramCounts == 0)
            {
                return Next.CalcNextGram(ref count, inputWord, m);
            }

            else
            {
                return inputWord + " " + GenerateRandomWord<FourGram>(fourGramMatches, fourGramCounts).FourthWord;
            }
        }
    }
}
