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
using vschatbot.src.Commands;

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

        private TemporalStormRunTimeData lastData;
        private SystemTemporalStability temporalSystem;

        private const string CONFIGNAME = "vschatbot.json";

        public static Dictionary<string, DateTime> connectTimeDict = new Dictionary<string, DateTime>();

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            try
            {
                this.config = api.LoadModConfig<ModConfig>(CONFIGNAME);
            }
            catch (Exception e)
            {
                api.Server.LogError("vschatbot: Failed to load mod config!");
                return;
            }

            if (this.config == null)
            {
                api.Server.LogNotification($"vschatbot: non-existant modconfig at 'ModConfig/{CONFIGNAME}', creating default and disabling mod...");
                api.StoreModConfig(new ModConfig(), CONFIGNAME);

                return;
            }
            else if (this.config.Token == "insert bot token here" || this.config.ChannelId == default || this.config.ServerId == default)
            {
                api.Server.LogError($"vschatbot: invalid modconfig at 'ModConfig/{CONFIGNAME}'!");
                return;
            }

            this.api = api;
            Task.Run(async () => await this.MainAsync());

            this.api.Event.SaveGameLoaded += Event_SaveGameLoaded;
            if( this.config.RelayDiscordToGame )
                this.api.Event.PlayerChat += Event_PlayerChat;
            this.api.Event.PlayerJoin += Event_PlayerJoin;
            this.api.Event.PlayerDisconnect += Event_PlayerDisconnect;
            if (this.config.SendServerMessages)
            {
                this.api.Event.ServerRunPhase(EnumServerRunPhase.GameReady, Event_ServerStartup);
                this.api.Event.ServerRunPhase(EnumServerRunPhase.Shutdown, Event_ServerShutdown);
            }
            if (this.config.SendDeathMessages)
                this.api.Event.PlayerDeath += Event_PlayerDeath;
        }

        //Shout-out to Milo for texts
        private void Event_PlayerDeath(IServerPlayer byPlayer, DamageSource damageSource)
        {
            var deathMessage = (byPlayer?.PlayerName ?? "Unknown player") + " ";
            if (damageSource == null)
                deathMessage += "was killed by the unknown.";
            else
            {
                switch (damageSource.Type)
                {
                    case EnumDamageType.Gravity:
                        deathMessage += "smashed into the ground";
                        break;
                    case EnumDamageType.Fire:
                        deathMessage += "burned to death";
                        break;
                    case EnumDamageType.Crushing:
                    case EnumDamageType.BluntAttack:
                        deathMessage += "was crushed";
                        break;
                    case EnumDamageType.SlashingAttack:
                        deathMessage += "was sliced open";
                        break;
                    case EnumDamageType.PiercingAttack:
                        deathMessage += "was pierced through";
                        break;
                    case EnumDamageType.Suffocation:
                        deathMessage += "suffocated to death";
                        break;
                    case EnumDamageType.Heal:
                        deathMessage += "was somehow *healed* to death";
                        break;
                    case EnumDamageType.Poison:
                        deathMessage += "was poisoned";
                        break;
                    case EnumDamageType.Hunger:
                        deathMessage += "starved to death";
                        break;
                    default:
                        deathMessage += "was killed";
                        break;
                }

                deathMessage += " ";

                switch (damageSource.Source)
                {
                    case EnumDamageSource.Block:
                        deathMessage += "by a block.";
                        break;
                    case EnumDamageSource.Player:
                        deathMessage += "when they failed at PVP.";
                        break;
                    case EnumDamageSource.Fall:
                        deathMessage += "when they fell to their doom.";
                        break;
                    case EnumDamageSource.Drown:
                        deathMessage += "when they tried to breath in water.";
                        break;
                    case EnumDamageSource.Revive:
                        deathMessage += "just as they respawned.";
                        break;
                    case EnumDamageSource.Void:
                        deathMessage += "when they fell screaming into the abyss.";
                        break;
                    case EnumDamageSource.Suicide:
                        deathMessage += "when they killed themselves.";
                        break;
                    case EnumDamageSource.Internal:
                        deathMessage += "when they took damage from the inside...";
                        break;
                    case EnumDamageSource.Entity:
                        switch (damageSource.SourceEntity.Code.Path)
                        {
                            case "wolf-male":
                            case "wolf-female":
                                deathMessage += "and eaten by a wolf.";
                                break;
                            case "pig-wild-male":
                                deathMessage += "by a boar.";
                                break;
                            case "pig-wild-female":
                                deathMessage += "by a sow.";
                                break;
                            case "sheep-bighorn-female":
                            case "sheep-bighorn-male":
                                deathMessage += "by a sheep.";
                                break;
                            case "chicken-rooster":
                                deathMessage += "by a... chicken.";
                                break;
                            case "locust":
                                deathMessage += "by a locust.";
                                break;
                            case "drifter":
                                deathMessage += "by a drifter.";
                                break;
                            case "beemob":
                                deathMessage += "by a swarm of bees.";
                                break;
                            default:
                                deathMessage += "by a monster.";
                                break;
                        }
                        break;
                    case EnumDamageSource.Explosion:
                        deathMessage += "when they stood by a bomb.";
                        break;
                    case EnumDamageSource.Machine:
                        deathMessage += "when they got their hands stuck in a machine.";
                        break;
                    case EnumDamageSource.Unknown:
                        deathMessage += "when they encountered the unknown.";
                        break;
                    case EnumDamageSource.Weather:
                        deathMessage += "when the weather itself suddenly struck.";
                        break;
                    default:
                        deathMessage += "by the unknown.";
                        break;
                }
            }

            sendDiscordMessage(deathMessage);
        }

        private void Event_ServerShutdown()
        {
            sendDiscordMessage(this.config.TEXT_ServerStop);
        }

        private void Event_ServerStartup()
        {
            sendDiscordMessage(this.config.TEXT_ServerStart);
        }

        private void Event_PlayerDisconnect(IServerPlayer byPlayer)
        {
            DiscordWatcher.connectTimeDict.Remove(byPlayer.PlayerUID);

            sendDiscordMessage($"{byPlayer.PlayerName} has disconnected from the server! " +
                $"({api.Server.Players.Count(x => x.PlayerUID != byPlayer.PlayerUID && x.ConnectionState == EnumClientState.Playing)}" +
                $"/{api.Server.Config.MaxClients})");
        }

        private void Event_PlayerJoin(IServerPlayer byPlayer)
        {
            DiscordWatcher.connectTimeDict.Add(byPlayer.PlayerUID, DateTime.Now);

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
            if( this.config.RelayGameToDiscord )
                this.client.MessageCreated += Client_MessageCreated;
            this.client.ClientErrored += Client_ClientErrored;

            var commandConfiguration = new CommandsNextConfiguration
            {
                StringPrefix = "!",
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            };

            this.commands = this.client.UseCommandsNext(commandConfiguration);
            this.commands.RegisterCommands<GameCommands>();
            this.commands.RegisterCommands<DebugCommands>();

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
            if (this.config.SendStormNotification && api.World.Config.GetString("temporalStorms") != "off")
            {
                temporalSystem = api.ModLoader.GetModSystem<SystemTemporalStability>();
                api.Event.RegisterGameTickListener(onTempStormTick, 5000);
            }
        }

        private void onTempStormTick(float t1)
        {
            var data = this.temporalSystem.StormData;

            if (lastData?.stormDayNotify == 1 && data.stormDayNotify == 0)
            {
                var embed = new DiscordEmbedBuilder()
                .WithTitle(this.config.TEXT_StormBegin)
                .WithColor(DiscordColor.Red);

                sendDiscordMessage(embed: embed);
            }

            //double activeDaysLeft = data.stormActiveTotalDays - api.World.Calendar.TotalDays;
            if (lastData?.stormDayNotify == 0 && data.stormDayNotify == -1)
            {
                var embed = new DiscordEmbedBuilder()
                .WithTitle(this.config.TEXT_StormEnd)
                .WithColor(DiscordColor.Green);

                sendDiscordMessage(embed: embed);
            }

            lastData = data;
        }

        private Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel?.Id != this.discordChannel.Id || (e.Message.Content.StartsWith("!") && e.Message.Content.Length > 1))
                return Task.FromResult(true);

            var content = e.Message.Content;
            MatchCollection matches = new Regex(@"\<\@\!?(\d+)\>").Matches(content);
            try
            {
                var foundUsers = 0;

                foreach (Match match in matches)
                {
                    if (!match.Success || !ulong.TryParse(match.Groups[1].Value, out var id))
                        continue;

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

            var customEmojiMatches = new Regex(@"\<(\:.+\:)\d+\>").Matches(content);
            foreach (Match match in customEmojiMatches)
            {
                if (!match.Success)
                    continue;

                content = content.Replace(match.Groups[0].Value, match.Groups[1].Value);
            }

            api.SendMessageToGroup(GlobalConstants.GeneralChatGroup, $"Discord|<strong>{e.Author.Username}</strong>: {content.Replace(">", "&gt;").Replace("<", "&lt;")}", EnumChatType.OthersMessage);

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
