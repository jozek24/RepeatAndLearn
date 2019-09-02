using System.Collections.Generic;

namespace RepeatAndLearn.Model
{
    public class DictionaryResponse
    {
        public int Code { get; set; }
        public string Lang { get; set; }
        public List<string> Text { get; set; }
    }
}
