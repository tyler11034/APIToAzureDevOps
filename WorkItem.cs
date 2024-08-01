namespace DataLayer
{
    public class WorkItem
    {
        public string AssignedTo { get; set; }

        public int Id { get; set; }

        public string Discipline { get; set; }

        public string OriginalRequirementId { get; set; }

        public string RequirementCategory { get; set; }

        public string Round { get; set; }

        public string State { get; set; }

        public string Tags { get; set; }

        public string Title { get; set; }

        public string WorkItemType { get; set; }

        public string TeamProject { get; set; }
    }
}