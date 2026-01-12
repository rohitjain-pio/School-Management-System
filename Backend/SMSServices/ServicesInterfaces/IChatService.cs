using SMSDataModel.Model.Models;

namespace SMSServices.ServicesInterfaces
{
    public interface IChatService
    {
        Task<ChatRoom?> GetRoomAsync(string roomId);
        Task<bool> CreateRoomAsync(ChatRoom room);
        Task<bool> ValidateRoomPasswordAsync(string roomId, string password);
        Task<bool> IsUserParticipantAsync(string roomId, string userId);
        Task<bool> ValidateRoomAccessAsync(string roomId, string userId);
        void AddUserToRoom(string roomId, string connectionId, string username, string userId);
        (string Username, string UserId)? RemoveUserFromRoom(string roomId, string connectionId);
        List<string> GetRoomUsernames(string roomId);
        bool IsUserInRoom(string roomId, string connectionId);
        bool CheckFloodProtection(string userId, string roomId);
        void CleanupUserTracking(string userId);
        List<string> RemoveUserFromAllRooms(string connectionId);
        Task UpdateRoomActivityAsync(string roomId);
        Task<ChatMessage> SaveMessageAsync(string roomId, string userId, string content, bool isEncrypted);
        Task<List<ChatMessage>> GetMessageHistoryAsync(string roomId, int count = 50);
        string EncryptMessage(string message, string roomId);
        string DecryptMessage(string encryptedMessage, string roomId);
        string SanitizeMessage(string message);
    }
}
