using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NGram
{
    public class ConfusionMatrix
    {
        private IEnumerable<SpellingError> _results;

        public void LoadData(string _path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var lines = new List<string>();

            IEnumerable<SpellingError> Results = new List<SpellingError>();

            using (Stream stream = assembly.GetManifestResourceStream(_path))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            Results = lines.Select(line =>
            {
                var splitter = line.Replace(",", "").Split(' ');
                var resultCol = splitter.ToList();
                resultCol.Remove(splitter[0]);
                return new SpellingError(splitter[0].Replace(":", ""), resultCol);

            });

            
            _results = Results;
        }

        public double GetTransposistionCount(string letterOne, string letterTwo)
        {
            if (letterTwo.Length != 1 || letterOne.Length != 1)
            {
                throw new InvalidOperationException("String must be length of 1");
            }

            //Returns all miss-spellings where the correct word contains letterOne + letterTwo
            return _results.Where(x => x.CorrectWord.Contains(letterOne + letterTwo))
                .SelectMany(y => y.Errors).Count(z => z.Contains(letterTwo + letterOne));
        }
    }

    public class SpellingError : IEnumerable<string>
    {
        public string CorrectWord { get; set; }

        private readonly IList<string> _errors = new List<string>();

        public IEnumerable<string> Errors
        {
            get { return _errors; }
        }

        public SpellingError(string _correct, IEnumerable<string> errors )
        {
            CorrectWord = _correct;
            _errors = errors.ToList();
        }

        public void Add(string error)
        {
            _errors.Add(error);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _errors.GetEnumerator();
        }
    }
}
