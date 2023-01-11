using System.Text.Json.Serialization;

namespace MessengerAnalysis;

public class Chat
{
    [JsonPropertyName("participants")]
    public List<Participant> Participants { get; set; } = new ();
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new ();
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("is_still_participant")]
    public bool IsStillParticipant { get; set; } = false;
    [JsonPropertyName("thread_type")]
    public string ThreadType { get; set; } = string.Empty;
    [JsonPropertyName("thread_path")]
    public string ThreadPath { get; set; } = string.Empty;
    [JsonPropertyName("object")]
    public List<object> MagicWords { get; set; } = new ();
    [JsonPropertyName("joinable_mode")]
    public JoinableMode JoinableMode { get; set; } = new ();

    public void AddChat(Chat other)
    {
        Participants = Participants.Union(other.Participants).DistinctBy(p => p.Name).ToList();
        Messages.AddRange(other.Messages);
        MagicWords.AddRange(other.MagicWords);
    }
}

public class Message
{
    [JsonPropertyName("sender_name")]
    public string SenderName { get; set; } = string.Empty;
    [JsonPropertyName("timestamp_ms")]
    [JsonConverter(typeof(JsonDateTimeConverter))]
    public DateTime TimestampMs { get; set; } = DateTime.MinValue;
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    [JsonPropertyName("sticker")]
    public Sticker Sticker { get; set; } = new ();
    [JsonPropertyName("Reaction")]
    public List<Reaction> Reactions { get; set; } = new ();
}

public class Participant
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class Reaction
{
    [JsonPropertyName("reaction")]
    public string ReactionStr { get; set; } = string.Empty;
    [JsonPropertyName("actor")]
    public string Actor { get; set; } = string.Empty;
}

public class Sticker
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

public class JoinableMode
{
    [JsonPropertyName("mode")]
    public int Mode { get; set; }
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;
}
