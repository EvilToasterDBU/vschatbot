using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using DSharpPlus.Entities;

namespace vschatbot.src.Commands
{
    [Group("debug")]
    public class DebugCommands
    {
        private ICoreServerAPI api = DiscordWatcher.Api;

        [Command("playerlist")]
        [Aliases("onlineplayers", "players")]
        public async Task OnlinePlayersAsync(CommandContext context, [Description("Should use world.AllOnlinePlayers or server.Players)")] bool useOnlinePlayers = true)
        {
            var clients = api.World.AllOnlinePlayers.Select(x => new OnlinePlayersObject() { PlayerName = x.PlayerName, ClientId = x.ClientId, ConnectionState = EnumClientState.Playing });
            if (!useOnlinePlayers)
                clients = api.Server.Players.Select(x => new OnlinePlayersObject() { PlayerName = x.PlayerName, ClientId = x.ClientId, ConnectionState = x.ConnectionState });

            var embed = new DiscordEmbedBuilder().WithTitle("Currently online players (according to VS):")
                .WithDescription(clients.Select(x => $"Name: '{x.PlayerName}'" +
                $" - Id: '{x.ClientId}'" +
                $" - State: '{Enum.GetName(typeof(EnumClientState), x.ConnectionState)}'")
                .Aggregate("", (acc, str) => acc += (str + "\n")))
                .Build();

            await context.RespondAsync("", embed: embed);
        }
    }

    public class OnlinePlayersObject
    {
        public string PlayerName { get; set; }
        public int ClientId { get; set; }
        public EnumClientState ConnectionState { get; set; }
    }
}
