namespace DataLayer
{
    /// <summary>
    /// Test case ADO object
    /// </summary>
    public class TestCase : WorkItem
    {
        public List<AdoBug> AdoBugs { get; set; }

        public List<Requirement> Requirements { get; set; }

        public Outcome CurrentOutcome()
        {
            return new Outcome();
        }
    }
}
