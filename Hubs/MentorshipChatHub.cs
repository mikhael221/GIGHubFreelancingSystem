using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Freelancing.Data;
using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Hubs
{
    [Authorize]
    public class MentorshipChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, string> UserConnections = new();
        private static readonly Dictionary<string, List<string>> RoomConnections = new();

        public MentorshipChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
                await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.Remove(userId);

                // Remove from all rooms
                var roomsToRemove = RoomConnections
                    .Where(kvp => kvp.Value.Contains(Context.ConnectionId))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var room in roomsToRemove)
                {
                    RoomConnections[room].Remove(Context.ConnectionId);
                    if (!RoomConnections[room].Any())
                    {
                        RoomConnections.Remove(room);
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinMentorshipRoom(string mentorshipMatchId)
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify user is part of this mentorship match
            var match = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.Id.ToString() == mentorshipMatchId &&
                                          (mm.MentorId == userId || mm.MenteeId == userId) &&
                                          mm.Status == "Active");

            if (match == null)
            {
                await Clients.Caller.SendAsync("Error", "Access denied or mentorship not active");
                return;
            }

            var roomName = $"mentorship_{mentorshipMatchId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            if (!RoomConnections.ContainsKey(roomName))
            {
                RoomConnections[roomName] = new List<string>();
            }
            RoomConnections[roomName].Add(Context.ConnectionId);

            await Clients.Caller.SendAsync("JoinedRoom", roomName);
            await Clients.OthersInGroup(roomName).SendAsync("UserJoined", Context.User.Identity.Name);
        }

        public async Task SendMessage(string mentorshipMatchId, string message, string messageType = "text")
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var userId = Guid.Parse(userIdClaim);

                // Get user info from database to ensure we have the name
                var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                var fullName = $"{user.FirstName} {user.LastName}";

                // Verify access and get match details
                var match = await _context.MentorshipMatches
                    .FirstOrDefaultAsync(mm => mm.Id.ToString() == mentorshipMatchId &&
                                              (mm.MentorId == userId || mm.MenteeId == userId) &&
                                              mm.Status == "Active");

                if (match == null)
                {
                    await Clients.Caller.SendAsync("Error", "Access denied or match not found");
                    return;
                }

                // Save message to database
                var chatMessage = new MentorshipChatMessage
                {
                    Id = Guid.NewGuid(),
                    MentorshipMatchId = Guid.Parse(mentorshipMatchId),
                    SenderId = userId,
                    Message = message,
                    MessageType = messageType,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.MentorshipChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                var roomName = $"mentorship_{mentorshipMatchId}";

                // Create the message object to send
                var messageToSend = new
                {
                    Id = chatMessage.Id.ToString(),
                    SenderId = userId.ToString(),
                    SenderName = fullName,
                    Message = message,
                    MessageType = messageType,
                    SentAt = chatMessage.SentAt.ToString("O"), // Use ISO 8601 format
                    IsRead = false
                };

                // Log for debugging (remove in production)
                Console.WriteLine($"Sending message: {System.Text.Json.JsonSerializer.Serialize(messageToSend)}");

                // Send to all users in the room
                await Clients.Group(roomName).SendAsync("ReceiveMessage", messageToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        public async Task SendFile(string mentorshipMatchId, string fileName, string fileUrl, long fileSize, string fileType)
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var userId = Guid.Parse(userIdClaim);

                // Get user info from database to ensure we have the name
                var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                var fullName = $"{user.FirstName} {user.LastName}";

                // Verify access
                var match = await _context.MentorshipMatches
                    .FirstOrDefaultAsync(mm => mm.Id.ToString() == mentorshipMatchId &&
                                              (mm.MentorId == userId || mm.MenteeId == userId) &&
                                              mm.Status == "Active");

                if (match == null)
                {
                    await Clients.Caller.SendAsync("Error", "Access denied or match not found");
                    return;
                }

                // Determine message type based on file type
                string messageType = "file";
                if (!string.IsNullOrEmpty(fileType) && fileType.StartsWith("image/"))
                {
                    messageType = "image";
                }

                // Save file message to database
                var fileMessage = new MentorshipChatMessage
                {
                    Id = Guid.NewGuid(),
                    MentorshipMatchId = Guid.Parse(mentorshipMatchId),
                    SenderId = userId,
                    Message = fileName, // Store filename in message field
                    MessageType = messageType,
                    FileUrl = fileUrl,
                    FileSize = fileSize,
                    FileType = fileType,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.MentorshipChatMessages.Add(fileMessage);
                await _context.SaveChangesAsync();

                var roomName = $"mentorship_{mentorshipMatchId}";

                // Create the file message object to send
                var messageToSend = new
                {
                    Id = fileMessage.Id.ToString(),
                    SenderId = userId.ToString(),
                    SenderName = fullName,
                    Message = fileName, // This will be used as fallback
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileSize = fileSize,
                    FileType = fileType,
                    MessageType = messageType,
                    SentAt = fileMessage.SentAt.ToString("O"), // Use ISO 8601 format
                    IsRead = false
                };

                // Log for debugging (remove in production)
                Console.WriteLine($"Sending file message: {System.Text.Json.JsonSerializer.Serialize(messageToSend)}");

                // Send to all users in the room
                await Clients.Group(roomName).SendAsync("ReceiveFile", messageToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendFile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Clients.Caller.SendAsync("Error", $"Failed to send file: {ex.Message}");
            }
        }

        public async Task StartVideoCall(string mentorshipMatchId)
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var fullName = Context.User?.FindFirst("FullName")?.Value;
            var roomName = $"mentorship_{mentorshipMatchId}";

            // Notify other user about incoming video call
            await Clients.OthersInGroup(roomName).SendAsync("IncomingVideoCall", new
            {
                CallerId = userId,
                CallerName = fullName,
                MentorshipMatchId = mentorshipMatchId
            });
        }

        public async Task AcceptVideoCall(string mentorshipMatchId, string callerId)
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var roomName = $"mentorship_{mentorshipMatchId}";

            await Clients.Group(roomName).SendAsync("VideoCallAccepted", new
            {
                AccepterId = userId,
                CallerId = callerId,
                MentorshipMatchId = mentorshipMatchId
            });
        }

        public async Task DeclineVideoCall(string mentorshipMatchId, string callerId)
        {
            var roomName = $"mentorship_{mentorshipMatchId}";

            await Clients.Group(roomName).SendAsync("VideoCallDeclined", new
            {
                MentorshipMatchId = mentorshipMatchId
            });
        }

        public async Task EndVideoCall(string mentorshipMatchId)
        {
            var roomName = $"mentorship_{mentorshipMatchId}";

            await Clients.Group(roomName).SendAsync("VideoCallEnded", new
            {
                MentorshipMatchId = mentorshipMatchId
            });
        }

        // WebRTC signaling methods
        public async Task SendOffer(string mentorshipMatchId, string offer)
        {
            var roomName = $"mentorship_{mentorshipMatchId}";
            await Clients.OthersInGroup(roomName).SendAsync("ReceiveOffer", offer);
        }

        public async Task SendAnswer(string mentorshipMatchId, string answer)
        {
            var roomName = $"mentorship_{mentorshipMatchId}";
            await Clients.OthersInGroup(roomName).SendAsync("ReceiveAnswer", answer);
        }

        public async Task SendIceCandidate(string mentorshipMatchId, string candidate)
        {
            var roomName = $"mentorship_{mentorshipMatchId}";
            await Clients.OthersInGroup(roomName).SendAsync("ReceiveIceCandidate", candidate);
        }

        public async Task MarkMessagesAsRead(string mentorshipMatchId)
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var messages = await _context.MentorshipChatMessages
                .Where(mcm => mcm.MentorshipMatchId.ToString() == mentorshipMatchId &&
                             mcm.SenderId != userId && !mcm.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var roomName = $"mentorship_{mentorshipMatchId}";
            await Clients.Group(roomName).SendAsync("MessagesMarkedAsRead", new
            {
                ReadById = userId,
                MessageIds = messages.Select(m => m.Id).ToList()
            });
        }

        public async Task SendTypingIndicator(string mentorshipMatchId, bool isTyping)
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var fullName = Context.User?.FindFirst("FullName")?.Value;
            var roomName = $"mentorship_{mentorshipMatchId}";

            await Clients.OthersInGroup(roomName).SendAsync("TypingIndicator", new
            {
                UserId = userId,
                UserName = fullName,
                IsTyping = isTyping
            });
        }
    }
}