using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class TranspositionOneEditDistance : IOneEditDistance
    {
        public IEnumerable<Unigram> GetCandidates(string input, Model m)
        {
            var allWords = m.UniGrams.ToList();
            var stringResultList = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                var currentLetter = input[i];
                var nextLetter = i == input.Length - 1 ? (char?)null : input[i + 1];

                var inputAsArray = input.ToCharArray();

                if (nextLetter != null)
                {
                    inputAsArray[i] = nextLetter.Value;
                    inputAsArray[i + 1] = currentLetter;

                    var resultString = new String(inputAsArray);
                    if (allWords.Select(x => x.FirstWord).Contains(resultString))
                    {
                        stringResultList.Add(resultString);
                    }
                }
            }

            return allWords.Where(x => stringResultList.Contains(x.FirstWord));
        }
    }
}
