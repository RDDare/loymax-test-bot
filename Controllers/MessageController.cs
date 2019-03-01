using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using LoymaxTestBot.Models;

namespace LoymaxTestBot.Controllers
{
    [Route(@"api/message/update")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Method unavailable";
        }

        //[Route(@"api/message/update")] //webhook uri part
        //public async Task<OkResult> Update([FromBody]Update update)
        //{
        //    var commands  = Bot.Commands;
        //    var message   = update.Message;
        //    var botClient = await Bot.GetBotClientAsync();
        //    foreach (var command in commands)
        //    {
        //        if (command.Contains(message.Text))
        //        {
        //            command.Execute(message, botClient);
        //            break;
        //        }
        //    }

        //    return Ok();
        //}

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update == null) return Ok();

            var commands = Bot.Commands;
            var message = update.Message;
            var botClient = await Bot.GetBotClientAsync();
            foreach (var command in commands)
            {
                if (command.Contains(message.Text))
                {
                    command.Execute(message, botClient);
                    break;
                }
            }

            return Ok();
        }
    }
}
