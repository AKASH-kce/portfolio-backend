// Controllers/ContactController.cs
using Microsoft.AspNetCore.Mvc;
using portfolioAPI.Data;
using portfolioAPI.Models;
using portfolioAPI.Dtos;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace portfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("AddContact")]
        public async Task<IActionResult> AddContact([FromForm] ContactMessageDto dto, IFormFile? file)
        {
            var entity = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Message = dto.Message
            };
            var newUser = new User
            {
                Name = dto.Name,
                Email=dto.Email
            };
            

            if (file != null && file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    entity.FileData = ms.ToArray();
                }
                entity.FileName = file.FileName;
                entity.ContentType = file.ContentType;
            }

            // Send email before saving to DB
            var emailBody = $"Name: {dto.Name}\nEmail: {dto.Email}\nMessage: {dto.Message}";
            if (file != null && file.Length > 0)
            {
                emailBody += $"\nFileName: {file.FileName}";
            }
            var emailSent = await EmailService.SendEmailAsync(
                to: "akashkce123@gmail.com",
                subject: "New Contact Message",
                body: emailBody,
                file: file
            );
            if (!emailSent)
            {
                return StatusCode(500, new { message = "Failed to send email. Data not saved." });
            }

            // Send thank you email to the user
            var thankYouBody = $"Hi {dto.Name},\n\nThank you so much for reaching out! I truly appreciate your message and will get back to you as soon as possible.\n\nWith gratitude,\nAkash";
            await EmailService.SendEmailAsync(
                to: dto.Email,
                subject: "Thank you for contacting Akash!",
                body: thankYouBody
            );

            _context.ContactMessages.Add(entity);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Saved Successfully", id = entity.Id });
        }

       
        [HttpGet("DownloadFile/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var contact = await _context.ContactMessages.FindAsync(id);
            if (contact == null || contact.FileData == null)
                return NotFound();

            return File(contact.FileData, contact.ContentType ?? "application/octet-stream", contact.FileName);
        }

        [HttpGet("Visit")]
        public async Task<IActionResult> Visit()
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                     ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "Unknown";

            string locationInfo = "Unknown";
            if (ip != "Unknown" && ip != "::1" && ip != "127.0.0.1")
            {
                using var httpClient = new System.Net.Http.HttpClient();
                try
                {
                    var geoResponse = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
                    using var doc = JsonDocument.Parse(geoResponse);
                    var geoData = doc.RootElement;
                    var country = geoData.GetProperty("country").GetString();
                    var region = geoData.GetProperty("regionName").GetString();
                    var city = geoData.GetProperty("city").GetString();
                    locationInfo = $"{city}, {region}, {country}";
                }
                catch
                {
                    locationInfo = "GeoIP lookup failed";
                }
            }

            var userAgent = Request.Headers["User-Agent"].ToString();
            var referer = Request.Headers["Referer"].ToString();
            var url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";

            // Save visit to database
            var visit = new Visit
            {
                Timestamp = DateTime.UtcNow,
                IP = ip,
                Location = locationInfo,
                UserAgent = userAgent,
                Referer = referer,
                Url = url
            };
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            var ipMode = ip.Contains(":") ? "IPv6" : "IPv4";
            var emailBody = $@"
📥 New Visitor Logged

🌐 IP Address: {ip} ({ipMode})
📍 Location: {locationInfo}
📱 Platform: {userAgent}
🔗 Referer: {referer}
🧭 Requested URL: {url}
🕒 Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

            await EmailService.SendEmailAsync(
                to: "akashkce123@gmail.com",
                subject: "📡 New Site Visit",
                body: emailBody
            );

            return Ok(new { message = "Visit logged and email sent." });
        }

        [HttpGet("VisitStats")]
        public IActionResult GetVisitStats()
        {
            var grouped = _context.Visits
                .GroupBy(v => v.Timestamp.Date)
                .Select(g => new {
                    Date = g.Key,
                    Count = g.Count(),
                    Locations = g.Select(v => v.Location).ToList()
                })
                .OrderBy(x => x.Date)
                .ToList();
            var stats = grouped.Select(g => {
                // Parse states and countries from location strings
                var stateList = new List<string>();
                var countryList = new List<string>();
                foreach (var loc in g.Locations)
                {
                    if (!string.IsNullOrEmpty(loc))
                    {
                        var parts = loc.Split(',');
                        if (parts.Length >= 3)
                        {
                            // e.g., Navi Mumbai, Maharashtra, India
                            stateList.Add(parts[1].Trim());
                            countryList.Add(parts[2].Trim());
                        }
                        else if (parts.Length == 2)
                        {
                            // e.g., City, Country
                            stateList.Add("Unknown");
                            countryList.Add(parts[1].Trim());
                        }
                        else if (parts.Length == 1)
                        {
                            countryList.Add(parts[0].Trim());
                        }
                    }
                }
                var states = stateList.GroupBy(s => s)
                    .Select(stateGroup => new {
                        State = stateGroup.Key,
                        Count = stateGroup.Count()
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();
                var countries = countryList.GroupBy(c => c)
                    .Select(countryGroup => new {
                        Country = countryGroup.Key,
                        Count = countryGroup.Count()
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();
                return new {
                    Date = g.Date,
                    Count = g.Count,
                    States = states,
                    Countries = countries
                };
            }).ToList();

            return Ok(stats);
        }

        [HttpGet("TotalVisits")]
        public IActionResult GetTotalVisits()
        {
            var total = _context.Visits.Count();
            return Ok(new { TotalVisits = total });
        }

        [HttpGet("AllVisits")]
        public IActionResult GetAllVisits()
        {
            var visits = _context.Visits
                .OrderByDescending(v => v.Timestamp)
                .ToList();
            return Ok(visits);
        }

        [HttpDelete("DeleteVisit/{id}")]
        public IActionResult DeleteVisit(int id)
        {
            var visit = _context.Visits.FirstOrDefault(v => v.Id == id);
            if (visit == null)
                return NotFound();
            _context.Visits.Remove(visit);
            _context.SaveChanges();
            return Ok(new { message = "Visit deleted" });
        }

        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(new { message = "User deleted" });
        }
    }
}
