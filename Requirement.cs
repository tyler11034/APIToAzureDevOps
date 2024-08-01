namespace DataLayer
{
    public class Requirement : WorkItem
    {
        public string TestVerificationMethod { get; set; }

        public string BoardColumn { get; set; }

        public List<TestCase> TestCases { get; set; }
    }
}
