using Azure.Identity;
using ExpenseManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpenseManagement.Services;

public class DatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseService> _logger;
    private bool _useDummyData = false;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<SqlConnection> GetConnectionAsync()
    {
        try
        {
            var managedIdentityClientId = _configuration["ManagedIdentityClientId"];
            var connectionString = "Server=tcp:sql-expense-mgmt-xyz.database.windows.net,1433;" +
                                  "Initial Catalog=ExpenseManagementDB;" +
                                  "Encrypt=True;" +
                                  "TrustServerCertificate=False;" +
                                  "Connection Timeout=30;";

            SqlConnection connection;

            if (!string.IsNullOrEmpty(managedIdentityClientId))
            {
                // Use managed identity authentication
                connectionString += $"Authentication=Active Directory Managed Identity;User Id={managedIdentityClientId};";
                connection = new SqlConnection(connectionString);
            }
            else
            {
                // Fallback to Azure CLI credentials for local development
                var credential = new AzureCliCredential();
                var token = await credential.GetTokenAsync(
                    new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                
                connection = new SqlConnection(connectionString);
                connection.AccessToken = token.Token;
            }

            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database. Falling back to dummy data.");
            _useDummyData = true;
            throw;
        }
    }

    public async Task<List<Expense>> GetExpensesAsync(string? filter = null)
    {
        if (_useDummyData)
        {
            return GetDummyExpenses();
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT e.ExpenseId, e.UserId, e.CategoryId, e.StatusId, e.AmountMinor, 
                       e.Currency, e.ExpenseDate, e.Description, e.ReceiptFile, 
                       e.SubmittedAt, e.ReviewedBy, e.ReviewedAt, e.CreatedAt,
                       u.UserName, c.CategoryName, s.StatusName
                FROM dbo.Expenses e
                JOIN dbo.Users u ON e.UserId = u.UserId
                JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
                JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
                WHERE (@filter IS NULL OR u.UserName LIKE '%' + @filter + '%' 
                       OR c.CategoryName LIKE '%' + @filter + '%'
                       OR e.Description LIKE '%' + @filter + '%')
                ORDER BY e.CreatedAt DESC", connection);
            
            command.Parameters.AddWithValue("@filter", (object?)filter ?? DBNull.Value);

            var expenses = new List<Expense>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpenseFromReader(reader));
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses. Returning dummy data.");
            _useDummyData = true;
            return GetDummyExpenses();
        }
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
    {
        if (_useDummyData)
        {
            return GetDummyExpenses().FirstOrDefault(e => e.ExpenseId == expenseId);
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT e.ExpenseId, e.UserId, e.CategoryId, e.StatusId, e.AmountMinor, 
                       e.Currency, e.ExpenseDate, e.Description, e.ReceiptFile, 
                       e.SubmittedAt, e.ReviewedBy, e.ReviewedAt, e.CreatedAt,
                       u.UserName, c.CategoryName, s.StatusName
                FROM dbo.Expenses e
                JOIN dbo.Users u ON e.UserId = u.UserId
                JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
                JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
                WHERE e.ExpenseId = @expenseId", connection);
            
            command.Parameters.AddWithValue("@expenseId", expenseId);

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapExpenseFromReader(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense. Returning dummy data.");
            _useDummyData = true;
            return GetDummyExpenses().FirstOrDefault(e => e.ExpenseId == expenseId);
        }
    }

    public async Task<int> CreateExpenseAsync(Expense expense)
    {
        if (_useDummyData)
        {
            return new Random().Next(1000, 9999);
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                INSERT INTO dbo.Expenses (UserId, CategoryId, StatusId, AmountMinor, Currency, 
                                         ExpenseDate, Description, ReceiptFile, CreatedAt)
                VALUES (@userId, @categoryId, @statusId, @amountMinor, @currency, 
                        @expenseDate, @description, @receiptFile, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() as int);", connection);
            
            command.Parameters.AddWithValue("@userId", expense.UserId);
            command.Parameters.AddWithValue("@categoryId", expense.CategoryId);
            command.Parameters.AddWithValue("@statusId", expense.StatusId);
            command.Parameters.AddWithValue("@amountMinor", expense.AmountMinor);
            command.Parameters.AddWithValue("@currency", expense.Currency);
            command.Parameters.AddWithValue("@expenseDate", expense.ExpenseDate);
            command.Parameters.AddWithValue("@description", (object?)expense.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@receiptFile", (object?)expense.ReceiptFile ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense. Returning dummy ID.");
            _useDummyData = true;
            return new Random().Next(1000, 9999);
        }
    }

    public async Task<bool> UpdateExpenseAsync(Expense expense)
    {
        if (_useDummyData)
        {
            return true;
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE dbo.Expenses 
                SET CategoryId = @categoryId, 
                    StatusId = @statusId, 
                    AmountMinor = @amountMinor, 
                    Currency = @currency,
                    ExpenseDate = @expenseDate, 
                    Description = @description, 
                    ReceiptFile = @receiptFile
                WHERE ExpenseId = @expenseId", connection);
            
            command.Parameters.AddWithValue("@expenseId", expense.ExpenseId);
            command.Parameters.AddWithValue("@categoryId", expense.CategoryId);
            command.Parameters.AddWithValue("@statusId", expense.StatusId);
            command.Parameters.AddWithValue("@amountMinor", expense.AmountMinor);
            command.Parameters.AddWithValue("@currency", expense.Currency);
            command.Parameters.AddWithValue("@expenseDate", expense.ExpenseDate);
            command.Parameters.AddWithValue("@description", (object?)expense.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@receiptFile", (object?)expense.ReceiptFile ?? DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense.");
            _useDummyData = true;
            return false;
        }
    }

    public async Task<bool> SubmitExpenseAsync(int expenseId)
    {
        if (_useDummyData)
        {
            return true;
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE dbo.Expenses 
                SET StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted'),
                    SubmittedAt = SYSUTCDATETIME()
                WHERE ExpenseId = @expenseId", connection);
            
            command.Parameters.AddWithValue("@expenseId", expenseId);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense.");
            _useDummyData = true;
            return false;
        }
    }

    public async Task<bool> ApproveExpenseAsync(int expenseId, int managerId)
    {
        if (_useDummyData)
        {
            return true;
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE dbo.Expenses 
                SET StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved'),
                    ReviewedBy = @managerId,
                    ReviewedAt = SYSUTCDATETIME()
                WHERE ExpenseId = @expenseId", connection);
            
            command.Parameters.AddWithValue("@expenseId", expenseId);
            command.Parameters.AddWithValue("@managerId", managerId);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense.");
            _useDummyData = true;
            return false;
        }
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        if (_useDummyData)
        {
            return GetDummyCategories();
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT CategoryId, CategoryName, IsActive FROM dbo.ExpenseCategories WHERE IsActive = 1", connection);

            var categories = new List<ExpenseCategory>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                categories.Add(new ExpenseCategory
                {
                    CategoryId = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    IsActive = reader.GetBoolean(2)
                });
            }

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories. Returning dummy data.");
            _useDummyData = true;
            return GetDummyCategories();
        }
    }

    public async Task<List<ExpenseStatus>> GetStatusesAsync()
    {
        if (_useDummyData)
        {
            return GetDummyStatuses();
        }

        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT StatusId, StatusName FROM dbo.ExpenseStatus", connection);

            var statuses = new List<ExpenseStatus>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                statuses.Add(new ExpenseStatus
                {
                    StatusId = reader.GetInt32(0),
                    StatusName = reader.GetString(1)
                });
            }

            return statuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statuses. Returning dummy data.");
            _useDummyData = true;
            return GetDummyStatuses();
        }
    }

    private Expense MapExpenseFromReader(SqlDataReader reader)
    {
        return new Expense
        {
            ExpenseId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            CategoryId = reader.GetInt32(2),
            StatusId = reader.GetInt32(3),
            AmountMinor = reader.GetInt32(4),
            Currency = reader.GetString(5),
            ExpenseDate = reader.GetDateTime(6),
            Description = reader.IsDBNull(7) ? null : reader.GetString(7),
            ReceiptFile = reader.IsDBNull(8) ? null : reader.GetString(8),
            SubmittedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
            ReviewedBy = reader.IsDBNull(10) ? null : reader.GetInt32(10),
            ReviewedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
            CreatedAt = reader.GetDateTime(12),
            UserName = reader.GetString(13),
            CategoryName = reader.GetString(14),
            StatusName = reader.GetString(15)
        };
    }

    private List<Expense> GetDummyExpenses()
    {
        return new List<Expense>
        {
            new Expense
            {
                ExpenseId = 1,
                UserId = 1,
                CategoryId = 1,
                StatusId = 2,
                AmountMinor = 12000,
                Currency = "GBP",
                ExpenseDate = DateTime.Parse("2024-01-15"),
                Description = "Taxi from airport to client site",
                StatusName = "Submitted",
                CategoryName = "Travel",
                UserName = "Alice Example",
                CreatedAt = DateTime.Now.AddDays(-10)
            },
            new Expense
            {
                ExpenseId = 2,
                UserId = 1,
                CategoryId = 2,
                StatusId = 2,
                AmountMinor = 6900,
                Currency = "GBP",
                ExpenseDate = DateTime.Parse("2023-01-10"),
                Description = "Client lunch meeting",
                StatusName = "Submitted",
                CategoryName = "Food",
                UserName = "Alice Example",
                CreatedAt = DateTime.Now.AddDays(-15)
            },
            new Expense
            {
                ExpenseId = 3,
                UserId = 1,
                CategoryId = 3,
                StatusId = 3,
                AmountMinor = 9950,
                Currency = "GBP",
                ExpenseDate = DateTime.Parse("2023-12-04"),
                Description = "Office supplies for project",
                StatusName = "Approved",
                CategoryName = "Office Supplies",
                UserName = "Alice Example",
                CreatedAt = DateTime.Now.AddDays(-20)
            },
            new Expense
            {
                ExpenseId = 4,
                UserId = 1,
                CategoryId = 4,
                StatusId = 3,
                AmountMinor = 1920,
                Currency = "GBP",
                ExpenseDate = DateTime.Parse("2023-21-18"),
                Description = "Public transport ticket",
                StatusName = "Approved",
                CategoryName = "Transport",
                UserName = "Alice Example",
                CreatedAt = DateTime.Now.AddDays(-25)
            }
        };
    }

    private List<ExpenseCategory> GetDummyCategories()
    {
        return new List<ExpenseCategory>
        {
            new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
            new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
            new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
            new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
            new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
        };
    }

    private List<ExpenseStatus> GetDummyStatuses()
    {
        return new List<ExpenseStatus>
        {
            new ExpenseStatus { StatusId = 1, StatusName = "Draft" },
            new ExpenseStatus { StatusId = 2, StatusName = "Submitted" },
            new ExpenseStatus { StatusId = 3, StatusName = "Approved" },
            new ExpenseStatus { StatusId = 4, StatusName = "Rejected" }
        };
    }
}
