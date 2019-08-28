using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace RepeatAndLearn.Model
{
    public class DictionaryResponse
    {
        public int Code { get; set; }
        public string Lang { get; set; }
        public List<string> Text { get; set; }
    }
}
