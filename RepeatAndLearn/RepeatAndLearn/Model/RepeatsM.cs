using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeatAndLearn.Model
{
    class RepeatsM
    {
        public int DaysToNextRepeat(int totalRepeats, int currentRepeats)
        {
            if (totalRepeats <= 1)
            {
                if (currentRepeats <= 2)
                    return 5;

                return 2;
            }
            else if (totalRepeats >= 2 && totalRepeats <= 5)
            {
                if (currentRepeats <= 2)
                    return 14;
                return 10;
            }
            else if (totalRepeats >= 6 && totalRepeats <= 9)
            {
                if (currentRepeats <= 2)
                    return 31;
                return 18;
            }
            else
            {
                if (currentRepeats < 2)
                    return 100;
                return 365;
            }
        }
    }
}
