using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class SubsitutionOneEditDistance : IOneEditDistance
    {
        public IEnumerable<Unigram> GetCandidates(string input, Model m)
        {
            var words = m.UniGrams;

            var alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var stringResultList = new List<string>();

            for(int i = 0; i < input.Length; i ++)
            {
                foreach(var letter in alpha)
                {
                    var val = input.ToCharArray();
                    val[i] = letter;
                    var result = new String(val);

                    if (words.Select(x => x.FirstWord).Contains(result))
                    {
                        stringResultList.Add(result);
                    }
                }
            }

            return words.Where(x => stringResultList.Contains(x.FirstWord));
        }
    }
}
