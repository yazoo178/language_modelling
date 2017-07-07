using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class UniChain : ChainBase
    {
        public override string CalcNextGram(ref int count, string inputWord, Model m)
        {
            var uniGrams = m.UniGrams;
            
            var total = m.UniGrams.Sum(x => x.Occurences);
            
            return inputWord + " " + GenerateRandomWord<Unigram>(uniGrams, total).FirstWord;
        }

    }
}
