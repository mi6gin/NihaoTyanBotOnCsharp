using NihaoTyan.Bot.commandsList.userCommands;
using NihaoTyan.Bot.commandsList.userCommands.Models;
using NihaoTyan.Bot.commandsList.userCommands.YtDlp;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.Threading.Tasks;

namespace NihaoTyan.Bot
{
    class Program
    {
        private static CallbackQueryHandler _callbackHandler = new CallbackQueryHandler();

        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(Config.Token);
            SQLitePCL.Batteries.Init();

            // Инициализация базы данных
            using (var context = new StepToFDbContext())
            {
                context.Database.EnsureCreated();
            }

            var botInfo = await botClient.GetMeAsync();
            Console.Title = botInfo.Username;
            Console.WriteLine($"Bot @{botInfo.Username} запущен.");
            botClient.StartReceiving();
            // Подписка на обработку входящих сообщений
            botClient.OnMessage += async (_, args) => 
                await Commands.HandleCommandsAsync(args.Message, botClient);
            
            botClient.OnMessage += DedInside.OnMessage;
            botClient.OnMessage += CallbackQueryHandler.WorkTimeAsync;
            botClient.OnMessage += Tikitoki.TTMessage;
            botClient.OnMessage += Vkudahti.VKMessage;

            // Подписка на обработку callback-запросов
            botClient.OnCallbackQuery += async (_, args) =>
                await _callbackHandler.CallbackQueryHandlerSTF(args.CallbackQuery, args.CallbackQuery.Message);
            
            Console.ReadLine();

            // Остановка получения сообщений при завершении
            botClient.StopReceiving();
        }
    }
}