using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using NihaoTyan.Bot.commandsList.userCommands.FSM;

namespace  NihaoTyan.Bot.commandsList.userCommands
{
    public class CallbackQueryHandler
    {
        // Текущее состояние конечного автомата (FSM) для обработки действий пользователя
        private static Fsm.FSM _state;
        private static long _userChatId;
        private static int _userMessageId;
        private static readonly TelegramBotClient botClient = new TelegramBotClient(Config.Token);

        /// <summary>
        /// Обрабатывает входящие callback-запросы для режима StepToFreedom.
        /// Валидирует пользователя и вызывает соответствующие методы.
        /// </summary>
        public async Task CallbackQueryHandlerSTF(CallbackQuery callbackQuery, Message message)
        {
            _state = Fsm.FSM.JobStart;
            _userChatId = callbackQuery.Message.Chat.Id;
            _userMessageId = callbackQuery.Message.MessageId;

            string data = callbackQuery.Data;
            // Извлекаем userId из callback-данных и убеждаемся, что запрос пришёл от правильного пользователя
            if (int.TryParse(data.Substring(data.IndexOf('F') + 1), out int userId) && callbackQuery.From.Id == userId)
            {
                if (data.StartsWith("sSTF"))
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, string.Empty, true);
                    await botClient.DeleteMessageAsync(_userChatId, _userMessageId);
                    await HandleStart(callbackQuery);
                }
                else if (data.StartsWith("cSTF"))
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, string.Empty, true);
                    await botClient.DeleteMessageAsync(_userChatId, _userMessageId);
                }
                else if (data.StartsWith("chSTF"))
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, string.Empty, true);
                    await HandleChange(message);
                }
            }
        }

        /// <summary>
        /// Инициирует запуск цикла таймера для режима StepToFreedom.
        /// </summary>
        private async Task HandleStart(CallbackQuery callbackQuery)
        {
            _state = Fsm.FSM.FinishFSM;
            await StepToFreedomHelper.HandleStepToFreedomTimerAsync(callbackQuery, botClient);
        }

        /// <summary>
        /// Запрашивает у пользователя ввод новых значений таймера для работы и отдыха.
        /// </summary>
        private async Task HandleChange(Message message)
        {
            _state = Fsm.FSM.WrkTme;
            if (message.Chat.Id == _userChatId)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Пожалуйста, читайте внимательно!!!\nОтправьте сообщение в таком стиле:\n\n`Работа: число1, Отдых: число2`",
                    parseMode: ParseMode.Markdown);
            }
        }

        /// <summary>
        /// Обрабатывает входящее сообщение с новыми значениями таймера.
        /// Метод должен быть подписан на событие получения новых сообщений.
        /// </summary>
        public static async void WorkTimeAsync(object sender, MessageEventArgs e)
        {
            if (_state == Fsm.FSM.WrkTme && e.Message.Chat.Id == _userChatId && e.Message.Type == MessageType.Text)
            {
                // Извлекаем числа для работы и отдыха из сообщения с помощью регулярного выражения
                var regex = new Regex(@"Работа:\s*(\d+),\s*Отдых:\s*(\d+)", RegexOptions.IgnoreCase);
                var match = regex.Match(e.Message.Text);

                if (match.Success)
                {
                    int workTime = int.Parse(match.Groups[1].Value);
                    int restTime = int.Parse(match.Groups[2].Value);
                    await UpdateTimer(e.Message, workTime, restTime);
                }
                else
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Не удалось обновить таймер");
                    _state = Fsm.FSM.JobStart;
                }
            }
        }

        /// <summary>
        /// Обновляет настройки таймера пользователя в базе данных и уведомляет его об обновлении.
        /// </summary>
        private static async Task UpdateTimer(Message message, int workTime, int restTime)
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                $"Таймер обновлен. Работа: {workTime} минут, Отдых: {restTime} минут.");
            await StepToFreedom.UpdateUserTimerSettingAsync(message.From.Id, workTime, restTime);
        }
    }
}
