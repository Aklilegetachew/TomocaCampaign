using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;
using TomocaCampaignAPI.Services;
using TomocaCampaignAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;

namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly ReferralCodeService _referralCodeService;
        private readonly IConfiguration _configuration;

        public EmployeeController(AppDbContext context, ReferralCodeService referralCodeService, IConfiguration configuration) // Step 2: Inject and assign context
        {
            _context = context;
            _referralCodeService = referralCodeService;
            _configuration = configuration;
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


            //employee.ReferralCode = await _referralCodeService.GenerateReferralCodeAsync(employee.Name, employee.EmployeeId);
            employee.ReferralCode = _referralCodeService.GenerateEmployeeCode(employee.Name, employee.EmployeeId);

            employee.EmployeCode = $"http://t.me/Tomoca_bot?start={employee.ReferralCode}";


            Console.WriteLine(employee.ReferralCode);

            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.ReferralCount = 0;
            employee.TotalRevenue = 0.0m;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeByIdApi), new { id = employee.Id }, employee);


        }

        [HttpPost("login")]
        public async Task<ActionResult<Employee>> loginEmployee([FromBody] LoginRequest loginRequest)
        {

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == loginRequest.Username);


            if (employee == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, employee.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            var tokenData = new
            {
                Id = employee.Id,
                Username = employee.Username,
                Name = employee.Name,
                refernceCode = employee.ReferralCode,
                referncelink = employee.ReferralCount,
                revenu = employee.TotalRevenue,
                Role = employee.RoleType,
            };

            var token = GenerateJwtToken(tokenData);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
              
            });

        }



        // 1. Get all employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
        {
            return await _context.Set<Employee>().ToListAsync();
        }

        // 2. Get employee by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeByIdApi(int id)
        {
            var employee = await _context.Set<Employee>().FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // 3. Get all employees ordered by ReferralCount (big to small)
        [HttpGet("order-by-referral")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesOrderedByReferralCount()
        {
            return await _context.Set<Employee>()
                                 .OrderByDescending(e => e.ReferralCount)
                                 .ToListAsync();
        }

        // 4. Get all employees ordered by TotalRevenue (big to small)
        [HttpGet("order-by-revenue")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesOrderedByTotalRevenue()
        {
            return await _context.Set<Employee>()
                                 .OrderByDescending(e => e.TotalRevenue)
                                 .ToListAsync();
        }


        [HttpGet("total-revenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            var totalRevenue = await _context.Set<Employee>().SumAsync(e => e.TotalRevenue);
            return Ok(totalRevenue);
        }

        // 6. Get percentage contribution of each employee to the total revenue
        [HttpGet("revenue-percentage")]
        public async Task<ActionResult<IEnumerable<object>>> GetRevenuePercentage()
        {
            var employees = await _context.Set<Employee>().ToListAsync();
            var totalRevenue = employees.Sum(e => e.TotalRevenue);

            if (totalRevenue == 0)
            {
                return Ok(new { message = "Total revenue is zero; no percentages to calculate." });
            }

            var revenuePercentage = employees.Select(e => new
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                TotalRevenue = e.TotalRevenue,
                ContributionPercentage = Math.Round((e.TotalRevenue / totalRevenue) * 100, 2) // Rounded to 2 decimal places
            });

            return Ok(revenuePercentage);
        }

        [HttpGet("total-revenue-and-subscribers")]
        public async Task<ActionResult<object>> GetTotalRevenueAndSubscribers()
        {
            // Calculate the total revenue by summing the revenue of all subscribers
            var totalRevenue = await _context.Set<Employee>().SumAsync(s => s.TotalRevenue);

            // Get the total number of subscribers (or references, depending on your definition)
            var totalSubscribers = await _context.Set<Employee>().SumAsync(s => s.ReferralCount);

            // Return the response as an object containing both the total revenue and total subscribers count
            return Ok(new
            {
                totalRevenue,
                totalSubscribers
            });
        }

        [HttpGet("employee-data")]
        public async Task<ActionResult<List<object>>> GetEmployeeData()
        {
           
            var employees = await _context.Set<Employee>()
                .Select(e => new
                {
                    Name = e.Name,            
                    Referrals = e.ReferralCount,  
                    Revenue = e.TotalRevenue   
                })
                .ToListAsync();

          
            var employeeData = employees.Select(e => new
            {
                name = e.Name,
                referrals = e.Referrals,
                revenue = e.Revenue
            }).ToList();

           
            return Ok(employeeData);
        }
        private string GenerateJwtToken(dynamic tokenData)
        {
            // Access environment variables through IConfiguration
            //var jwtKey = _configuration["Jwt:Key"];
            //var jwtIssuer = _configuration["Jwt:Issuer"];
            //var jwtAudience = _configuration["Jwt:Audience"];

            var jwtKey = "MySuperSecretKey";
            var jwtIssuer = "TomocaCompaign";
            var jwtAudience = "TomocaCustomers";

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new ArgumentNullException("JWT configuration is incomplete.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, tokenData.Username),
        new Claim("EmployeeId", tokenData.Id.ToString()),
        new Claim("Name", tokenData.Name),
        new Claim("ReferenceCode", tokenData.refernceCode),
        new Claim("ReferenceCount", tokenData.referncelink.ToString()),
        new Claim("Revenue", tokenData.revenu.ToString()),
        new Claim("Role", tokenData.Role ?? "User") // Default to "User" if null
    };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }







    }



}
