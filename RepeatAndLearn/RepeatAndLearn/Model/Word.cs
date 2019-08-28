using System;

namespace RepeatAndLearn.Model
{
    class Word
    {
        public int IdWord { get; set; }
        public string PlWord { get; set; }
        public string EnWord { get; set; }
        public DateTime DateOfNextRepeat { get; set; }
        public int CurrentAmountOfRepeats { get; set; }
        public int TotalAmountOfRepeats { get; set; }
    }
}