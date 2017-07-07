using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class FiveChain : ChainBase
    {
        public override string CalcNextGram(ref int count, string inputWord, Model m)
        {
            var words = this.SplitOnType(inputWord);

            var fourGramMatches = m.FiveGrams.AsParallel().Where(x => x.FirstWord == words[3] && x.SecondWord
                == words[2] && x.ThirdWord == words[1] && x.FourthWord == words[0]).ToList();

            var fiveGramCounts = fourGramMatches.AsParallel().Sum(x => x.Occurences);

            if (fiveGramCounts == 0)
            {
                return Next.CalcNextGram(ref count, inputWord, m);
            }

            else
            {
                return inputWord + " " + GenerateRandomWord<FiveGram>(fourGramMatches, fiveGramCounts).FifthWord;
            }
        }
    }
}