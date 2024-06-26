using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace TylerAPIToDevOps
{

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var azureDevOpsService = new AzureDevOpsService(
                AzureDevOpsConfig.Organization,
                AzureDevOpsConfig.ProjectName,
                AzureDevOpsConfig.PersonalAccessToken);

            try
            {
                // Define all queries with their respective Wiql queries
                var queries = new Dictionary<Func<Task<List<object>>>, string>
                {
                    { async () => (await azureDevOpsService.QueryWorkItemsAsync(QueryConstants.WorkItemWiqlQuery)).Cast<object>().ToList(), "Work Items" },
                    { async () => (await azureDevOpsService.QueryBugsAsync(QueryConstants.BugWiqlQuery)).Cast<object>().ToList(), "Bugs" },
                    { async () => (await azureDevOpsService.QueryTestCasesAsync(QueryConstants.TestCaseWiqlQuery)).Cast<object>().ToList(), "Test Cases" },
                    { async () => (await azureDevOpsService.QueryRequirementsAsync(QueryConstants.RequirementWiqlQuery)).Cast<object>().ToList(), "Requirements" },
                    { async () => (await azureDevOpsService.QueryOutcomesAsync(QueryConstants.OutcomeWiqlQuery)).Cast<object>().ToList(), "Outcomes" }
                };

                // Execute all queries and print results in order
                foreach (var query in queries)
                {
                    await ProcessQueryAndPrintAsync(azureDevOpsService, query.Key, query.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(); // Using ReadKey to pause until a key is pressed
            }
        }

        // Processes a query asynchronously and prints the first two items' details
        private static async Task ProcessQueryAndPrintAsync(AzureDevOpsService service, Func<Task<List<object>>> queryFunc, string queryName)
        {
            var items = await queryFunc();

            Console.WriteLine($"--- {queryName} ---");
            for (int i = 0; i < Math.Min(2, items.Count); i++)
            {
                PrintDetails(items[i], queryName);
            }
            Console.WriteLine();
        }

        // Prints details of an object dynamically based on its type and query name
        private static void PrintDetails<T>(T item, string queryName)
        {
            var methodInfo = item.GetType().GetMethod("Print" + queryName.Replace(" ", "") + "Details");
            if (methodInfo != null)
            {
                methodInfo.Invoke(item, null);
            }
        }
    }
}
