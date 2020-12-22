using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using vschatbot.src.Models;

namespace vschatbot.src.Commands
{
    public class GameCommands
    {
        private ICoreServerAPI api = DiscordWatcher.Api;

        [Command("time")]
        [Aliases("season")]
        [Description("Returns the current server time and the season")]
        public async Task GetTimeAsync(CommandContext context)
        {
            var calendar = api.World.Calendar;

            var embed = new DiscordEmbedBuilder().WithTitle("Time and season:")
                .WithDescription($"{calendar.PrettyDate()}{Environment.NewLine}It is currently {Enum.GetName(typeof(EnumSeason), calendar.GetSeason(api.World.DefaultSpawnPosition.AsBlockPos))}.")
                .Build();

            await context.RespondAsync("", embed: embed);
        }

        [Command("players")]
        [Aliases("onlineplayers", "playerlist", "online", "list")]
        [Description("Shows the currently online players and their play time")]
        public async Task OnlinePlayersAsync(CommandContext context)
        {
            var playerData = this.api.World.AllOnlinePlayers.Select(player => {
                var newPlayerData = new OnlinePlayerData() { PlayerName = player.PlayerName };

                newPlayerData.SessionLengthInMinutes = (int)(DateTime.UtcNow - DiscordWatcher.connectTimeDict[player.PlayerUID]).TotalMinutes;
                newPlayerData.TotalPlaytimeInMinutes = newPlayerData.SessionLengthInMinutes;

                string playtimeJson = "";
                if (this.api.PlayerData.GetPlayerDataByUid(player.PlayerUID)?.CustomPlayerData?.TryGetValue(DiscordWatcher.PLAYERDATA_TOTALPLAYTIMEKEY, out playtimeJson) ?? false)
                {
                    newPlayerData.TotalPlaytimeInMinutes += (int) JsonConvert.DeserializeObject<TimeSpan>(playtimeJson).TotalMinutes;
                }

                return newPlayerData;
            });

            string StringifyTime(int time)
            {
                return $"{(time > 120 ? (time / 60) + " hours and " + (time % 60) : time.ToString())} minute{(time % 60 == 1 ? "" : "s")}";
            }

            var embed = new DiscordEmbedBuilder().WithTitle($"Currently online players ({this.api.World.AllOnlinePlayers.Count()}/{this.api.Server.Config.MaxClients}):")
                .WithDescription(playerData.Select(x => $"Name: '{x.PlayerName}'" +
                $" - Session playtime: {StringifyTime(x.SessionLengthInMinutes)}" +
                $" - Total playtime: {StringifyTime(x.TotalPlaytimeInMinutes)}")
                .Aggregate("", (acc, str) => acc += (str + "\n")))
                .Build();

            await context.RespondAsync("", embed: embed);
        }

        [Command("lastseen")]
        [Aliases("seen", "lastonline")]
        [Description("Shows the last time a player was connected with the specific playername")]
        public async Task LastSeenAsync(CommandContext context, [Description("The player's name to search for")] string name)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("Last seen activity:");

            var isOnline = this.api.World.AllOnlinePlayers.FirstOrDefault(x => x.PlayerName.ToLower() == name.ToLower()) != null;
            if (isOnline)
            {
                embed.WithDescription($"Well... He's online right now!");
                await context.RespondAsync("", embed: embed);
                return;
            }

            var playerData = api.PlayerData.GetPlayerDataByLastKnownName(name);
            if (playerData == null)
            {
                embed.WithDescription($"No player data found for '{name}'").Build();
                await context.RespondAsync("", embed: embed);
                return;
            }

            if (!playerData.CustomPlayerData.TryGetValue(DiscordWatcher.PLAYERDATA_LASTSEENKEY, out var lastSeenJson))
            {
                embed.WithDescription($"Player data for '{name}' found, but he hasn't been seen since this bot was installed...").Build();
                await context.RespondAsync("", embed: embed);
                return;
            }

            var data = JsonConvert.DeserializeObject<DateTime>(lastSeenJson);
            embed.WithDescription($"The last time '{name}' was seen, was {data:f} in UTC timezone");
            await context.RespondAsync("", embed: embed);
            return;
        }
    }
}
