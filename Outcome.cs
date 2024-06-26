using System;

namespace TylerAPIToDevOps
{

    /// <summary>
    /// Test outcome ADO object
    /// </summary>
    public class Outcome
    {
        public string RunBy { get; set; }

        public string Tester { get; set; }
        public string TestOutcome { get; set; }  // called Outcome in ADO

        public DateTime TimeStamp { get; set; }
    }
}