using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NihaoTyan.Bot.commandsList
{
    public class Listofcommands
    {
        private static bool _commandsRegistered = false;

        public static async Task AllCommands(Message message, TelegramBotClient botClient)
        {
            try
            {
                // Регистрация команд в Telegram только если они еще не зарегистрированы
                if (!_commandsRegistered)
                {
                    var commands = new List<BotCommand>
                    {
                        new BotCommand { Command = "start", Description = "Поздороватся с Нихао" },
                        new BotCommand { Command = "help", Description = "Позвать Нихао" },
                        new BotCommand { Command = "dedinside", Description = "Позвать собутылника" },
                        new BotCommand { Command = "stf", Description = "Режим прогулки" },
                        new BotCommand { Command = "anecdote", Description = "Расскажи анекдот про Штирлица" }
                    };

                    await botClient.SetMyCommandsAsync(commands);
                    _commandsRegistered = true; // Устанавливаем флаг, что команды зарегистрированы
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
