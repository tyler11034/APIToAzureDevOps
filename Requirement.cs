using System;
using System.Collections.Generic;

namespace TylerAPIToDevOps
{
    /// <summary>
    /// Requirement ADO object
    /// </summary>
    public class Requirement : WorkItem
    {
        public string TestVerificationMethod { get; set; }

        public string BoardColumn { get; set; }

        public List<TestCase> TestCases { get; set; }


        public void PrintRequirementDetails()
        {
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Title: {Title}");
            Console.WriteLine($"Assigned To: {AssignedTo}");
            Console.WriteLine($"State: {State}");
            Console.WriteLine($"Tags: {Tags}");
            Console.WriteLine($"Test Verification Method: {TestVerificationMethod}");
            Console.WriteLine($"Board Column: {BoardColumn}");
            Console.WriteLine("------------------------------------------");
        }
    }
}