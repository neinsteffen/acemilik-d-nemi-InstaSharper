using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InstaToTelegram
{
    class Program
    {
        static ITelegramBotClient botClient;
        private const string username = "username";
        private const string password = "password";
        private static UserSessionData user;
        private static IInstaApi api;
        static void Main(string[] args)
        {
            user = new UserSessionData();
            user.UserName = username;
            user.Password = password;
            Login();
            System.Threading.Thread.Sleep(5000);
            botClient = new TelegramBotClient("your token");
            var m = botClient.GetMeAsync().Result;
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Bot runned \n*");
            Console.ReadKey();

            botClient.StopReceiving();

        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Bot proccessing datas...\n*");
            System.Threading.Thread.Sleep(5000);
            //login side
            var logged = await api.LoginAsync();
            var loginRequest = await api.LoginAsync();
            if (logged.Succeeded)
            {
                IResult<InstaUser> userSearch = await api.GetUserAsync(user.UserName);

                //Console.WriteLine(string.Format("User: {0} \n\t Followers: {1} \n\t \n\t Verifeid: {2}", userSearch.Value.FullName, userSearch.Value.FollowersCount, userSearch.Value.IsVerified));
                IResult<InstaMediaList> media = await api.GetUserMediaAsync(user.UserName, PaginationParameters.MaxPagesToLoad(6));
                List<InstaMedia> mediaList = media.Value.ToList();

                //get posts from instagram and send to telegram bot side
                for (int i = 0; i < mediaList.Count; i++)
                {
                    InstaMedia m = mediaList[i];
                    if (m != null && m.Caption != null)
                    {
                        string captionText = m.Caption.Text;
                        if (captionText != null)
                        {



                            if (m.MediaType == InstaMediaType.Image && m.Images[i].URI != null && m.Images[i].URI != null)
                            {
                                //getting img url
                                string uri = m.Images[i].URI;
                                string info = mediaList[i].InstaIdentifier;
                                int hashDecode = mediaList[i].GetHashCode();
                                //System.Console.WriteLine(Convert.ToString(i + 1) + ".nci POST IMG ->" + info + " " + uri + "\n" + "\n\t hashcode: " + hashDecode);
                                //System.Console.WriteLine(Convert.ToString(i + 1) + ".nci POST MASSEGE -> " + captionText + "\n");
                                System.Threading.Thread.Sleep(3000);
                                //Telegram bot
                                if (e.Message.Text != null && e.Message.Text.ToLower() == "getir")
                                {
                                    Console.WriteLine($"Last message -> {e.Message.Chat.Id}.");

                                    //Send Message
                                    Message message = await botClient.SendTextMessageAsync(
                                       chatId: e.Message.Chat,
                                       text: captionText,

                                       parseMode: ParseMode.Markdown,
                                       disableNotification: true,
                                       //replyToMessageId: e.Message.MessageId,
                                       replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(
                                       "",
                                       "https://core.telegram.org/bots/api#sendmessage"
                                     ))
                                      );
                                    //Send Image
                                    Message photo = await botClient.SendPhotoAsync(
                                    chatId: e.Message.Chat,
                                    photo: uri,
                                    parseMode: ParseMode.Html
                                   );



                                }

                            }







                        }
                    }
                }

            }


        }//Instagram get data
        private static async void Login()
        {   //set api
            api = InstaApiBuilder.CreateBuilder()
                .SetUser(user)
                .UseLogger(new DebugLogger(InstaSharper.Logger.LogLevel.Exceptions))
                .SetRequestDelay(RequestDelay.FromSeconds(8, 8))
                .Build();
            //make login request 
            var loginRequest = await api.LoginAsync();
            Console.WriteLine("Logging in!");
            //if the connection is success
            if (loginRequest.Succeeded)
            {

                Console.WriteLine("Connecting to account.");
                Console.WriteLine(user.UserName + " Login Successful");
                System.Threading.Thread.Sleep(3000);


            }
            else
                System.Console.WriteLine("Login Error!" + loginRequest.Info.Message);
        }
    }
}