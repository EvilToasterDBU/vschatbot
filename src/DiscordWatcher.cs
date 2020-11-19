using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using System.Reflection;
using Vintagestory.API.Config;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using Vintagestory.GameContent;
using vschatbot.src.Utils;

namespace vschatbot.src
{
    public class DiscordWatcher : ModSystem
    {
        public static ICoreServerAPI Api;

        private ICoreServerAPI api 
        {
            get => Api;
            set => Api = value; 
        }
        private ModConfig config;
        private DiscordClient client;
        private CommandsNextModule commands;
        private DiscordChannel discordChannel;

        private dynamic lastData;
        private object temporalSystem;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            try
            {
                this.config = api.LoadModConfig<ModConfig>("vschatbot.json");
            }
            catch (Exception e)
            {
                api.Server.LogError("vschatbot: Failed to load mod config!");
                return;
            }

            if (this.config == null)
            {
                api.Server.LogNotification("vschatbot: non-existant modconfig at vschatbot.json, creating default and shutting down...");
                api.StoreModConfig(new ModConfig() { Token = "insert bot token here" }, "vschatbot.json");

                return;
            }
            else if (this.config.Token == "insert bot token here" || this.config.ChannelId == default || this.config.ServerId == default)
            {
                api.Server.LogError("vschatbot: invalid modconfig at vschatbot.json!");
                return;
            }

            this.api = api;

            Task.Run(async () => await this.MainAsync());

            this.api.Event.SaveGameLoaded += Event_SaveGameLoaded;
            this.api.Event.PlayerChat += Event_PlayerChat;
            this.api.Event.PlayerJoin += Event_PlayerJoin;
            this.api.Event.PlayerDisconnect += Event_PlayerDisconnect;
        }

        private void Event_PlayerDisconnect(IServerPlayer byPlayer)
        {
            sendDiscordMessage($"{byPlayer.PlayerName} has disconnected from the server! " +
                $"({api.Server.Players.Count(x => x.PlayerUID != byPlayer.PlayerUID && x.ConnectionState == EnumClientState.Playing)}" +
                $"/{api.Server.Config.MaxClients})");
        }

        private void Event_PlayerJoin(IServerPlayer byPlayer)
        {
            sendDiscordMessage($"{byPlayer.PlayerName} has connected to the server! " +
                $"({api.Server.Players.Count(x => x.ConnectionState != EnumClientState.Offline)}" +
                $"/{api.Server.Config.MaxClients})");
        }

        private void sendDiscordMessage(string message = "", DiscordEmbed embed = null)
        {
            this.client.SendMessageAsync(this.discordChannel, message, embed: embed);
        }

        private async Task MainAsync()
        {
            this.client = new DiscordClient(new DiscordConfiguration()
            {
                Token = this.config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug
            });

            this.client.Ready += Client_Ready;
            this.client.MessageCreated += Client_MessageCreated;
            this.client.ClientErrored += Client_ClientErrored;

            var commandConfiguration = new CommandsNextConfiguration
            {
                StringPrefix = "!",
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };

            this.commands = this.client.UseCommandsNext(commandConfiguration);
            this.commands.RegisterCommands(Assembly.GetExecutingAssembly());

            try
            {
                await this.client.ConnectAsync();
            }
            catch (Exception)
            {
                this.api.Server.LogError("vschatbot: Failed to login using token...");
                return;
            }

            await Task.Delay(-1);
        }

        private Task Client_ClientErrored(ClientErrorEventArgs e)
        {
            api.Server.LogError("vschatbot: Disconnected from Discord...", e.Exception);
            api.Server.LogError("vschatbot: " + e.Exception.ToString());

            return Task.FromResult(true);
        }

        private void Event_SaveGameLoaded()
        {
            temporalSystem = api.ModLoader.GetModSystem<SystemTemporalStability>();

            if (api.World.Config.GetString("temporalStorms") != "off")
            {
                api.Event.RegisterGameTickListener(onTempStormTick, 5000);
            }
        }

