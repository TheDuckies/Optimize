using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversionOptimizer
{
    class Metrics
    {
        public static int notStarted,
                          inProgress,
                          onHold,
                          onHoldException,
                          waitingForReview,
                          finished,
                          totalTests,
                          totalMacros;
        

        public Metrics(int tests, int macros)
        {
            totalTests = tests;
            totalMacros = macros;
        }
    }
}
