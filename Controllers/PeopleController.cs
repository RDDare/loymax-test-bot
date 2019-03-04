using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using LoymaxTestBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LoymaxTestBot.Controllers
{
    [Route("api/message/update")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly PersonContext _context;
        private static char[] separator = { ' ' };
        private static char[] birthseparator = { '-' };

        public PeopleController(PersonContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update == null) return Ok();

            var commands = Bot.Commands;
            var message = update.Message;
           
            var botClient = await Bot.GetBotClientAsync();

            if (update.Type == UpdateType.Message)
            {
                foreach (var command in commands)
                {
                    if (command.Name == message.Text)
                    {
                        switch (command.Name)
                        {
                            case "/register":
                            {
                                //CHECK: User already registered
                                var person = await _context.Person.FindAsync(message.Chat.Id);
                                if (person != null)
                                {
                                    await botClient.SendTextMessageAsync(
                                        message.Chat.Id,
                                        "You are already registered! You can /view or /delete registered data",
                                        ParseMode.Markdown);
                                    return Ok();
                                }

                                command.Execute(message, botClient);

                                StreamWriter sw = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt", false);
                                sw.Write("firstName ");
                                sw.Close();
                                break;
                            }
                            case "/start":
                            {
                                command.Execute(message, botClient);
                                break;        
                            }
                            case "/view":
                            {
                                var person = await _context.Person.FindAsync(message.Chat.Id);

                                if (person == null)
                                {
                                    await botClient.SendTextMessageAsync(
                                            message.Chat.Id,
                                            "No registered data yet. Press /register to create account");

                                }
                                else
                                {
                                    string reply = "Full Name: " + person.SecondName + " " + person.FirstName;
                                    reply += " " + person.Patronymic + "\r\nDate of birth: ";
                                    reply += person.DateBirth.ToShortDateString();
                                    await botClient.SendTextMessageAsync(message.Chat.Id, reply);
                                }
                                
                                break;
                            }
                            case "/delete":
                                {
                                    var person = await _context.Person.FindAsync(message.Chat.Id);

                                    if (person == null)
                                    {
                                        await botClient.SendTextMessageAsync(
                                                message.Chat.Id,
                                                "No registered data yet. Press /register to create account");

                                    }
                                    else
                                    {
                                        _context.Person.Remove(person);
                                        await _context.SaveChangesAsync();
                                        await botClient.SendTextMessageAsync(
                                            message.Chat.Id,
                                            "Data removed succesfully! You can always /register again.");
                                    }
                                    break;
                                }
                        }
                        
                    }
                }

                //Registering process
                if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt") &&
                    !message.Text.Contains('/'))
                {
                    StreamReader sr = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt");
                    List<string> parameters = new List<string>();
                    while (!sr.EndOfStream) parameters.Add(sr.ReadLine());
                    sr.Close();
                    StreamWriter sw = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt", true);
                    switch (parameters.Count)
                    {
                        case 1:
                        {
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Hi " + message.Text + "! Enter your second name",
                                ParseMode.Markdown);
                        

                            sw.WriteLine(message.Text);
                            sw.Write("secondName ");
                            sw.Close();
                            break;
                        }
                        case 2:
                        {
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Enter your patronymic",
                                ParseMode.Markdown);
                            sw.WriteLine(message.Text);
                            sw.Write("patronymic ");
                            sw.Close();
                            break;
                        }
                        case 3:
                        {
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Fine! Now set your date of birth in format YYYY-MM-DD",
                                ParseMode.Markdown);
                            sw.WriteLine(message.Text);
                            sw.Write("birthdate ");
                            sw.Close();
                            break;
                        }
                        case 4:
                        {
                            try
                            {
                                sw.WriteLine(message.Text);
                                sw.Close();
                                Person newPerson = new Person();
                                newPerson.Id = message.Chat.Id;
                                newPerson.FirstName = parameters[0].Split(separator, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                                newPerson.SecondName = parameters[1].Split(separator, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                                newPerson.Patronymic = parameters[2].Split(separator, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                                List<string> date = message.Text.Split(birthseparator, 3, StringSplitOptions.RemoveEmptyEntries).ToList();
                                                
                                int year = Convert.ToInt32(date[0]);
                                int month = Convert.ToInt32(date[1]);
                                int day = Convert.ToInt32(date[2]);

                                DateTime birthDate = new DateTime(year, month, day);
                                newPerson.DateBirth = birthDate;
                                _context.Person.Add(newPerson);
                                await _context.SaveChangesAsync();
                                await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Great! You are registered! Now you can /view or /delete registered data",
                                    ParseMode.Markdown);
                                System.IO.File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt");
                                
                            }
                            catch (Exception er)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, er.ToString());
                            }
                            break;
                        }

                    }
                }
            }

            if (update.CallbackQuery != null) //This shit is always null, wtf bro?
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                                                    "callback not null: text is" + update.CallbackQuery.Message.Text,
                                                    ParseMode.Markdown);
            }
            return Ok();
        }


        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
    }
}