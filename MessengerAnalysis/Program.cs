using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessengerAnalysis;

internal static class Program
{
    internal static void Main(string[] args)
    {
        string path = args[0];
        string me = args[1];
        string output = args[2];

        var watch = Stopwatch.StartNew();
        
        // Parsing.
        Dictionary<string, Chat> chats = GetChats(path);
        
        Console.WriteLine($"Parsing done [{watch.Elapsed}]");
        watch.Restart();
        
        // Analysis.
        PerformAnalysis(me, output, chats);

        Console.WriteLine($"Statistical analysis done [{watch.Elapsed}]");
        
    }

    private static void PerformAnalysis(string me, string path, Dictionary<string, Chat> chats)
    {
        Chat allChat = new Chat()
        {
            Title = "Total"
        };
        foreach (Chat chat in chats.Values)
        {
            allChat.AddChat(chat);
        }
        chats.Add("total", allChat);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        WriteDataCsv(me, path, chats);
        foreach ((string name, Chat chat) in chats)
        {
            WriteChatData(path, name, chat);
        }
    }

    private static void WriteChatData(string path, string name, Chat chat)
    {
        using CsvStream csv = new CsvStream(Path.Combine(path, name + ".csv"));

        csv.Field("dates");
        foreach (Participant participant in chat.Participants.DistinctBy(p => p.Name))
        {
            string participantName = participant.Name;
            csv.Field($"{participantName}");
        }
        csv.Row();

        IEnumerable<Message> messages = chat.Messages.OrderBy(m => m.TimestampMs.Ticks);
        DateTime date = messages.FirstOrDefault()?.TimestampMs.Date ?? DateTime.MaxValue;
        DateTime endDate = messages.LastOrDefault()?.TimestampMs.Date ?? DateTime.MaxValue;
        IEnumerator < Message > enumerator = messages.GetEnumerator();
        enumerator.MoveNext();
        while (date <= endDate)
        {
            Dictionary<string, int> messageCount = chat.Participants.DistinctBy(p => p.Name).ToDictionary(p => p.Name, _ => 0);
            while (enumerator.Current.TimestampMs.Date == date)
            {
                string sender = enumerator.Current.SenderName;
                if (!messageCount.ContainsKey(sender))
                {
                    messageCount[sender] = 0;
                }
                messageCount[sender]++;
                if (!enumerator.MoveNext())
                    break;
            }

            csv.Field(date);
            foreach (var participant in chat.Participants)
            {
                string participantName = participant.Name;
                csv.Field(messageCount[participantName]);
            }
            csv.Row();
            date += TimeSpan.FromDays(1);
        }
        csv.Flush();
    }

    private static void WriteDataCsv(string me, string path, Dictionary<string, Chat> chats)
    {
        using CsvStream csv = new CsvStream(Path.Combine(path, "data.csv"));

        WriteDataHeader(csv);
        
        foreach ((string name, Chat chat) in chats)
        {
            List<Message> messages = chat.Messages.OrderBy(m => m.TimestampMs.Ticks).ToList();
            csv.Field(name);
            csv.Field(chat.Title);
            csv.Field(string.Join("|", chat.Participants.Select(p => p.Name)));
                
            WriteMessageStatistics(csv, messages);
            WriteMessageStatistics(csv, messages.Where(m => m.SenderName == me));
            WriteMessageStatistics(csv, messages.Where(m => m.SenderName != me));
                
            csv.Row();
        }
    }

    private static void WriteMessageStatistics(CsvStream csv, IEnumerable<Message> messages)
    {
        List<string> words = messages.SelectMany(m => m.Content.Split(" ")).ToList();
        List<char> characters = words.SelectMany(w => w.ToCharArray()).ToList();
            
        csv.Field(messages.Count());
        csv.Field(words.Count());
        csv.Field(characters.Count());
        csv.Field((double)words.Count() / messages.Count());
        csv.Field((double)characters.Count() / messages.Count());
        csv.Field((double)characters.Count() / words.Count());
        csv.Field((messages.LastOrDefault()?.TimestampMs - messages.FirstOrDefault()?.TimestampMs) / messages.Count());
    }

    private static void WriteDataHeader(CsvStream csv)
    {
        csv.Field("Chat Name");
        csv.Field("Chat Title");
        csv.Field("Participants");
        // Total.
        csv.Field("Total Messages");
        csv.Field("Total Word Count");
        csv.Field("Total Character Count");
        csv.Field("Total Words / Message");
        csv.Field("Total Characters / Message");
        csv.Field("Total Characters / Word");
        csv.Field("Average Message Interval");
        // Sent.
        csv.Field("Messages Sent");
        csv.Field("Word Count Sent");
        csv.Field("Character Count Sent");
        csv.Field("Words / Message Sent");
        csv.Field("Characters / Message Sent");
        csv.Field("Characters / Word Sent");
        csv.Field("Average Message Interval Sent");
        // Received.
        csv.Field("Messages Received");
        csv.Field("Word Count Received");
        csv.Field("Character Count Received");
        csv.Field("Words / Message Received");
        csv.Field("Characters / Message Received");
        csv.Field("Characters / Word Received");
        csv.Field("Average Message Interval Received");
        csv.Row();
    }

    private static Dictionary<string, Chat> GetChats(string path)
    {
        Dictionary<string, Chat> chats = new();
        string[] files = Directory.GetFiles(path, "message*.json", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            string name = Path.GetDirectoryName(file) ?? "";
            name = Path.GetRelativePath(Path.Combine(name, ".."), name);
            int underscoreIndex = name.IndexOf('_');
            if (underscoreIndex == -1)
            {
                continue;
            }
            name = name[..underscoreIndex];
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            using Stream stream = File.OpenRead(file);
            object? chatObj = JsonSerializer.Deserialize(stream, typeof(Chat));
            if (chatObj is not Chat chat)
            {
                continue;
            }
            
            if (chats.TryGetValue(name, out Chat? chatCache))
            {
                chatCache.AddChat(chat);
            }
            else
            {
                chats.Add(name, chat);
            }
        }

        return chats;
    }
}