using Curious_Cat.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Curious_Cat
{
    class CatBot
    {
        string token;
        
        public DiscordClient Client { get; private set; }

        public CommandsNextExtension Commands { get; private set; }

        public InteractivityExtension Interactivity { get; private set; }


        public async Task RunAsync()
        {
            var json = string.Empty;

            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(60)                
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "*" },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,

            };

            Commands = Client.UseCommandsNext(commandsConfig);
            
            Commands.RegisterCommands<TestCommands>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(object sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        public CatBot(string token)
        {
            this.token = token;
        }
    }
}
