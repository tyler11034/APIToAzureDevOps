using System.Net.Http.Headers;
using System.Text;
using DataLayer;
using System.Text.Json;

namespace TylerAPIToDevOps
{
    public class AzureDevOpsService
    {
        #region Fields
        private readonly string _organization;
        private readonly string _projectName;
        private readonly string _personalAccessToken;
        private readonly HttpClient _httpClient;
        #endregion

        #region Constructor
        // Constructor 
        public AzureDevOpsService(string organization, string projectName, string personalAccessToken)
        {
            _organization = organization;
            _projectName = projectName;
            _personalAccessToken = personalAccessToken;
            _httpClient = CreateHttpClient();
            Console.WriteLine("Connection made to DevOps");
        }
        #endregion

        #region HttpClient Setup
        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"https://dev.azure.com/{_organization}/")
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return httpClient;
        }
        #endregion

        #region WIQL Query Execution
        // Executes a WIQL (Work Item Query Language) query
        private async Task<JsonDocument> ExecuteWiqlQueryAsync(string wiqlQuery)
        {
            var content = new StringContent($"{{\"query\": \"{wiqlQuery}\"}}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_projectName}/_apis/wit/wiql?api-version=6.0", content);
                response.EnsureSuccessStatusCode(); // Throws an exception for non-success status codes
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(responseBody);
            }
            catch (HttpRequestException ex) when (response != null)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to execute WIQL query. Status code: {response.StatusCode}, Error details: {errorDetails}");
                throw new CustomAzureDevOpsException($"HttpRequestException: {ex.Message}", ex, response.StatusCode, errorDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while executing WIQL query: {ex.Message}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }
        #endregion

        #region Work Item Details
        // Retrieves details for a specific work item
        public async Task<JsonDocument> GetWorkItemDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workitems/{id}?api-version=6.0");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(responseBody);
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve work item details. Status code: {response.StatusCode}");
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error details: {errorDetails}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving work item details: {ex.Message}");
            }

            return null;
        }
        #endregion

        #region Query Work Items
        // Get WorkItems data
        public async Task<List<WorkItem>> QueryWorkItemsAsync(string wiqlQuery)
        {
            var workItems = new List<WorkItem>();

            var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
            if (queryResult == null)
                return workItems;

            if (queryResult.RootElement.TryGetProperty("workItems", out var workItemsElement))
            {
                var ids = workItemsElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32());

                var tasks = ids.Select(id => GetWorkItemDetailsAsync(id)).ToList();

                var workItemDetailsResults = await Task.WhenAll(tasks);

                foreach (var workItemDetails in workItemDetailsResults)
                {
                    if (workItemDetails != null)
                    {
                        var workItem = CreateWorkItem(workItemDetails);
                        workItems.Add(workItem);
                    }
                }
            }

            return workItems;
        }

        private static WorkItem CreateWorkItem(JsonDocument workItemDetails)
        {
            var fields = workItemDetails.RootElement.GetProperty("fields");
            return new WorkItem
            {
                Id = workItemDetails.RootElement.GetProperty("id").GetInt32(),
                Title = fields.GetProperty("System.Title").GetString(),
                AssignedTo = fields.TryGetProperty("System.AssignedTo", out var assignedToElement) ? assignedToElement.GetProperty("displayName").GetString() : null,
                State = fields.TryGetProperty("System.State", out var stateElement) ? stateElement.GetString() : null,
                Tags = fields.TryGetProperty("System.Tags", out var tagsElement) ? tagsElement.GetString() : null,
                TeamProject = fields.GetProperty("System.TeamProject").GetString(),
                WorkItemType = fields.GetProperty("System.WorkItemType").GetString(),
                Round = fields.TryGetProperty("Custom.Round", out var roundElement) ? roundElement.GetString() : null,
                Discipline = fields.TryGetProperty("Microsoft.VSTS.Common.Discipline", out var disciplineElement) ? disciplineElement.GetString() : null,
                RequirementCategory = fields.TryGetProperty("Custom.RequirementCategory", out var requirementCategoryElement) ? requirementCategoryElement.GetString() : null,
                OriginalRequirementId = fields.TryGetProperty("Custom.3e6a7048-fa00-4b92-890c-5f56c824ea24", out var originalRequirementIdElement) ? originalRequirementIdElement.GetString() : null
            };
        }
        #endregion

        #region Query Bugs
        private async Task<JsonDocument> GetBugDetailsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workItems/{id}?api-version=6.0");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        // Get Bugs data
        public async Task<List<AdoBug>> QueryBugsAsync(string bugWiqlQuery)
        {
            var bugs = new List<AdoBug>();

            try
            {
                var queryResult = await ExecuteWiqlQueryAsync(bugWiqlQuery);
                if (queryResult == null || !queryResult.RootElement.TryGetProperty("workItems", out var workItemsElement))
                    return bugs;

                var ids = workItemsElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToList();
                var tasks = ids.Select(id => GetBugDetailsAsync(id)).ToList();
                var bugDetailsResults = await Task.WhenAll(tasks);

                foreach (var bugDetails in bugDetailsResults)
                {
                    if (bugDetails != null)
                    {
                        var bug = CreateBug(bugDetails);
                        bugs.Add(bug);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while querying bugs: {ex.Message}");
            }

            return bugs;
        }

        private static AdoBug CreateBug(JsonDocument bugDetails)
        {
            var fields = bugDetails.RootElement.GetProperty("fields");
            return new AdoBug()
            {
                Id = bugDetails.RootElement.GetProperty("id").GetInt32(),
                Title = bugDetails.RootElement.GetProperty("fields").GetProperty("System.Title").GetString(),
                State = bugDetails.RootElement.GetProperty("fields").GetProperty("System.State").GetString(),
                BugType = bugDetails.RootElement.GetProperty("fields").TryGetProperty("Custom.BugType", out var bugTypeElement) ? bugTypeElement.GetString() : null,
                Priority = bugDetails.RootElement.GetProperty("fields").TryGetProperty("Microsoft.VSTS.Common.Priority", out var priorityElement) ? priorityElement.GetInt32() : 0
            };
        }
        #endregion

        #region Query Test Cases
        private static TestCase CreateTestCase(JsonDocument testCaseDetails)
        {
            var fields = testCaseDetails.RootElement.GetProperty("fields");
            return new TestCase
            {
                Id = testCaseDetails.RootElement.GetProperty("id").GetInt32(),
                Title = fields.GetProperty("System.Title").GetString(),
                State = fields.TryGetProperty("System.State", out var stateElement) ? stateElement.GetString() : null,
            };
        }

        // Get TestCases data
        public async Task<List<TestCase>> QueryTestCasesAsync(string wiqlQuery)
        {
            var testCases = new List<TestCase>();

            var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
            if (queryResult == null)
                return testCases;

            if (queryResult.RootElement.TryGetProperty("workItems", out var workItemsElement))
            {
                var ids = workItemsElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32());

                var tasks = ids.Select(id => GetTestCaseDetailsAsync(id)).ToList();

                var testCaseDetailsResults = await Task.WhenAll(tasks);

                foreach (var testCaseDetails in testCaseDetailsResults)
                {
                    if (testCaseDetails != null)
                    {
                        var testCase = CreateTestCase(testCaseDetails);
                        testCases.Add(testCase);
                    }
                }
            }

            return testCases;
        }

        private async Task<JsonDocument> GetTestCaseDetailsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workItems/{id}?api-version=6.0");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }
        #endregion

        #region Query Outcomes
        private static Outcome CreateOutcome(JsonDocument outcomeDetails)
        {
            var fields = outcomeDetails.RootElement.GetProperty("fields");

            return new Outcome
            {
                Id = outcomeDetails.RootElement.TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0,

                Title = TryGetStringProperty(fields, "System.Title"),

                AssignedTo = TryGetStringProperty(fields, "System.AssignedTo", "displayName"),

                State = TryGetStringProperty(fields, "System.State"),

                CurrentOutcome = TryGetStringProperty(fields, "Custom.CurrentOutcome"),

                ChangedDate = fields.TryGetProperty("System.ChangedDate", out var changedDateElement)
                    ? DateTime.TryParse(changedDateElement.GetString(), out var date) ? date : DateTime.MinValue
                    : DateTime.MinValue,

                ChangedBy = TryGetStringProperty(fields, "System.ChangedBy"),
            };
        }

        private static string TryGetStringProperty(JsonElement fields, string propertyName, string subPropertyName = null)
        {
            if (fields.TryGetProperty(propertyName, out var propertyElement))
            {
                if (subPropertyName != null)
                {
                    // Handle cases where the property is an object with a sub-property
                    if (propertyElement.ValueKind == JsonValueKind.Object && propertyElement.TryGetProperty(subPropertyName, out var subPropertyElement))
                    {
                        return subPropertyElement.GetString();
                    }
                    else
                    {
                        Console.WriteLine($"Expected property '{propertyName}' to be an object with sub-property '{subPropertyName}', but found: {propertyElement.ValueKind}");
                        return null;
                    }
                }
                else
                {
                    // Handle simple string properties
                    return propertyElement.ValueKind == JsonValueKind.String ? propertyElement.GetString() : null;
                }
            }
            return null;
        }

        // Get Outcomes data
        public async Task<List<Outcome>> QueryOutcomesAsync(string wiqlQuery)
        {
            var outcomes = new List<Outcome>();

            try
            {
                var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
                if (queryResult == null)
                {
                    Console.WriteLine("Query result is null.");
                    return outcomes;
                }

                if (queryResult.RootElement.TryGetProperty("workItems", out var workItemsElement))
                {
                    var ids = workItemsElement.EnumerateArray().Select(item => item.TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0).ToList();
                    var tasks = ids.Select(id => GetOutcomeDetailsAsync(id)).ToList();

                    var outcomeDetailsResults = await Task.WhenAll(tasks);

                    foreach (var outcomeDetails in outcomeDetailsResults)
                    {
                        if (outcomeDetails != null)
                        {
                            try
                            {
                                var outcome = CreateOutcome(outcomeDetails);
                                outcomes.Add(outcome);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error creating outcome from details: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Outcome details are null.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No 'workItems' property found in query result.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying outcomes: {ex.Message}");
            }

            return outcomes;
        }

        private async Task<JsonDocument> GetOutcomeDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workItems/{id}?api-version=6.0");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(responseBody);
                }
                else
                {
                    Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching outcome details for ID {id}: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Query Requirements
        private static Requirement CreateRequirement(JsonDocument requirementDetails)
        {
            var fields = requirementDetails.RootElement.GetProperty("fields");
            return new Requirement
            {
                Id = requirementDetails.RootElement.GetProperty("id").GetInt32(),
                Title = fields.GetProperty("System.Title").GetString(),
                AssignedTo = fields.TryGetProperty("System.AssignedTo", out var assignedToElement) ? assignedToElement.GetProperty("displayName").GetString() : null,
                State = fields.TryGetProperty("System.State", out var stateElement) ? stateElement.GetString() : null,
                TestVerificationMethod = fields.TryGetProperty("Custom.TestVerificationMethod", out var testVerificationMethodElement) ? testVerificationMethodElement.GetString() : null,
                BoardColumn = fields.TryGetProperty("System.BoardColumn", out var boardColumnElement) ? boardColumnElement.GetString() : null
            };
        }

        public async Task<List<Requirement>> QueryRequirementsAsync(string wiqlQuery)
        {
            var requirements = new List<Requirement>();

            var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
            if (queryResult == null)
                return requirements;

            if (queryResult.RootElement.TryGetProperty("workItems", out var workItemsElement))
            {
                var ids = workItemsElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32());

                var tasks = ids.Select(id => GetRequirementDetailsAsync(id)).ToList();

                var requirementDetailsResults = await Task.WhenAll(tasks);

                foreach (var requirementDetails in requirementDetailsResults)
                {
                    if (requirementDetails != null)
                    {
                        var requirement = CreateRequirement(requirementDetails);
                        requirements.Add(requirement);
                    }
                }
            }

            return requirements;
        }

        private async Task<JsonDocument> GetRequirementDetailsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workItems/{id}?api-version=6.0");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }
        #endregion

        #region List Fields
        // Print list of all available fields
        public async Task ListFieldsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/fields?api-version=6.0");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseBody);

                    Console.WriteLine("Available fields:");
                    foreach (var field in jsonDocument.RootElement.GetProperty("value").EnumerateArray())
                    {
                        var name = field.GetProperty("name").GetString();
                        var referenceName = field.GetProperty("referenceName").GetString();
                        var type = field.GetProperty("type").GetString();

                        Console.WriteLine($"Name: {name}, Reference Name: {referenceName}, Type: {type}");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve fields. Status code: {response.StatusCode}");
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error details: {errorDetails}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving fields: {ex.Message}");
            }
        }
        #endregion
    }
}
