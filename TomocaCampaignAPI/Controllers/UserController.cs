using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        
    }
}
