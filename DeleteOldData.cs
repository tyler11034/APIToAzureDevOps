using System.Data.SqlClient;

namespace ApiConsole
{
    internal class DeleteOldData
    {
        public static async Task DeleteAllWorkItemsAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM WorkItems";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAllBugsAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Bugs";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAllTestCasesAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM TestCases";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAllRequirementsAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Requirements";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task DeleteAllOutcomesAsync(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Outcomes";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
