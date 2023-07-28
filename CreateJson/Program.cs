using AyrA.IsEmoji;
using System.Text.Json;

try
{
    Console.Error.Write("Building cache...");
    await Emoji.AutoInit(false);
    Console.Error.WriteLine(" [DONE]");
}
catch (Exception ex)
{
    Console.Error.WriteLine(" [FAIL]");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("There was an error initializing the emoji cache");
    while (ex != null)
    {
        Console.Error.WriteLine("[{0}] {1}", ex.GetType().Name, ex.Message);
    }
    Console.ResetColor();
    return;
}

File.WriteAllText("emoji-grouped.json", JsonSerializer.Serialize(Emoji.GetAllGroups()));
File.WriteAllText("emoji-list.json", JsonSerializer.Serialize(Emoji.GetAllEmoji()));

Console.WriteLine("Emoji data exported");
