using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;


namespace TylerAPIToDevOps
{
    class TestBatchCollection
    {
        private readonly string _organization;
        private readonly string _projectName;
        private readonly string _personalAccessToken;
        private readonly HttpClient _httpClient;

        public TestBatchCollection(string organization, string projectName, string personalAccessToken)
        {
            _organization = organization;
            _projectName = projectName;
            _personalAccessToken = personalAccessToken;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"https://dev.azure.com/{_organization}/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            Console.WriteLine("Connection made to DevOps");
        }

        // Collect WorkItems data from Azure Devops Connection in batches
        public async Task<List<WorkItem>> QueryWorkItemsAsync(string wiqlQuery)
        {
            var workItems = new List<WorkItem>();
            const int batchSize = 100; // Fetch 100 items per batch

            try
            {
                var queryResult = await ExecuteWiqlQueryAsync(wiqlQuery);
                if (queryResult != null && queryResult["workItems"] != null)
                {
                    var ids = queryResult["workItems"].Select(item => item["id"].Value<int>()).ToList();
                    for (int i = 0; i < ids.Count; i += batchSize)
                    {
                        var batchIds = ids.Skip(i).Take(batchSize).ToList();
                        var batchResults = await GetWorkItemsBatchAsync(batchIds);
                        workItems.AddRange(batchResults);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while querying work items: {ex.Message}");
            }

            return workItems;
        }

        // Helper method to execute WIQL query asynchronously
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

        // Helper method to fetch work items in a batch based on provided IDs
        private async Task<List<WorkItem>> GetWorkItemsBatchAsync(List<int> ids)
        {
            var workItems = new List<WorkItem>();

            try
            {
                foreach (var id in ids)
                {
                    var workItem = await GetWorkItemDetailsAsync(id);
                    if (workItem != null)
                    {
                        workItems.Add(workItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching work items batch: {ex.Message}");
            }

            return workItems;
        }

        // Helper method to fetch details of a work item by ID asynchronously
        private async Task<WorkItem> GetWorkItemDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_projectName}/_apis/wit/workitems/{id}?api-version=6.0");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var workItem = JsonConvert.DeserializeObject<JObject>(responseBody);

                    return new WorkItem
                    {
                        Id = workItem["id"].Value<int>(),
                        Title = workItem["fields"]["System.Title"].Value<string>(),
                        AssignedTo = workItem["fields"]["System.AssignedTo"]?["displayName"]?.Value<string>(),
                        State = workItem["fields"]["System.State"]?.Value<string>(),
                        Tags = workItem["fields"]["System.Tags"]?.Value<string>(),
                        Discipline = workItem["fields"]["Microsoft.VSTS.Common.Discipline"]?.Value<string>(),
                        OriginalRequirementId = workItem["fields"]["Custom.3e6a7048-fa00-4b92-890c-5f56c824ea24"]?.Value<string>(),
                        RequirementCategory = workItem["fields"]["Custom.RequirementCategory"]?.Value<string>(),
                        Round = workItem["fields"]["Custom.Round"]?.Value<string>()
                    };
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
    }
}
