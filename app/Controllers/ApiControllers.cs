using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(DatabaseService databaseService, ILogger<ExpensesController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter string for searching expenses</param>
    /// <returns>List of expenses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Expense>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Expense>>> GetExpenses([FromQuery] string? filter = null)
    {
        try
        {
            var expenses = await _databaseService.GetExpensesAsync(filter);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            return StatusCode(500, "An error occurred while retrieving expenses");
        }
    }

    /// <summary>
    /// Get a specific expense by ID
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Expense details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Expense), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        try
        {
            var expense = await _databaseService.GetExpenseByIdAsync(id);
            
            if (expense == null)
            {
                return NotFound($"Expense with ID {id} not found");
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense {ExpenseId}", id);
            return StatusCode(500, "An error occurred while retrieving the expense");
        }
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    /// <param name="expense">Expense details</param>
    /// <returns>Created expense ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateExpense([FromBody] Expense expense)
    {
        try
        {
            if (expense == null)
            {
                return BadRequest("Expense data is required");
            }

            var expenseId = await _databaseService.CreateExpenseAsync(expense);
            return CreatedAtAction(nameof(GetExpense), new { id = expenseId }, new { expenseId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, "An error occurred while creating the expense");
        }
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <param name="expense">Updated expense details</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateExpense(int id, [FromBody] Expense expense)
    {
        try
        {
            if (expense == null || expense.ExpenseId != id)
            {
                return BadRequest("Expense ID mismatch");
            }

            var success = await _databaseService.UpdateExpenseAsync(expense);
            
            if (!success)
            {
                return NotFound($"Expense with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", id);
            return StatusCode(500, "An error occurred while updating the expense");
        }
    }

    /// <summary>
    /// Submit an expense for approval
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SubmitExpense(int id)
    {
        try
        {
            var success = await _databaseService.SubmitExpenseAsync(id);
            
            if (!success)
            {
                return NotFound($"Expense with ID {id} not found");
            }

            return Ok(new { message = "Expense submitted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense {ExpenseId}", id);
            return StatusCode(500, "An error occurred while submitting the expense");
        }
    }

    /// <summary>
    /// Approve an expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <param name="request">Approval request containing manager ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveExpense(int id, [FromBody] ApprovalRequest request)
    {
        try
        {
            if (request == null || request.ManagerId <= 0)
            {
                return BadRequest("Valid manager ID is required");
            }

            var success = await _databaseService.ApproveExpenseAsync(id, request.ManagerId);
            
            if (!success)
            {
                return NotFound($"Expense with ID {id} not found");
            }

            return Ok(new { message = "Expense approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense {ExpenseId}", id);
            return StatusCode(500, "An error occurred while approving the expense");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(DatabaseService databaseService, ILogger<CategoriesController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expense categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseCategory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExpenseCategory>>> GetCategories()
    {
        try
        {
            var categories = await _databaseService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StatusesController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<StatusesController> _logger;

    public StatusesController(DatabaseService databaseService, ILogger<StatusesController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expense statuses
    /// </summary>
    /// <returns>List of statuses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseStatus>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExpenseStatus>>> GetStatuses()
    {
        try
        {
            var statuses = await _databaseService.GetStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statuses");
            return StatusCode(500, "An error occurred while retrieving statuses");
        }
    }
}

public class ApprovalRequest
{
    public int ManagerId { get; set; }
}
