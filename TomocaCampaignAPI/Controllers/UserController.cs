using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using TomocaCampaignAPI.DTOs;
using TomocaCampaignAPI.Services;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserController> _logger;
        public UserController(AppDbContext dbContext, ILogger<UserController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        //Get User: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _dbContext.Users.ToListAsync();
        }

        [HttpPost("NewUser")]

        public async Task<IActionResult> NewAddUser(string RefernceCode, [FromBody] TelegramUpdate update)
        {
            _logger.LogInformation("Processing New User Sign");

            if (RefernceCode == null || update == null)
            {
                
                return NotFound("No refernce number sent and update sent");

            }

            if (update.Message != null)
            {
                var userId = update.Message.From!.Id;
                var firstName = update.Message.From.FirstName;
                var lastName = update.Message.From.LastName;
                var username = update.Message.From.Username;
               
               

                var empoyee = await _dbContext.Employees
     .Where(e => e.ReferralCode!.Trim() == RefernceCode.Trim())
     .FirstOrDefaultAsync();
               
                if (empoyee == null)
                {
                    _logger.LogInformation("No employee found with the provided referral code.");

                    return NotFound("No employee found with the provided referral code.");
                }

          
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId.ToString());

                if (existingUser != null)
                {
                    
                    return Conflict("A user with the same UserId already exists.");
                }

                
                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserId = userId.ToString(),
                    EmployeeId = empoyee.Id,
                    MoneySpent = 0.0M 
                };

                
                await _dbContext.Users.AddAsync(user);

                await _dbContext.SaveChangesAsync();



                empoyee.ReferralCount += 1; 

                _dbContext.Employees.Update(empoyee);
                await _dbContext.SaveChangesAsync();

 
                return Ok("User added and employee count updated successfully.");


            }
            else
            {
                return BadRequest("Missing users data");
            }
           
        }


        // 2. Get user by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _dbContext.Users.Include(u => u.Employee)
                                             .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // 3. Get users ordered by MoneySpent (big to small)
        [HttpGet("order-by-money-spent")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersOrderedByMoneySpent()
        {
            return await _dbContext.Users.OrderByDescending(u => u.MoneySpent)
                                         .Include(u => u.Employee)
                                         .ToListAsync();
        }

        // 4. Get users ordered by JoiningDate (newest to oldest)
        [HttpGet("order-by-joining-date")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersOrderedByJoiningDate()
        {
            return await _dbContext.Users.OrderByDescending(u => u.JoiningDate)
                                         .Include(u => u.Employee)
                                         .ToListAsync();
        }

        // 5. Get users grouped by their EmployeeId
        [HttpGet("grouped-by-employee")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersGroupedByEmployee()
        {
            var groupedUsers = await _dbContext.Users
                .GroupBy(u => u.EmployeeId)
                .ToListAsync();

          
            var result = groupedUsers.Select(g => new
            {
                EmployeeId = g.Key,
                EmployeeName = g.FirstOrDefault()?.Employee?.Name ?? "Unknown", 
                Users = g.ToList()
            }).ToList();

           
            return Ok(result);
        }

    }
}
