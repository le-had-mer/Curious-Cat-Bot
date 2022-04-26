using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curious_Cat.Commands
{
    //[Group("test"), Description("funny commands to test basic features")]
    class TestCommands : BaseCommandModule
    {
        private const string eo = "eo";
        private const string mw = "mw";

        NeuralNetwork currentCat;

        private string ToMeow(string notMeow)
        {
            Random rnd = new Random();

            string[] words = Regex.Split(notMeow, @"[\W\d]+", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

            string meows = "";

            for (int i = 0; i < words.Length; i++)
            {

                for (int j = 0; j < words[i].Length; j++)
                {
                    if (Regex.IsMatch(words[i][j].ToString(), @"[aeiouyаеёиоуыэюя]", RegexOptions.IgnoreCase)) meows += eo[rnd.Next(0, 2)];
                    else meows += mw[rnd.Next(0, 2)];
                }
                meows += " ";
            }

            if (meows.Length > 0)
            {
                meows = meows.Remove(meows.Length - 1);
                meows = meows.Substring(0, 1).ToUpper() + meows.Substring(1);
            }

            return meows;
        }

        [Command("meow"), Aliases("ping", "voice")]
        [Description("Meows")]
        public async Task Meow(CommandContext ctx)
        {
            int ping = ctx.Client.Ping;
            string post;
            if (ping >= 2000)
                post = "...";
            else if (ping < 200)
                post = "!!!";
            else post = "!..";

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Meow{post} [{ping}ms]").ConfigureAwait(false);
        }

        [Command("count"), Aliases("meowtimes")]
        [Description("Meows multiple times")]
        public async Task Count(CommandContext ctx, [Description("How much")] int count)
        {
            await ctx.TriggerTypingAsync();

            string str = "";

            for (int i = count; i > 0; --i) str += "meow-";
            if (count > 0)
            {
                str = str.Remove(str.Length - 1);
                str = str.Substring(0, 1).ToUpper() + str.Substring(1);
            }

            await Task.Delay(count * 50);

            await ctx.RespondAsync(str).ConfigureAwait(false);
        }

        [Command("startrepeat"), Aliases("repeat")]
        [Description("Trying to repeat after you")]
        public async Task Startrepeat(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Meow?"); 

            var interactivity = ctx.Client.GetInteractivity();

            do
            {
                await ctx.TriggerTypingAsync();

                await Task.Delay(100);

                var message = await interactivity.WaitForMessageAsync(x => 
                (x.Channel == ctx.Channel) 
                && (ctx.Client.CurrentUser != x.Author), TimeSpan.FromSeconds(15));
                
                if (!message.TimedOut)
                {
                   await ctx.RespondAsync(ToMeow(message.Result.Content));
                }
                else
                {
                    await ctx.RespondAsync("Meo...");
                    break;
                }
                
            }
            while (true);
        }

        [Command("say")]
        [Description("Trying to meow what you say")]
        public async Task Say(CommandContext ctx, [Description("What")] params string[] speech)
        {
            await ctx.TriggerTypingAsync();

            await Task.Delay(100);

            string message = "";

            foreach (string str in speech)
            {
                message += str + " ";
            }
                        
            await ctx.RespondAsync(ToMeow(message));
        }

        [Command("bye"), Aliases("goodnight")]
        [Description("Idles bot")]
        public async Task Bye(CommandContext ctx, [Description("What")] params string[] speech)
        {
            await ctx.TriggerTypingAsync();            
            await ctx.RespondAsync("Me... ow...");

            await ctx.Client.UpdateStatusAsync(new DiscordActivity("kitty's fluffy dreams"), UserStatus.Idle);
            await Task.Delay(15000);
            await ctx.Client.UpdateStatusAsync(null, UserStatus.Online);
        }

        [Command("pet")]
        [Description("Pet for purr")]
        public async Task Pet(CommandContext ctx)
        {
            string[] emojis = { ":back_of_hand:", ":cut_of_meat:", ":cucumber:" };

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emojis[0]));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emojis[1]));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emojis[2]));

            var interactivity = ctx.Client.GetInteractivity();

            var a = await interactivity.WaitForReactionAsync(re =>
            re.Message == ctx.Message
            && re.User != ctx.Client.CurrentUser
            && (re.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[0]))
            | (re.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[1]))
            | (re.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[2])), TimeSpan.FromSeconds(15));

            if (!a.TimedOut)
            {
                await ctx.TriggerTypingAsync();
                if (a.Result.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[0]))
                {
                    await ctx.RespondAsync($"[{emojis[0]}] [Purr]... [purr]... [purr]...");
                    await ctx.Message.DeleteAllReactionsAsync();
                }
                else if (a.Result.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[1]))
                {
                    await ctx.RespondAsync($"[{emojis[1]}] [Champ]... [crunch]... [champ]...");
                    await ctx.Message.DeleteAllReactionsAsync();
                }
                else if (a.Result.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[2]))
                {
                    Random rnd = new Random();
                    int length = rnd.Next(5, 20);
                    string wtfLang = ".,?!@#№$%^&*+\"=~:;";
                    string wtf = "";
                    for (int i = 0; i < length; i++)
                    {
                        wtf += wtfLang[rnd.Next(0, wtfLang.Length)];
                    }
                    await ctx.RespondAsync($"[{emojis[2]}] [{wtf}]");
                    await ctx.Message.DeleteAllReactionsAsync();
                }
            }
            else
            {
                await ctx.RespondAsync($"[:zzz:] . . .");
                await ctx.Message.DeleteAllReactionsAsync();
            }
        }

        [Command("born")]
        [Description("New cat")]
        public async Task Born(CommandContext ctx, [Description("Who")]string name, [Description("Quality")]double quality)
        {
            await ctx.TriggerTypingAsync();
            double q = quality;
            Logic logic = new Logic(20, 4, 32, 5);
            NeuralNetwork cat;
            do
            {
                cat = logic.RunSchedulePrimitive(10000, q);
                q -= quality * 0.01;
            } while (cat == null);

            currentCat = cat;

            BinaryFormatter formatter = new BinaryFormatter();

            await using(FileStream fs = new FileStream($"{name}.kit", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, cat);

                Console.WriteLine($"{name}.kit was saved sucsesfully.");
                await ctx.RespondAsync($"[{name}.kit was saved sucsesfully]");
            }
        }

        [Command("rise")]
        [Description("Existing cat")]
        public async Task Rise(CommandContext ctx, [Description("Who")]string name)
        {
            await ctx.TriggerTypingAsync();

            BinaryFormatter formatter = new BinaryFormatter();

            FileStream fs = new FileStream($"{name}.kit", FileMode.OpenOrCreate);
            
            currentCat = (NeuralNetwork)formatter.Deserialize(fs);

            Console.WriteLine($"{name}.kit was loaded sucsesfully");
            await ctx.RespondAsync($"[{name}.kit was loaded sucsesfully]");
        }

        [Command("run")]
        [Description("Run cat")]
        public async Task Run(CommandContext ctx)
        {
            LogicSec life = new LogicSec(ctx, currentCat);

            Console.WriteLine($"Simulation was started");
            await ctx.RespondAsync($"[Simulation was started]");

            life.RunRealtime();


        }
    }
}
