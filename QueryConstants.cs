using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TylerAPIToDevOps
{
    class QueryConstants
    {
        // Original query with no custom fields
        // WIQL stands for (Work Item Query Language)
        public const string WiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags]
                            FROM WorkItems
                            WHERE [System.WorkItemType] = 'Requirement'""
            }";

        // Query to pull all Work Item data
        public const string WorkItemWiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], [Microsoft.VSTS.Common.Discipline], [Custom.3e6a7048-fa00-4b92-890c-5f56c824ea24], [Custom.RequirementCategory], [Custom.Round]
                           FROM WorkItems
                           WHERE [System.WorkItemType] = 'Requirement'""
            }";

        // Query to pull all Bug data
        public const string BugWiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], [Custom.BugType], [Custom.Configuration], [Microsoft.VSTS.Common.Priority]
                            FROM WorkItems
                            WHERE [System.WorkItemType] = 'Bug'""
            }";

        // Query to pull all Test Case data
        public const string TestCaseWiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], [Microsoft.VSTS.Common.Discipline], [Custom.RequirementCategory], [Custom.Round]
                           FROM WorkItems
                           WHERE [System.WorkItemType] = 'TestCase'""
            }";

        // Query to pull all Requirement data
        public const string RequirementWiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], 
                            [Microsoft.VSTS.Common.TestVerificationMethod], [Custom.BoardColumn]
                            FROM WorkItems
                            WHERE [System.WorkItemType] = 'Requirement'""
            }";

        // Query to pull all Outcome data
        public const string OutcomeWiqlQuery = @"
            {
                ""query"": ""SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], 
                            [Microsoft.VSTS.Common.RunBy], [Microsoft.VSTS.Common.Tester], [Microsoft.VSTS.Common.Outcome], [System.ChangedDate]
                            FROM WorkItems
                            WHERE [System.WorkItemType] = 'Test Case Outcome'""
            }";
    }
}
