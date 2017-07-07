using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class ChainStart
    {
        public static readonly double ALPHA = 0.4;

        private IChainItem _start;
        public void ConstructBasedOnNGram() { }

        public void ConsructBasedOnNGram()
        {
            _start = new FiveChain() { Level = 5, Next = new FourChain() { Level = 4
                , Next = new TriChain() { Level = 3, Next = new BiChain() { Level = 2
                    , Next = new UniChain() } } } };
        }

        public void TrancendChain(ref int count, string inputWord, Model m)
        {

            for (int i = 0; i < 100; i++)
            {
                inputWord = _start.CalcNextGram(ref count, inputWord, m);
            }

            
            Console.WriteLine(inputWord + ".");
            Console.WriteLine(String.Empty);


            if (count > 0)
            {
                count--;
                TrancendChain(ref count, inputWord.Split(' ')[0], m);
            }
        }
    }
}
