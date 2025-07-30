using Microsoft.AspNetCore.Mvc;
using portfolioAPI.models;
using portfolioAPI.services;
using System.Threading.Tasks;

namespace portfolioAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req)
        {
            try
            {
                var answer = await _chatService.AskAsync(req.Question);
                return Ok(new { response = answer });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Request error: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
