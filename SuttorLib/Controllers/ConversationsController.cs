using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.DTOs;
using SuttorLib.Models;
using SuttorLibrary.Core.Services.aiConversations;
using System.Security.Claims;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController(IConversationService conversationService, ILogger<ConversationsController> logger) : ControllerBase
    {
        private readonly IConversationService _conversation = conversationService;
        private readonly ILogger<ConversationsController> _logger = logger;

        // Post api/conversation/new
        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> NewConversation(AddConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Messages is null || dto.Messages.Count == 0)
                return BadRequest("Messages cannot be null or empty.");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _conversation.CreateConversation(dto, userId);
                if (result is null)
                {
                    _logger.LogInformation("Failed to Create the Conversation for the user with ID: {userId}", userId);
                    return BadRequest("Something went wrong. Cannot Create your conversation!");
                }

                List<Message>? messages = await _conversation.GetMessagesForConversation(result.Id.ToString()) ?? new List<Message>();

                ConversationDTO cDto = new ConversationDTO
                {
                    Id = result.Id.ToString(),
                    Title = result.Title ?? "",
                    Messages = messages,
                    CreatedAT = result.CreatedAT
                };

                _logger.LogInformation("Create the Conversation for the user with ID: {userId} Successfully", cDto.Id);
                return CreatedAtAction(nameof(NewConversation), new { cDto.Id }, cDto);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the conversation.");
                return StatusCode(500, "An error occurred while creating the conversation.");
            }
        }

        // GET api/conversations/cId=...
        [Authorize]
        [HttpGet("{cId}")]
        public async Task<IActionResult> GetConversation([FromRoute] string cId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(cId))
                return BadRequest("conversation id is required");

            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var conv = await _conversation.GetConversation(cId, userId);

                _logger.LogInformation("Retrieved the Conversation {cId} for the user with ID: {userId}",cId, userId);
                return Ok(conv);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the conversation {cId} for the user.", cId);
                return StatusCode(500, "An error occurred while the conversation for the user.");
            }
        }

        // GET api/conversations
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var convers = await _conversation.GetConversationsAsync(userId);
                if (convers is null)
                {
                    _logger.LogInformation("Failed to Find the Conversation for the user with ID: {userId}", userId);
                    return NotFound("No Conversation for you");
                }

                _logger.LogInformation("Retrieved the Conversations for the user with ID: {userId} successfully", userId);
                return Ok(convers);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all the conversations for the user.");
                return StatusCode(500, "An error occurred while retrieving all the conversations for the user.");
            }
        }

        // PUT api/conversations?cId=...
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateConversation([FromQuery] string cId, [FromBody] UpdateConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(cId))
                return BadRequest("Conversation ID is required");

            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var con = await _conversation.UpdateConversation(cId, dto, userId);

                List<Message> messages = await _conversation.GetMessagesForConversation(con.Id.ToString()) ?? [];

                ConversationDTO cDto = new ConversationDTO
                {
                    Id = cId,
                    Title = con.Id.ToString(),
                    Messages = messages,
                    CreatedAT = con.CreatedAT
                };

                _logger.LogInformation("Updated the Conversation for the user with ID: {userId} successfully", userId);
                return Ok(cDto);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the conversation {cId}.", cId);
                return StatusCode(500, "An error occurred while Updating the blog.");
            }
        }

        // DELETE api/conversations/{id}/Delete
        [Authorize]
        [HttpDelete("{id}/Delete")]
        public async Task<IActionResult> DeleteConversation([FromBody] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(id))
                return BadRequest("Conversation Id is required");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var conv = await _conversation.DeleteConversation(id, userId);
                if (!conv)
                {
                    _logger.LogInformation("Delete Conversation: {id}", id);
                    return BadRequest($"Something went wrong, Retry later.");
                }

                _logger.LogInformation("Delete Conversation with ID: {id} Successfully", id);
                return Ok(conv);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the conversation {cid}.", id);
                return StatusCode(500, "An error occurred while deleting the conversation.");
            }
        }

        // Delete api/conversations/DeleteAll
        [Authorize]
        [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                await _conversation.DeleteAllConversations(userId);

                return Ok();
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Unauthorized(ua.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Deleting all the conversations.");
                return StatusCode(500, "An error occurred while Deleting all the conversations.");
            }
        }
    }
}
