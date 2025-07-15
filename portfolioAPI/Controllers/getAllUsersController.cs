using Microsoft.AspNetCore.Mvc;
using portfolioAPI.Data;
using portfolioAPI.Models;
using System.Linq;
using portfolioAPI.Controllers; 
namespace portfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.ContactMessages
                .Select(cm => new { cm.Id, cm.Name, cm.Email })
                .Distinct()
                .ToList();
            return Ok(users);
        }

        [HttpPost("send-mass-email")]
        public IActionResult SendMassEmail([FromBody] MassEmailDto dto)
        {
            var emails = _context.ContactMessages
                .Select(cm => cm.Email)
                .Distinct()
                .ToList();

            foreach (var email in emails)
            {
                EmailService.SendEmailAsync(email, dto.Subject, dto.Message).Wait();
            }

            return Ok(new { message = "Mass email sent to all users." });
        }

        public class MassEmailDto
        {
            public string Subject { get; set; }
            public string Message { get; set; }
        }
    }
}
