using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using TomocaCampaignAPI.DTOs;

namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
            if (update.Message != null)
            {
                var userId = update.Message.From.Id;
                var firstName = update.Message.From.FirstName;
                var lastName = update.Message.From.LastName; 
                var username = update.Message.From.Username;

               
                var empoyee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.ReferralCode == RefernceCode);

                if (empoyee == null)
                {
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
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    EmployeeName = g.FirstOrDefault().Employee.Name, 
                    Users = g.ToList()
                })
                .ToListAsync();

            return Ok(groupedUsers);
        }

    }
}
