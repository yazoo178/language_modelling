using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    class SpellChecker
    {
        private readonly IList<IOneEditDistance> validators = new List<IOneEditDistance>();

        public SpellChecker()
        {
            validators.Add(new DeletionOneEditDistance());
            validators.Add(new InsertionOneEditDistance());
            validators.Add(new SubsitutionOneEditDistance());
            validators.Add(new TranspositionOneEditDistance());
        }

    
        public IEnumerable<Unigram> GetCandidates(string word, Model m)
        {
            IEnumerable<Unigram> result = new List<Unigram>();

            foreach(var vali in validators)
            {
                result = vali.GetCandidates(word, m).Concat(result);
            }

            return result;
        }

        private bool NeedsCorrecting(string word, Model m)
        {
            return !m.UniGrams.Select(x => x.FirstWord).Contains(word);
        }

        private string GetMostLikelyWord(IEnumerable<Unigram> grams, string[] originalInputWords, Model m, int indexOfOriginalWord)
        {
            try
            {
                //Returns bigrams where previous word is one word before our corrected word, and the second word is one of the change candidates
                var bigramMatches = m.BiGrams.AsParallel().Where(x => x.FirstWord == originalInputWords[indexOfOriginalWord - 1]
                    && grams.Select(y => y.FirstWord).Contains(x.SecondWord)).OrderByDescending(z => z.Occurences);

                var preMatchCount = bigramMatches.Sum(x => x.Occurences);
                double probablity = 0;

                try
                {
                    var bigramMatchesAfter = m.BiGrams.AsParallel().Where(x => x.SecondWord == originalInputWords[indexOfOriginalWord + 1]
                    && grams.Select(y => y.FirstWord).Contains(x.FirstWord)).OrderByDescending(z => z.Occurences);

                    var postMatchCount = bigramMatchesAfter.Sum(x => x.Occurences);
                    string mostLikely = null;

                    foreach (var bi in bigramMatches)
                    {
                        if (!bigramMatchesAfter.Any())
                        {
                            throw new Exception();
                        }
                        foreach (var afterbigGram in bigramMatchesAfter)
                        {
                            if  (bi.Probablity(postMatchCount)*afterbigGram.Probablity(postMatchCount) > probablity)
                            {
                                probablity = bi.Probablity(postMatchCount)*afterbigGram.Probablity(postMatchCount);
                                mostLikely = bi.SecondWord;
                            }
                        }
                    }

                    return mostLikely;
                }
                catch (Exception)
                {
                    probablity = 0;
                    foreach (var value in bigramMatches)
                    {
                        if (probablity < value.Probablity(preMatchCount))
                        {
                            probablity = value.Probablity(preMatchCount);
                        }
                    }
                    //If we cannot get a bigram match then we simply take the most likely unigram
                    return bigramMatches.FirstOrDefault(x => x.Probablity(preMatchCount) == probablity).SecondWord;
                }

            }
            catch (Exception)
            {
                return grams.OrderByDescending(x => x.Probablity(m.UniGrams.Sum(y => y.Occurences))).Select(z => z.FirstWord).FirstOrDefault();
            }
        }

        public string PerformSpellCheck(string input, Model m)
        {
            var words = input.Split(' ');

            for (int i = Int32.MaxValue; i < words.Length; i ++ )
            {
                if (NeedsCorrecting(words[i], m))
                {
                    var possibleWords = GetCandidates(words[i], m).ToList();

                    if (possibleWords.Any())
                    {
                        words[i] = GetMostLikelyWord(possibleWords, words, m, i);
                    }
                }
                
            }

            for(int  i = 0; i < words.Length; i++)
            {
                CorrectGrammar(input, m, i, words);
            }

            return String.Join(" ", words);
        }

        private void CorrectGrammar(string input, Model model, int i, string[] words)
        {
            double countOfTotalWords = model.SumOfUnigrams;
            if (i == 0 || words.Length <= i + 1)
            {
                return;
            }
            else
            {
                var wordwithCandidates = new Dictionary<string, Unigram[]>();

                for (int j = i - 1; j <= i + 1; j++)
                {
                    wordwithCandidates.Add(words[j], GetCandidates(words[j], model).ToArray());
                }

                double probablity = 0;
                IList<string> mostLikely = null;

                foreach (var keyP in wordwithCandidates.First().Value)
                {
                    foreach (var val in wordwithCandidates[words[i]])
                    {
                        foreach (var valTwo in wordwithCandidates[words[i + 1]])
                        {
                            double localProbablity = keyP.Probablity(countOfTotalWords)*
                                                     (val.Probablity(countOfTotalWords)*
                                                     valTwo.Probablity(countOfTotalWords));

                            if (localProbablity > probablity)
                            {
                                probablity = localProbablity;
                                mostLikely = new List<string>() { keyP.FirstWord, val.FirstWord, valTwo.FirstWord};
                            }
                        }
                    }

                }


                words[i - 1] = mostLikely[0];
                words[i] = mostLikely[1];
                words[i + 1] = mostLikely[2];

            }
        }        
     
    }
}
