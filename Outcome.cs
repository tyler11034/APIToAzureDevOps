namespace DataLayer
{
    public class Outcome
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }
        public string State { get; set; }
        public string CurrentOutcome { get; set; }
        public DateTime ChangedDate { get; set; }

        public string ChangedBy { get; set; }
    }
}
