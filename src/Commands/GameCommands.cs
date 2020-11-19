using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

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

            var message = $"{calendar.PrettyDate()}" +
                $"{Environment.NewLine}It is currently {Enum.GetName(typeof(EnumSeason), calendar.GetSeason(api.World.DefaultSpawnPosition.AsBlockPos))}.";

            await context.RespondAsync(message);
        }

        [Command("players")]
        [Aliases("onlineplayers", "playerlist")]
        [Description("Shows the currently online players and their play time")]
        public async Task OnlinePlayersAsync(CommandContext context)
        {
            var clients = api.World.AllOnlinePlayers.Select(x => new { x.PlayerName, SessionLengthInMinutes = (int)(DateTime.Now.AddHours(2) - DiscordWatcher.connectTimeDict[x.PlayerUID]).TotalMinutes });

            var embed = new DiscordEmbedBuilder().WithTitle("Currently online players:")
                .WithDescription(clients.Select(x => $"Name: '{x.PlayerName}'" +
                $" - Playtime: {(x.SessionLengthInMinutes > 120 ? (x.SessionLengthInMinutes / 60) + " hours and " + (x.SessionLengthInMinutes % 60) : x.SessionLengthInMinutes.ToString()) } minutes" )
                .Aggregate("", (acc, str) => acc += (str + "\n")))
                .Build();

            await context.RespondAsync("", embed: embed);
        }
    }
}