        private void onTempStormTick(float t1)
        {
            var dataField = typeof(SystemTemporalStability).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
            dynamic data = dataField.GetValue(temporalSystem).ToDynamic();

            if (lastData?.stormDayNotify == 1 && data.stormDayNotify == 0)
            {
                var embed = new DiscordEmbedBuilder()
                .WithTitle("Harketh the storm doth come, Wary be thine self, as for thy own end be near.")
                .WithColor(DiscordColor.Red);

                sendDiscordMessage(embed: embed);
            }

            double activeDaysLeft = data.stormActiveTotalDays - api.World.Calendar.TotalDays;
            if (lastData?.stormDayNotify == 0 && data.stormDayNotify == -1)
            {
                var embed = new DiscordEmbedBuilder()
                .WithTitle("The temporal storm seems to be waning")
                .WithColor(DiscordColor.Green);

                sendDiscordMessage(embed: embed);
            }

            lastData = data;
        }

        private Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel?.Id != this.discordChannel.Id)
                return Task.FromResult(true);

            var content = e.Message.Content;
            MatchCollection matches = new Regex(@"\<\@\!(\d+)\>").Matches(content);
            try
            {
                var foundUsers = 0;

                foreach (Match match in matches)
                {
                    var id = ulong.Parse(match.Groups[1].Value);

                    if (e.Message.MentionedUsers?.Count() > foundUsers)
                    {
                        content = content.Replace(match.Groups[0].Value, "@" + client.GetUserAsync(ulong.Parse(match.Groups[1].Value))?.ConfigureAwait(false).GetAwaiter().GetResult().Username ?? "unknown");
                        foundUsers++;
                    }
                    //else if (e.Message.MentionedChannels.Any(x => x.Id == id))
                    //    content = content.Replace(match.Groups[0].Value, "@" + client.GetChannelAsync(ulong.Parse(match.Groups[1].Value))?.ConfigureAwait(false).GetAwaiter().GetResult().Name ?? "unknown");
                    //else if (e.Message.MentionedRoles.Any(x => x.Id == id))
                    //    content = content.Replace(match.Groups[0].Value, "@" + client.GetGuildAsync(config.ServerId)?.ConfigureAwait(false).GetAwaiter().GetResult().GetRole(ulong.Parse(match.Groups[1].Value))?.Name ?? "unknown");
                }
            }
            catch (Exception)
            {
                this.api.Server.LogError($"vschatbot: Something went wrong while trying to parse the message '{e.Message.Content}'...");
                sendDiscordMessage($"Unfortunately {e.Author.Username}, " +
                    $"an internal error occured while handling your message... (Blame {this.api.World.AllOnlinePlayers?.RandomElement()?.PlayerName ?? "Capsup"} or something)");
                return Task.FromResult(true);
            }

            api.SendMessageToGroup(GlobalConstants.AllChatGroups, $"Discord|<strong>{e.Author.Username}</strong>: {content.Replace(">", "&gt;").Replace("<", "&lt;")}", EnumChatType.Notification);

            return Task.FromResult(true);
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            this.api.Server.LogNotification("vschatbot: connected to discord and ready!");

            this.discordChannel = this.client.GetChannelAsync(this.config.ChannelId).ConfigureAwait(false).GetAwaiter().GetResult();

            return Task.FromResult(true);
        }

        private void Event_PlayerChat(IServerPlayer byPlayer, int channelId, ref string message, ref string data, Vintagestory.API.Datastructures.BoolRef consumed)
        {
            if (channelId == GlobalConstants.GeneralChatGroup)
            {
                var foundText = new Regex(@".*?> (.+)$").Match(message);
                if (!foundText.Success)
                    return;

                sendDiscordMessage($"**{byPlayer.PlayerName}**: {foundText.Groups[1].Value}");
            }
        }
    }
}
