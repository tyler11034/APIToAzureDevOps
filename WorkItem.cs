using System;

namespace TylerAPIToDevOps
{
    /// <summary>
    /// Base class for all ADO work item objects
    /// </summary>
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



        /// <summary>
        /// Prints the details of the work item.
        /// </summary>
        public void PrintWorkItemDetails()
        {
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Title: {Title}");
            Console.WriteLine($"Assigned To: {AssignedTo}");
            Console.WriteLine($"Discipline: {Discipline}");
            Console.WriteLine($"Original Requirement ID: {OriginalRequirementId}");
            Console.WriteLine($"Requirement Category: {RequirementCategory}");
            Console.WriteLine($"Round: {Round}");
            Console.WriteLine($"State: {State}");
            Console.WriteLine($"Tags: {Tags}");
            Console.WriteLine("------------------------------------------");
        }
    }


}