using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using LoymaxTestBot.Models.Commands;

namespace LoymaxTestBot.Models
{
    public class Bot
    {
        private static TelegramBotClient botClient;
        private static List<Command> commandList;
        //private static readonly IReadOnlyList<Command> Commands;

        public static IReadOnlyList<Command> Commands { get => commandList.AsReadOnly(); }

        public static async Task<TelegramBotClient> GetBotClientAsync()
        {
            if (botClient != null) return botClient;

            //Initialize commands
            commandList = new List<Command>();
            commandList.Add(new RegisterCommand());
            commandList.Add(new StartCommand());
            commandList.Add(new ViewCommand());
            commandList.Add(new DeleteCommand());

            botClient = new TelegramBotClient(BotSettings.Key);
            var webHook = string.Format(BotSettings.Url, "api/message/update");

            //web-hook
            await botClient.SetWebhookAsync(webHook);

            return botClient;
        }
    }
}
