using Telegram.Bot;
using Telegram.Bot.Types;

namespace NihaoTyan.Bot.commandsList.userCommands
{
    public static class Anecdote
    {
        private static readonly Random _random = new Random(); // Используем один экземпляр Random для избежания повторений
        private static readonly string FilePath = Path.Combine(
            "DreamGirl", "commandsList", "userCommands", "mediaFiles", "anekdots.txt");



        public static async Task SendAsync(Message message, TelegramBotClient botClient)
        {
            if (!System.IO.File.Exists(FilePath))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Вся жизнь твоя – шутка \ud83d\ude06");
                return;
            }

            var content = await System.IO.File.ReadAllTextAsync(FilePath);
            var anecdotes = content.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (anecdotes.Length == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "В файле нет анекдотов.");
                return;
            }

            string randomAnecdote = anecdotes[_random.Next(anecdotes.Length)];

            // Telegram ограничивает длину сообщения 4096 символами
            if (randomAnecdote.Length > 4096)
            {
                randomAnecdote = randomAnecdote.Substring(0, 4093) + "...";
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, randomAnecdote);
        }
    }
}