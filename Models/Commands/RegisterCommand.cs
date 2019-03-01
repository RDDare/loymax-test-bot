using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoymaxTestBot.Models.Commands
{
    public class RegisterCommand : Command
    {
        public override string Name => "register";

        public override async void Execute(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            //TODO: command-logic

            await botClient.SendTextMessageAsync(chatId, "Nice try mr. Freeman", replyToMessageId: messageId); 
        }
    }
}
