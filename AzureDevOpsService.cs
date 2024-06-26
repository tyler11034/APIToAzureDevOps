using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace TylerAPIToDevOps
{
    public class AzureDevOpsService
    {
        private readonly string _organization;
        private readonly string _projectName;
        private readonly string _personalAccessToken;
        private readonly HttpClient _httpClient;

        // Constructor 
        public AzureDevOpsService(string organization, string projectName, string personalAccessToken)
        {
            _organization = organization;
            _projectName = projectName;
            _personalAccessToken = personalAccessToken;
            _httpClient = CreateHttpClient();
            Console.WriteLine("Connection made to DevOps");
        }


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

        // executes a WIQL (Work Item Query Language) query
        private async Task<JObject> ExecuteWiqlQueryAsync(string wiqlQuery)
        {
            try
            {
                var content = new StringContent(wiqlQuery, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_projectName}/_apis/wit/wiql?api-version=6.0", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JObject>(responseBody);
                }
                else
                {
                    Console.WriteLine($"Failed to execute WIQL query. Status code: {response.StatusCode}");
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error details: {errorDetails}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while executing WIQL query: {ex.Message}");
            }

            return null;
        }

        // retrieves details for a specific work item
        private async Task<JObject> GetWorkItemDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workitems/{id}?api-version=6.0");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JObject>(responseBody);
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

        // Get WorkItems data
        public async Task<List<WorkItem>> QueryWorkItemsAsync(string wiqlQuery)
        {
            var workItems = new List<WorkItem>();

            var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
            if (queryResult == null || queryResult["workItems"] == null)
                return workItems;

            var ids = new List<int>();
            foreach (var item in queryResult["workItems"])
            {
                ids.Add(item["id"].Value<int>());
            }

            foreach (var id in ids)
            {
                var workItemDetails = await GetWorkItemDetailsAsync(id);
                if (workItemDetails != null)
                {
                    var workItem = new WorkItem
                    {
                        Id = workItemDetails["id"].Value<int>(),
                        Title = workItemDetails["fields"]["System.Title"].Value<string>(),
                        AssignedTo = workItemDetails["fields"]["System.AssignedTo"]?["displayName"]?.Value<string>(),
                        State = workItemDetails["fields"]["System.State"]?.Value<string>(),
                        Tags = workItemDetails["fields"]["System.Tags"]?.Value<string>(),
                        Discipline = workItemDetails["fields"]["Microsoft.VSTS.Common.Discipline"]?.Value<string>(),
                        OriginalRequirementId = workItemDetails["fields"]["Custom.3e6a7048-fa00-4b92-890c-5f56c824ea24"]?.Value<string>(),
                        RequirementCategory = workItemDetails["fields"]["Custom.RequirementCategory"]?.Value<string>(),
                        Round = workItemDetails["fields"]["Custom.Round"]?.Value<string>()
                    };
                    workItems.Add(workItem);
                }
            }

            return workItems;
        }

        // Get Bugs data
        public async Task<List<AdoBug>> QueryBugsAsync(string bugWiqlQuery)
        {
            var bugs = new List<AdoBug>();

            var queryResult = await ExecuteWiqlQueryAsync(bugWiqlQuery);
            if (queryResult == null || queryResult["workItems"] == null)
                return bugs;

            var ids = new List<int>();
            foreach (var item in queryResult["workItems"])
            {
                ids.Add(item["id"].Value<int>());
            }

            foreach (var id in ids)
            {
                var workItemDetails = await GetWorkItemDetailsAsync(id);
                if (workItemDetails != null)
                {
                    var bugDetails = new AdoBug
                    {
                        // Populate AdoBug properties here
                    };
                    bugs.Add(bugDetails);
                }
            }

            return bugs;
        }

        //Get TestCases Data
        public async Task<List<TestCase>> QueryTestCasesAsync(string testCaseWiqlQuery)
        {
            var testCases = new List<TestCase>();

            var queryResult = await ExecuteWiqlQueryAsync(testCaseWiqlQuery);
            if (queryResult == null || queryResult["workItems"] == null)
                return testCases;

            var ids = new List<int>();
            foreach (var item in queryResult["workItems"])
            {
                ids.Add(item["id"].Value<int>());
            }

            foreach (var id in ids)
            {
                var workItemDetails = await GetWorkItemDetailsAsync(id);
                if (workItemDetails != null)
                {
                    var testCase = new TestCase
                    {
                        // Populate TestCase properties here
                    };
                    testCases.Add(testCase);
                }
            }

            return testCases;
        }

        //Get Requirements Data
        public async Task<List<Requirement>> QueryRequirementsAsync(string requirementWiqlQuery)
        {
            var requirements = new List<Requirement>();

            var queryResult = await ExecuteWiqlQueryAsync(requirementWiqlQuery);
            if (queryResult == null || queryResult["workItems"] == null)
                return requirements;

            var ids = new List<int>();
            foreach (var item in queryResult["workItems"])
            {
                ids.Add(item["id"].Value<int>());
            }

            foreach (var id in ids)
            {
                var workItemDetails = await GetWorkItemDetailsAsync(id);
                if (workItemDetails != null)
                {
                    var requirement = new Requirement
                    {
                        Id = workItemDetails["id"].Value<int>(),
                        Title = workItemDetails["fields"]["System.Title"].Value<string>(),
                        AssignedTo = workItemDetails["fields"]["System.AssignedTo"]?["displayName"]?.Value<string>(),
                        State = workItemDetails["fields"]["System.State"]?.Value<string>(),
                        Tags = workItemDetails["fields"]["System.Tags"]?.Value<string>(),
                        TestVerificationMethod = workItemDetails["fields"]["Microsoft.VSTS.Common.TestVerificationMethod"]?.Value<string>(),
                        BoardColumn = workItemDetails["fields"]["Custom.BoardColumn"]?.Value<string>()
                        // Add more fields as needed
                    };
                    requirements.Add(requirement);
                }
            }

            return requirements;
        }

        // Get Outcomes data
        public async Task<List<Outcome>> QueryOutcomesAsync(string outcomeWiqlQuery)
        {
            var outcomes = new List<Outcome>();

            var queryResult = await ExecuteWiqlQueryAsync(outcomeWiqlQuery);
            if (queryResult == null || queryResult["workItems"] == null)
                return outcomes;

            var ids = new List<int>();
            foreach (var item in queryResult["workItems"])
            {
                ids.Add(item["id"].Value<int>());
            }

            foreach (var id in ids)
            {
                var workItemDetails = await GetWorkItemDetailsAsync(id);
                if (workItemDetails != null)
                {
                    var outcome = new Outcome
                    {
                        RunBy = workItemDetails["fields"]["Microsoft.VSTS.Common.RunBy"]?.Value<string>(),
                        Tester = workItemDetails["fields"]["Microsoft.VSTS.Common.Tester"]?.Value<string>(),
                        TestOutcome = workItemDetails["fields"]["Microsoft.VSTS.Common.Outcome"]?.Value<string>(),
                        TimeStamp = workItemDetails["fields"]["System.ChangedDate"]?.Value<DateTime>() ?? DateTime.MinValue
                    };
                    outcomes.Add(outcome);
                }
            }

            return outcomes;
        }

        //print list of all available fields
        public async Task ListFieldsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/fields?api-version=6.0");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var fields = JsonConvert.DeserializeObject<JObject>(responseBody);

                    Console.WriteLine("Available fields:");
                    foreach (var field in fields["value"])
                    {
                        Console.WriteLine($"Name: {field["name"]}, Reference Name: {field["referenceName"]}, Type: {field["type"]}");
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
    }
}
