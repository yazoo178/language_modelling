using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public interface IChainItem
    {
        string CalcNextGram(ref int count, string inputWord, Model m);

        IChainItem Next { get; set; }

        int Level { get; set; }
    }
}
