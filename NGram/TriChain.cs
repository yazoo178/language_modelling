using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class TriChain : ChainBase
    {
        public override string CalcNextGram(ref int count, string inputWord, Model m)
        {
            var words = this.SplitOnType(inputWord);

            var triGramMatches = m.TriGrams.AsParallel().Where(x => x.FirstWord == words[1] && x.SecondWord == words[0]).ToList();
            var triGramCounts = triGramMatches.AsParallel().Sum(x => x.Occurences);

            if(triGramCounts == 0)
            {
                return Next.CalcNextGram(ref count, inputWord, m);
            }
            else
            {
                return inputWord + " " + GenerateRandomWord<TriGram>(triGramMatches, triGramCounts).ThirdWord;
            }

        }
        
    }
}
