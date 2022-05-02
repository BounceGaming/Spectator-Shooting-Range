using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace ShootingRange.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Range : ICommand
    {
        public string Command => "range";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Transports you to the shooting range";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Player.Get(sender) is Player player)
            {
                if (PluginMain.Singleton.Config.RequirePermission && !sender.CheckPermission("range"))
                {
                    response = "No tienes permiso para usar este comando.";
                    return false;
                }

                if (Respawn.TimeUntilRespawn < 20)
                {
                    response = "Un respawn se acerca, no puedes entrar al campo de tiro.";
                    return false;
                }
            
                if (!PluginMain.Singleton.ActiveRange.TryAdmit(player))
                {
                    response = "No eres espectador o el campo de tiro está deshabilitado en estos momentos";
                    return false;
                }

                response = "Bienvenido al campo de tiro.";
                return true;    
            }
            
            response = "No puedes ejecutar este comando desde el servidor.";
            return false;
        }
    }
}
