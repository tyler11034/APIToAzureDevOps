namespace TylerAPIToDevOps
{
    class QueryConstants
    {
        // Query to pull all Work Item data
        public const string WorkItemWiqlQuery = @"
            SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [System.Tags], [System.TeamProject], [System.WorkItemType], [Custom.Round], [Microsoft.VSTS.Common.Discipline], [Custom.RequirementCategory], [Custom.3e6a7048-fa00-4b92-890c-5f56c824ea24]
            FROM WorkItems
            WHERE [System.WorkItemType] = 'Requirement' AND [System.TeamProject] = 'Electronic Health Record (EHR)'";

        // Query to pull all Bug data
        public const string BugWiqlQuery = @"
            SELECT [System.Id], [System.Title], [System.State], [Custom.BugType], [Microsoft.VSTS.Common.Priority]
            FROM WorkItems
            WHERE [System.WorkItemType] = 'Bug' AND [System.TeamProject] = 'Electronic Health Record (EHR)'";

        // Query to pull all Test Case data
        public const string TestCaseWiqlQuery = @"
            SELECT [System.Id], [System.Title], [System.State]
            FROM WorkItems
            WHERE [System.WorkItemType] = 'Test Case' AND [System.TeamProject] = 'Electronic Health Record (EHR)'";

        // Query to pull all Requirement data
        public const string RequirementWiqlQuery = @"
            SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [Custom.TestVerificationMethod], [System.BoardColumn]
            FROM WorkItems
            WHERE [System.WorkItemType] = 'Requirement' AND [System.TeamProject] = 'Electronic Health Record (EHR)'";

        // Query to pull all Outcome data
        public const string OutcomeWiqlQuery = @"
            SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State], [Custom.CurrentOutcome], [System.ChangedDate], [System.ChangedBy]
            FROM WorkItems
            WHERE [System.WorkItemType] = 'Test Case' AND [System.TeamProject] = 'Electronic Health Record (EHR)'";
    }
}
