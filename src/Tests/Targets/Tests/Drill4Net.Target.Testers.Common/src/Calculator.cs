using System;

namespace Drill4Net.Target.Testers.Common
{
    public class Calculator
    {
        public int FirstNumber { get; set; }
        public int SecondNumber { get; set; }

        /*************************************************************/

        public int Add()
        {
            return FirstNumber + SecondNumber;
        }

        public int Substract()
        {
            return FirstNumber - SecondNumber;
        }
    }
}
