using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
using TomocaCampaignAPI.DTOs;
using TomocaCampaignAPI.Models;

namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(AppDbContext appDbContext, ILogger<TransactionController> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        [HttpPost("newRevenu")]
        public async Task<IActionResult> newRevenueAdded(string trxId, [FromBody] UsersBot userData)
        {

            String UserID = userData.UserId;
            decimal transactionAmount = (decimal)userData.TotalAmount;
            _logger.LogInformation(transactionAmount.ToString());
            try
            {

                var user = await _appDbContext.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.UserId == UserID);

                if (user == null)
                {
                    return NotFound($"No user found with UserId: {UserID}");
                }


                var employee = user.Employee;

                if (employee == null)
                {
                    return NotFound($"No employee found for UserId: {UserID}");
                }


                var transaction = new Transactions
                {
                    UserName = $"{user.FirstName} {user.LastName}",
                    EmployeeName = employee.Name,
                    EmployeeDbId = employee.Id,
                    UserDbId = user.Id,
                    TransactionId = trxId,
                    TotalTransaction = transactionAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _appDbContext.Transactions.Add(transaction);

                employee.TotalRevenue += transactionAmount;
                user.MoneySpent += transactionAmount;

                await _appDbContext.SaveChangesAsync();


                var response = new
                {
                    Transaction = transaction,
                    UpdatedEmployee = employee
                };

                return Ok(response);


            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }

        // 1. Get all transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactions()
        {
            var transactions = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User) // Assuming you want to include both Employee and User data
                .ToListAsync();

            return Ok(transactions);
        }

        // 2. Get transaction by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Transactions>> GetTransactionById(int id)
        {
            var transaction = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        // 3. Get transactions by User ID
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactionsByUserId(int userId)
        {
            var transactions = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User)
                .Where(t => t.UserDbId == userId)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound();
            }

            return Ok(transactions);
        }

        // 4. Get transactions by Employee ID
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactionsByEmployeeId(int employeeId)
        {
            var transactions = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User)
                .Where(t => t.EmployeeDbId == employeeId)
                .ToListAsync();

            // Return an empty array instead of NotFound
            if (!transactions.Any())
            {
                return Ok(new List<Transactions>()); // Empty data with 200 OK
            }

            return Ok(transactions);
        }

        // 5. Get transactions ordered by TotalTransaction (big to small)
        [HttpGet("order-by-transaction-amount")]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactionsOrderedByTotalTransaction()
        {
            var transactions = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User)
                .OrderByDescending(t => t.TotalTransaction)
                .ToListAsync();

            return Ok(transactions);
        }

        // 6. Get transactions by date range (CreatedAt)
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            var transactions = await _appDbContext.Transactions
                .Include(t => t.Employee)
                .Include(t => t.User)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound();
            }

            return Ok(transactions);
        }

        [HttpGet("total-revenue-by-date")]
        public async Task<ActionResult<object>> GetTotalRevenueByDate()
        {
           
            var transactions = await _appDbContext.Set<Transactions>()
                                              .ToListAsync();

            
            var totalRevenueByDate = transactions
                .GroupBy(t => t.CreatedAt.ToString("MMM dd")) 
                .Select(g => new
                {
                    Date = g.Key, 
                    TotalRevenue = g.Sum(t => t.TotalTransaction) 
                })
                .ToList();

        
            var revenueDictionary = totalRevenueByDate.ToDictionary(x => x.Date, x => x.TotalRevenue);

          
            return Ok(revenueDictionary);
        }

    }
}