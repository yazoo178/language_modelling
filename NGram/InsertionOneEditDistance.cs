using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class InsertionOneEditDistance : IOneEditDistance
    {
        public IEnumerable<Unigram> GetCandidates(string input, Model m)
        {
            var allWords = m.UniGrams.ToList();
            var stringResultList = new List<string>();

            for(int i = 0; i < input.Length; i++)
            {
                var testWord = input.Remove(i, 1);
                if(allWords.Select(x => x.FirstWord).Contains(testWord))
                {
                    stringResultList.Add(testWord);
                }
            }

            return allWords.Where(x => stringResultList.Contains(x.FirstWord));
        }
    }
}
