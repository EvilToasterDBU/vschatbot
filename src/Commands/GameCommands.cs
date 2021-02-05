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
        LogCountData methods = new LogCountData();

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

        private string StringifyTime(int time)
        {
            return $"{(time > 120 ? (time / 60) + " hours and " + (time % 60) : time.ToString())} minute{(time % 60 == 1 ? "" : "s")}";
        }

        [Command("showlast")]
        [Aliases("showdebuglog")]
        [Description("Returns the last n debug messages")]
        public async Task ShowDebug(CommandContext context, [Description("Count of last debug messages")] int count)
        {
            var embed = new DiscordEmbedBuilder().Build();
            string last_log = methods.ShowLast(count);
            if (last_log == null)
            {
                embed = new DiscordEmbedBuilder()
                .WithTitle("Не удается получить доступ к файлу.")
                .WithColor(DiscordColor.Red)
                .WithDescription("Файл недоступен!")
                .Build();
            }
            else
            {
                if (count > 40)
                {
                    embed = new DiscordEmbedBuilder().WithTitle("Слишком большое число логов!")
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Не больше 40")
                        .Build();
                }
                else
                {
                    embed = new DiscordEmbedBuilder().WithTitle($"Последние {count} записей в логе (если есть):")
                    .WithDescription(last_log).Build();
                }
            }
                     
            await context.RespondAsync("", embed: embed);
        }

        [Command("clearlog")]
        [Aliases("clearfile")]
        [Description("Clears the log file")]
        public async Task ClearFile(CommandContext context)
        {
            methods.Clear_Log();
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Файл лога очищен.")
                .WithColor(DiscordColor.Green)
                .WithDescription("Файл лога пуст!")
                .Build();
            await context.RespondAsync("", embed: embed);
        }

        [Command("searchlog")]
        [Aliases("findbystring")]
        [Description("Find a log by string")]
        public async Task Show_Log_Time(CommandContext context, [Description("Word for searching")] string word)
        {
            int count = 0;
            string logs = methods.Find_by_time(word, out count);
            var embed = new DiscordEmbedBuilder().Build();
            if (logs == null)
            {
                embed = new DiscordEmbedBuilder()
                .WithTitle("Не удается получить доступ к файлу.")
                .WithColor(DiscordColor.Red)
                .WithDescription("Файл недоступен!")
                .Build();
            }
            else
            {
                embed = new DiscordEmbedBuilder()
                .WithTitle($"По запросу {word} найдено {count} записей. Последние:")
                .WithDescription(logs)
                .Build();
            }            
            await context.RespondAsync("", embed: embed);
        }

        [Command("getfile")]
        [Aliases("getlogfile")]
        [Description("Get a log file")]
        public async Task Get_log_file(CommandContext context)
        {
            await context.RespondWithFileAsync("log.txt");
        }

        [Command("players")]
        [Aliases("onlineplayers", "playerlist", "online", "list")]
        [Description("Shows the currently online players and their play time")]
        public async Task OnlinePlayersAsync(CommandContext context)
        {
            var playerData = this.api.World.AllOnlinePlayers.Select(player => {
                var newPlayerData = new OnlinePlayerData() { PlayerName = player.PlayerName, SessionLengthInMinutes = -1 };

                if(DiscordWatcher.connectTimeDict.TryGetValue(player.PlayerUID, out var sessionConnect))
                    newPlayerData.SessionLengthInMinutes = (int)(DateTime.UtcNow - sessionConnect ).TotalMinutes;

                return newPlayerData;
            });

            var embed = new DiscordEmbedBuilder().WithTitle($"Currently online players ({this.api.World.AllOnlinePlayers.Count()}/{this.api.Server.Config.MaxClients}):")
                .WithDescription(playerData.Select(x => $"Name: '{x.PlayerName}'" +
                $" - Session playtime: {StringifyTime(x.SessionLengthInMinutes)}") 
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
                embed.WithDescription($"Well... They're online right now!");
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
                embed.WithDescription($"Player data for '{name}' found, but they haven't been seen since this bot was installed...").Build();
                await context.RespondAsync("", embed: embed);
                return;
            }

            var data = JsonConvert.DeserializeObject<DateTime>(lastSeenJson);
            embed.WithDescription($"The last time '{name}' was seen, was {data:f} in UTC timezone");
            await context.RespondAsync("", embed: embed);
        }

        [Command("stats")]
        [Aliases("playerinfo", "playerstats")]
        [Description("Shows more in-depth stats about a particular playername")]
        public async Task PlayerInfoAsync(CommandContext context, [Description("The player's name to search for")] string name)
        {
            var embed = new DiscordEmbedBuilder().WithTitle($"Player stats for '{name}'");
            var descriptionStringBuilder = new StringBuilder();

            var playerData = api.PlayerData.GetPlayerDataByLastKnownName(name);
            if (playerData == null)
            {
                embed.WithDescription($"No player data found for '{name}'").Build();
                await context.RespondAsync("", embed: embed);
                return;
            }
            
            //Total playtime
            var totalPlaytimeInMinutes = 0;
            if( DiscordWatcher.connectTimeDict.TryGetValue(playerData.PlayerUID, out var connectTime) )
            {
                totalPlaytimeInMinutes = (int)(DateTime.UtcNow - connectTime).TotalMinutes;
            }
            
            if (playerData.CustomPlayerData.TryGetValue(DiscordWatcher.PLAYERDATA_TOTALPLAYTIMEKEY, out var playtimeJson) )
            {
                totalPlaytimeInMinutes += (int)JsonConvert.DeserializeObject<TimeSpan>(playtimeJson).TotalMinutes;
            }

            descriptionStringBuilder.AppendLine($"Total playtime: {StringifyTime(totalPlaytimeInMinutes)}");

            //Death count
            var deathCount = 0;
            if (playerData.CustomPlayerData.TryGetValue(DiscordWatcher.PLAYERDATA_TOTALDEATHCOUNT, out var totalDeathCountJson))
            {
                deathCount += JsonConvert.DeserializeObject<int>(totalDeathCountJson);
            }

            descriptionStringBuilder.AppendLine($"Total deaths: {deathCount}");

            embed.WithDescription(descriptionStringBuilder.ToString())
                 .Build();
            await context.RespondAsync("", embed: embed);
        }
    }
}
