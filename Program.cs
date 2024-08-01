using ApiConsole;
using DataLayer;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TylerAPIToDevOps
{
    internal class Program
    {
        #region Configuration
        private static IConfiguration Configuration { get; set; }
        #endregion

        #region Main Method
        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            var azureDevOpsService = new AzureDevOpsService(
                AzureDevOpsConfig.Organization,
                AzureDevOpsConfig.ProjectName,
                AzureDevOpsConfig.PersonalAccessToken);

            bool dataLoadedSuccessfully = false;

            while (!dataLoadedSuccessfully)
            {
                try
                {
                    // For testing purposes if something breaks
                    // Console.WriteLine(azureDevOpsService.ListFieldsAsync()); // print all available fields

                    #region Clear Old Data
                    // Clear old data in database
                    await DeleteOldData.DeleteAllWorkItemsAsync(connectionString);
                    await DeleteOldData.DeleteAllBugsAsync(connectionString);
                    await DeleteOldData.DeleteAllTestCasesAsync(connectionString);
                    await DeleteOldData.DeleteAllRequirementsAsync(connectionString);
                    await DeleteOldData.DeleteAllOutcomesAsync(connectionString);
                    Console.WriteLine("Old data deleted successfully.");
                    #endregion

                    #region Insert Data
                    // WorkItems
                    var workItems = await azureDevOpsService.QueryWorkItemsAsync(QueryConstants.WorkItemWiqlQuery);
                    await AddDataToSQL.InsertWorkItemsIntoDatabaseAsync(connectionString, workItems);

                    // Bugs
                    var bugs = await azureDevOpsService.QueryBugsAsync(QueryConstants.BugWiqlQuery);
                    await AddDataToSQL.InsertBugsIntoDatabaseAsync(connectionString, bugs);

                    // TestCases
                    var testCases = await azureDevOpsService.QueryTestCasesAsync(QueryConstants.TestCaseWiqlQuery);
                    await AddDataToSQL.InsertTestCasesIntoDatabaseAsync(connectionString, testCases);

                    // Requirements
                    var requirements = await azureDevOpsService.QueryRequirementsAsync(QueryConstants.RequirementWiqlQuery);
                    await AddDataToSQL.InsertRequirementsIntoDatabaseAsync(connectionString, requirements);

                    // Outcomes
                    var outcomes = await azureDevOpsService.QueryOutcomesAsync(QueryConstants.OutcomeWiqlQuery);
                    await AddDataToSQL.InsertOutcomesIntoDatabaseAsync(connectionString, outcomes);

                    Console.WriteLine("Data inserted successfully.");
                    dataLoadedSuccessfully = true; // Exit loop if data was inserted successfully
                    #endregion
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL connection failed: {ex.Message}. Retrying in 1 minute...");
                    await Task.Delay(TimeSpan.FromMinutes(1)); // Wait for 1 minute before retrying
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(); // Using ReadKey to pause until a key is pressed
        }
        #endregion
    }
}
