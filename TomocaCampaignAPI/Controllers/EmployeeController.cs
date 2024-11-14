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


            employee.ReferralCode = await _referralCodeService.GenerateReferralCodeAsync(employee.Name, employee.EmployeeId);
            employee.EmployeCode = await _referralCodeService.GenerateEmployeeCode(employee.Name, employee.EmployeeId);


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

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == loginRequest.Username);


            if (employee == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, employee.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(employee);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
                EmployeeId = employee.Id,
                Username = employee.Username
            });
         
        }




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


        private string GenerateJwtToken(Employee employee)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("EmployeeId", employee.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
