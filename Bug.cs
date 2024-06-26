using System;

namespace TylerAPIToDevOps
{
    /// <summary>
    /// AdoBug ADO object
    /// </summary>
    public class AdoBug : WorkItem
    {
        internal string BugType { get; set; }

        internal string Configuration { get; set; }

        internal int Priority { get; set; }


        /// <summary>
        /// Prints the details of the bug.
        /// </summary>
        public void PrintBugDetails()
        {

        }
    }


}