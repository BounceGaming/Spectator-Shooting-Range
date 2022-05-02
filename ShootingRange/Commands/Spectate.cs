using System;
using CommandSystem;
using Exiled.API.Features;

namespace ShootingRange.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Spectate : ICommand
    {
        public string Command => "spectate";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Returns you to spectating if you are on the range";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Player.Get(sender) is Player player)
            {
                if (PluginMain.Singleton.ActiveRange.DeletePlayer(player))
                {
                    player.ClearInventory();
                    player.SetRole(RoleType.Spectator);
                    response = "Has sido enviado a espectador de nuevo.";
                    return true;
                }

                response = "No estás en el campo de tiro.";
                return false;   
            }

            response = "No puedes ejecutar este comando desde el servidor.";
            return false;
        }
    }
}
