using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface ISteamCmdManager
    {
        void SetupSteamCmd(string path);
        bool VerifySteamCmd(string path);
    }
}
