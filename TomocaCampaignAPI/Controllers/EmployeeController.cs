using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;
using TomocaCampaignAPI.Services;


namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly ReferralCodeService _referralCodeService;


        public EmployeeController(AppDbContext context, ReferralCodeService referralCodeService) // Step 2: Inject and assign context
        {
            _context = context;
            _referralCodeService = referralCodeService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployee()
        {
            return await _context.Employees.ToListAsync();
        }

        // Sign up endpoint
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(Employee employee)
        {
            var existingUser = await _context.Employees.FirstOrDefaultAsync(e => e.Username == employee.Username);
            if (existingUser != null)
            {
                return BadRequest("User Already Exists");
            }


            employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);


            employee.ReferralCode = _referralCodeService.GenerateReferralCode(employee.Name, employee.EmployeeId);
            Console.WriteLine(employee.ReferralCode);
            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.ReferralCount = 0;
            employee.TotalRevenue = 0.0m;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);


        }

        [HttpPost("login")]
        public async Task<ActionResult<Employee>> loginEmployee([FromBody] LoginRequest loginRequest)
        {
            // Find the employee by username
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == loginRequest.Username);

            // If employee is not found or password doesn't match, return unauthorized
            if (employee == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, employee.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Optionally, generate and return a token (e.g., JWT) if you’re implementing token-based authentication
            return Ok(new
            {
                Message = "Login successful",
                EmployeeId = employee.Id,
                Username = employee.Username
                // You could also include a token here, if implemented
            });
        }



        // Additional method to retrieve employee by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return employee;
        }
    }
}
