using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NGram
{
    class XmlClassiferParse : IClassifierParse
    {
        private string _tag { get; set; }
        private IEnumerable<string> otherTags = null;

        public XmlClassiferParse(string parseTag)
        {
            this._tag = parseTag;
        }

        public XmlClassiferParse (string parseTag, IEnumerable<string> otherTagsToClear) : this (parseTag)
        {
            otherTags = otherTagsToClear;
        }

        public string Parse(string _path)
        {
            XDocument data = XDocument.Parse(_path);

            data.Descendants(_tag).ToList().ForEach(x =>
            {
                x.Value = string.Join(" ", x.Value.Split(' ').Distinct());
            });


            var returnData = data.ToString().Replace(string.Format("<{0}>", _tag), "")
                .Replace(",", "").Replace(string.Format("</{0}>", _tag), "").ToLower();

            if (otherTags != null)
            {
                foreach(var val in otherTags)
                {
                    returnData = returnData.Replace(string.Format("<{0}>", val), "").Replace(string.Format("</{0}>", val), "");
                }
            }

            return returnData;
        
        }
    }
}
