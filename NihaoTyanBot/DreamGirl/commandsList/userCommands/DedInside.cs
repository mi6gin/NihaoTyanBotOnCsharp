using NihaoTyan.Bot.commandsList.userCommands.FSM;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;

namespace NihaoTyan.Bot.commandsList.userCommands
{
    class DedInside
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient(Config.Token);
        private static long _userId;
        private static long _chatId;
        private static Fsm.FSM _state;
        private static bool _isFirstMessage = true;

        /// <summary>
        /// Инициализирует взаимодействие с пользователем и устанавливает начальное состояние.
        /// </summary>
        public static async Task DedInsideStart(Message message, TelegramBotClient botClient)
        {
            _chatId = message.Chat.Id;
            _userId = message.From.Id;
            _state = Fsm.FSM.StartBomber;
            _isFirstMessage = true;

            await botClient.SendTextMessageAsync(_chatId, "Здравия желаю, господа-коммунисты-бояре!\nКто нас интересует?");
        }

        /// <summary>
        /// Обрабатывает упоминание пользователя.
        /// </summary>
        private static async Task HandleMention(Message message)
        {
            if (message.Chat.Id != _chatId) return;
            if (message.Type != MessageType.Text) return;

            if (_isFirstMessage && message.Text.StartsWith("@"))
            {
                _isFirstMessage = false;
                for (int i = 0; i < 15; i++)
                {
                    var sentMessage = await Bot.SendTextMessageAsync(_chatId, message.Text);
                    await Task.Delay(1000);
                    await Bot.DeleteMessageAsync(_chatId, sentMessage.MessageId);
                }
                _state = Fsm.FSM.MainBomb;
            }
            else if (_isFirstMessage)
            {
                _isFirstMessage = false;
                await Bot.SendTextMessageAsync(_chatId, "Нет-нет-нет, товарищ...\nВы делаете всё не так. Нужно указывать перед ником \"@\".\nНапример: [@githab_parasha]");
                _state = Fsm.FSM.MainBomb;
            }
        }

        /// <summary>
        /// Обработчик входящих сообщений. 
        /// </summary>
        public static async void OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Chat.Id != _chatId || message.From.Id != _userId) return;

            if (_state == Fsm.FSM.StartBomber)
            {
                await HandleMention(message);
            }
        }
    }
}
