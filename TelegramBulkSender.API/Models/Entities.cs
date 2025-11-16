namespace TelegramBulkSender.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsRoot { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}

public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}

public class UserLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Chat
{
    public long Id { get; set; }
    public long TelegramChatId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsClient { get; set; }
    public bool IsSystemChat { get; set; }
    public string? Language { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ChatGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsGlobal { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public ICollection<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
    public DateTime CreatedAt { get; set; }
}

public class ChatGroupMember
{
    public int Id { get; set; }
    public int ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = default!;
    public long ChatId { get; set; }
}

public class MessageTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TextRu { get; set; } = string.Empty;
    public string TextEn { get; set; } = string.Empty;
    public bool IsGlobal { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<TemplateFile> Files { get; set; } = new List<TemplateFile>();
}

public class TemplateFile
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public MessageTemplate Template { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsImage { get; set; }
}

public class Broadcast
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string TextRu { get; set; } = string.Empty;
    public string TextEn { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalChats { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public ICollection<BroadcastMessage> Messages { get; set; } = new List<BroadcastMessage>();
    public ICollection<BroadcastFile> Files { get; set; } = new List<BroadcastFile>();
}

public class BroadcastMessage
{
    public int Id { get; set; }
    public int BroadcastId { get; set; }
    public Broadcast Broadcast { get; set; } = default!;
    public long ChatId { get; set; }
    public int? TelegramMessageId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
}

public class BroadcastFile
{
    public int Id { get; set; }
    public int BroadcastId { get; set; }
    public Broadcast Broadcast { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsImage { get; set; }
    public long FileSize { get; set; }
}
