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
using LoymaxTestBot.Models.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

            //await botClient.SendTextMessageAsync(
            //                           message.Chat.Id,
            //                           "new: " + update.Type.ToString());

            if (update.Type == UpdateType.Message)
            {
                switch (message.Text)
                {
                    case "/register":
                        {
                            //CHECK: User already registered
                            var person = await _context.Person.FindAsync(message.Chat.Id);
                            if (person != null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "You are already registered! You can /view or /delete registered data");
                                return Ok();
                            }

                            Command command = Bot.Commands.Single(com => com.Name == "/register");
                            command.Execute(message, botClient);

                            StreamWriter sw = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt", false);
                            sw.Write("firstName ");
                            sw.Close();
                            break;
                        }
                    case "/start":
                        {
                            Command command = Bot.Commands.Single(com => com.Name == "/start");
                            command.Execute(message, botClient);
                            break;        
                        }
                    case "/view":
                        {
                            var person = await _context.Person.FindAsync(message.Chat.Id);

                            if (person == null)
                            {
                                InlineKeyboardButton button = new InlineKeyboardButton();
                                button.Text = "\U0001F4DD Register";
                                button.CallbackData = "register";
                                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(button);
                                await botClient.SendTextMessageAsync(message.Chat.Id, "No registered data yet. Press /register to create account", replyMarkup: keyboard);
                            }
                            else
                            {
                                InlineKeyboardButton viewButton = new InlineKeyboardButton();
                                viewButton.Text = "\U0001F4C2 View";
                                viewButton.CallbackData = "view";

                                InlineKeyboardButton deleteButton = new InlineKeyboardButton();
                                deleteButton.Text = "\U0000274C Delete";
                                deleteButton.CallbackData = "delete";

                                var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                keyboardButtons[0] = viewButton;
                                keyboardButtons[1] = deleteButton;

                                keyboardInline[0] = keyboardButtons;

                                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                string reply = "\U00002139 Full Name: " + person.SecondName + " " + person.FirstName;
                                reply += " " + person.Patronymic + "\r\n\U0001F4C5 Date of birth: ";
                                reply += person.DateBirth.ToShortDateString();
                                await botClient.SendTextMessageAsync(message.Chat.Id, reply, replyMarkup: keyboard);
                            }
                                
                            break;
                        }
                    case "/delete":
                        {
                            var person = await _context.Person.FindAsync(message.Chat.Id);

                            if (person == null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "No registered data yet. Press /register to create account");
                            }
                            else
                            {
                                InlineKeyboardButton yesButton = new InlineKeyboardButton();
                                yesButton.Text = "\U0001F4A2 Yes";
                                yesButton.CallbackData = "delete-yes";

                                InlineKeyboardButton noButton = new InlineKeyboardButton();
                                noButton.Text = "\U0000274E No";
                                noButton.CallbackData = "delete-no";

                                var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                keyboardButtons[0] = yesButton;
                                keyboardButtons[1] = noButton;

                                keyboardInline[0] = keyboardButtons;

                                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                await botClient.SendTextMessageAsync(message.Chat.Id, "Are you really want to erase account data?", replyMarkup: keyboard);

                                
                            }
                            break;
                        }
                }

                //Registering process
                if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt") && !message.Text.Contains('/'))
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
                                    if (message.Text != "/register")
                                    {
                                        await botClient.SendTextMessageAsync(
                                            message.Chat.Id,
                                            "Hi " + message.Text + "! Enter your second name",
                                            ParseMode.Markdown);


                                        sw.WriteLine(message.Text);
                                        sw.Write("secondName ");
                                        sw.Close();
                                    }
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
                                    sw.Close();
                                    try
                                    { 
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

                                        InlineKeyboardButton viewButton = new InlineKeyboardButton();
                                        viewButton.Text = "\U0001F4C2 View";
                                        viewButton.CallbackData = "view";

                                        InlineKeyboardButton deleteButton = new InlineKeyboardButton();
                                        deleteButton.Text = "\U0000274C Delete";
                                        deleteButton.CallbackData = "delete";

                                        var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                        var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                        keyboardButtons[0] = viewButton;
                                        keyboardButtons[1] = deleteButton;

                                        keyboardInline[0] = keyboardButtons;

                                        InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                        await botClient.SendTextMessageAsync(
                                                message.Chat.Id,
                                                "Great! You are registered! Now you can /view or /delete registered data",
                                                replyMarkup: keyboard);
                                        System.IO.File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + message.Chat.Id + ".txt");

                                    }
                                    catch (Exception er)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, er.ToString() + "\r\nIncorrect data input. Please try again: YYYY-MM-DD");
                                    }
                                break;
                                }

                        }
                }
            }

            if (update.CallbackQuery != null)
            {
                try
                {
                    var data = update.CallbackQuery.Data;
                    var chatId = update.CallbackQuery.Message.Chat.Id;

                    switch (data)
                    {
                        case "register":
                            {
                                //CHECK: User already registered
                                var person = await _context.Person.FindAsync(chatId);
                                if (person != null)
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId,
                                        "You are already registered! You can /view or /delete registered data");
                                    return Ok();
                                }

                                Command command = Bot.Commands.Single(com => com.Name == "/register");
                                command.Execute(update.CallbackQuery.Message, botClient);

                                StreamWriter sw = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + chatId + ".txt", false);
                                sw.Write("firstName ");
                                sw.Close();
                                break;
                            }
                        case "delete-no":
                            {
                                var person = await _context.Person.FindAsync(chatId);

                                if (person == null)
                                {
                                    InlineKeyboardButton button = new InlineKeyboardButton();
                                    button.Text = "\U0001F4DD Register";
                                    button.CallbackData = "register";
                                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(button);
                                    await botClient.SendTextMessageAsync(chatId, "No registered data yet. Press /register to create account", replyMarkup: keyboard);
                                }
                                else
                                {

                                    InlineKeyboardButton viewButton = new InlineKeyboardButton();
                                    viewButton.Text = "\U0001F4C2 View";
                                    viewButton.CallbackData = "view";

                                    InlineKeyboardButton deleteButton = new InlineKeyboardButton();
                                    deleteButton.Text = "\U0000274C Delete";
                                    deleteButton.CallbackData = "delete";

                                    var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                    var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                    keyboardButtons[0] = viewButton;
                                    keyboardButtons[1] = deleteButton;

                                    keyboardInline[0] = keyboardButtons;

                                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                    string reply = "\U00002139 Full Name: " + person.SecondName + " " + person.FirstName;
                                    reply += " " + person.Patronymic + "\r\n\U0001F4C5 Date of birth: ";
                                    reply += person.DateBirth.ToShortDateString();
                                    await botClient.SendTextMessageAsync(chatId, reply, replyMarkup: keyboard);
                                }
                                break;
                            }
                        case "view":
                            {
                                var person = await _context.Person.FindAsync(chatId);

                                if (person == null)
                                {
                                    InlineKeyboardButton button = new InlineKeyboardButton();
                                    button.Text = "\U0001F4DD Register";
                                    button.CallbackData = "register";
                                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(button);
                                    await botClient.SendTextMessageAsync(chatId, "No registered data yet. Press /register to create account", replyMarkup: keyboard);
                                }
                                else
                                {
                                    InlineKeyboardButton viewButton = new InlineKeyboardButton();
                                    viewButton.Text = "\U0001F4C2 View";
                                    viewButton.CallbackData = "view";

                                    InlineKeyboardButton deleteButton = new InlineKeyboardButton();
                                    deleteButton.Text = "\U0000274C Delete";
                                    deleteButton.CallbackData = "delete";

                                    var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                    var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                    keyboardButtons[0] = viewButton;
                                    keyboardButtons[1] = deleteButton;

                                    keyboardInline[0] = keyboardButtons;

                                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                    string reply = "\U00002139 Full Name: " + person.SecondName + " " + person.FirstName;
                                    reply += " " + person.Patronymic + "\r\n\U0001F4C5 Date of birth: ";
                                    reply += person.DateBirth.ToShortDateString();
                                    await botClient.SendTextMessageAsync(chatId, reply, replyMarkup: keyboard);
                                }

                                break;
                            }
                        case "delete-yes":
                            {
                                var person = await _context.Person.FindAsync(chatId);

                                if (person == null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "No registered data yet. Press /register to create account");
                                }
                                else
                                {
                                    _context.Person.Remove(person);
                                    await _context.SaveChangesAsync();
                                    await botClient.SendTextMessageAsync(chatId, "Data removed succesfully! You can always /register again.");
                                }
                                break;
                            }
                        case "delete":
                            {
                                var person = await _context.Person.FindAsync(chatId);

                                if (person == null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "No registered data yet. Press /register to create account");
                                }
                                else
                                {
                                    InlineKeyboardButton yesButton = new InlineKeyboardButton();
                                    yesButton.Text = "\U0001F4A2 Yes";
                                    yesButton.CallbackData = "delete-yes";

                                    InlineKeyboardButton noButton = new InlineKeyboardButton();
                                    noButton.Text = "\U0000274E No";
                                    noButton.CallbackData = "delete-no";

                                    var keyboardInline = new InlineKeyboardButton[1][]; //Rows = 1
                                    var keyboardButtons = new InlineKeyboardButton[2]; //Columns = 2
                                    keyboardButtons[0] = yesButton;
                                    keyboardButtons[1] = noButton;

                                    keyboardInline[0] = keyboardButtons;

                                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardInline);

                                    await botClient.SendTextMessageAsync(chatId, "Are you really want to erase account data?", replyMarkup: keyboard);


                                }
                                break;
                            }
                        
                    }
                }
                catch(Exception er)
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, er.ToString());
                }
                
            }
            return Ok();
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
    }
}