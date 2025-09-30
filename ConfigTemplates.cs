namespace PulsePanel
{
    public static class ConfigTemplates
    {
        public static Dictionary<string, Dictionary<string, string>> GetTemplates()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                ["Counter-Strike 2"] = new()
                {
                    ["server.cfg"] = @"// Counter-Strike 2 Server Configuration
hostname ""My CS2 Server""
sv_password """"
rcon_password ""changeme""
sv_cheats 0
mp_autoteambalance 1
mp_limitteams 2
mp_roundtime 2
mp_freezetime 15
mp_maxrounds 30
mp_startmoney 800
mp_buytime 90
mp_c4timer 35
sv_alltalk 0
sv_deadtalk 0
mp_friendlyfire 0
mp_tkpunish 1
sv_pausable 0
log on
sv_logbans 1
sv_logecho 1
sv_logfile 1
sv_log_onefile 0"
                },

                ["Garry's Mod"] = new()
                {
                    ["server.cfg"] = @"// Garry's Mod Server Configuration
hostname ""My GMod Server""
sv_password """"
rcon_password ""changeme""
gamemode ""sandbox""
map ""gm_flatgrass""
maxplayers 16
sbox_maxprops 200
sbox_maxragdolls 10
sbox_maxvehicles 4
sbox_maxeffects 50
sbox_maxdynamite 10
sbox_maxlamps 20
sbox_maxthrusters 30
sbox_maxwheels 20
sbox_maxhoverballs 20
sbox_maxballoons 100
sbox_maxnpcs 10
sbox_godmode 0
sbox_noclip 0
sbox_playershurtplayers 1
sv_alltalk 1
sv_voiceenable 1"
                },

                ["Arma 3"] = new()
                {
                    ["server.cfg"] = @"// Arma 3 Server Configuration
hostname = ""My Arma 3 Server"";
password = """";
passwordAdmin = ""changeme"";
serverCommandPassword = ""changeme"";
maxPlayers = 64;
motd[] = {
    ""Welcome to my Arma 3 Server"",
    ""Have fun and follow the rules!""
};
admins[] = {};
headlessClients[] = {};
localClient[] = {};
voteMissionPlayers = 3;
voteThreshold = 0.33;
disableVoN = 0;
vonCodec = 1;
vonCodecQuality = 30;
persistent = 1;
timeStampFormat = ""short"";
BattlEye = 1;
verifySignatures = 2;
kickDuplicate = 1;
upnp = 0;
allowedFilePatching = 0;
filePatchingExceptions[] = {};
class Missions {
    class Mission1 {
        template = ""Altis_Life.Altis"";
        difficulty = ""regular"";
    };
};"
                },

                ["Rust"] = new()
                {
                    ["server.cfg"] = @"# Rust Server Configuration
server.hostname ""My Rust Server""
server.description ""Welcome to my Rust server!""
server.url """"
server.headerimage """"
server.logoimage """"
server.maxplayers 100
server.worldsize 4000
server.seed 12345
server.saveinterval 300
server.globalchat true
server.stability true
server.radiation true
rcon.password ""changeme""
rcon.port 28016
server.port 28015
server.queryport 28017
decay.scale 1.0
craft.instant false
gather.rate.dispenser 1.0
server.pve false
server.pvp true"
                },

                ["Valheim"] = new()
                {
                    ["start_server.bat"] = @"@echo off
valheim_server.exe -nographics -batchmode -name ""My Valheim Server"" -port 2456 -world ""Dedicated"" -password ""secret"" -public 1
pause"
                },

                ["7 Days to Die"] = new()
                {
                    ["serverconfig.xml"] = @"<?xml version=""1.0""?>
<ServerSettings>
    <property name=""ServerName"" value=""My 7DTD Server""/>
    <property name=""ServerDescription"" value=""A 7 Days to Die server""/>
    <property name=""ServerWebsiteURL"" value=""""/>
    <property name=""ServerPassword"" value=""""/>
    <property name=""ServerLoginConfirmationText"" value=""""/>
    <property name=""Region"" value=""NorthAmericaEast""/>
    <property name=""Language"" value=""English""/>
    <property name=""ServerPort"" value=""26900""/>
    <property name=""ServerVisibility"" value=""2""/>
    <property name=""ServerDisabledNetworkProtocols"" value=""SteamNetworking""/>
    <property name=""ServerMaxWorldTransferSpeedKiBs"" value=""1300""/>
    <property name=""ServerMaxPlayerCount"" value=""8""/>
    <property name=""ServerReservedSlots"" value=""0""/>
    <property name=""ServerReservedSlotsPermission"" value=""100""/>
    <property name=""ServerAdminSlots"" value=""0""/>
    <property name=""ServerAdminSlotsPermission"" value=""0""/>
    <property name=""GameWorld"" value=""Navezgane""/>
    <property name=""WorldGenSeed"" value=""asdf""/>
    <property name=""WorldGenSize"" value=""4096""/>
    <property name=""GameName"" value=""My Game""/>
    <property name=""GameMode"" value=""GameModeSurvival""/>
    <property name=""GameDifficulty"" value=""2""/>
    <property name=""BlockDamagePlayer"" value=""100""/>
    <property name=""BlockDamageAI"" value=""100""/>
    <property name=""BlockDamageAIBM"" value=""100""/>
    <property name=""XPMultiplier"" value=""100""/>
    <property name=""PlayerSafeZoneLevel"" value=""5""/>
    <property name=""PlayerSafeZoneHours"" value=""5""/>
    <property name=""BuildCreate"" value=""false""/>
    <property name=""DayNightLength"" value=""60""/>
    <property name=""DayLightLength"" value=""18""/>
    <property name=""DeathPenalty"" value=""1""/>
    <property name=""DropOnDeath"" value=""1""/>
    <property name=""DropOnQuit"" value=""0""/>
    <property name=""BedrollDeadZoneSize"" value=""15""/>
    <property name=""BedrollExpiryTime"" value=""45""/>
</ServerSettings>"
                },

                ["Squad"] = new()
                {
                    ["Server.cfg"] = @"[/Script/SquadGame.SQGameMode]
bAllowTeamChanges=true
bRecordDemoFile=false
bUseVehicleRespawnSystemV2=true

[/Script/SquadGame.SQTeamInfo]
TeamLimit=40

[/Script/Engine.GameSession]
MaxPlayers=80

[/Script/SquadGame.SQGameState]
PreRoundTime=90
RoundTime=2700
PostRoundTime=180

[/Script/SquadGame.SQPlayerController]
VoipEnabled=true
AllowProfanity=false

[/Script/SquadGame.SQGameMode]
ShouldDisplayTeamkillInfo=true
TeamkillLimit=3
TeamkillReduceLimit=1
TkAutoKickEnabled=true"
                },

                ["ARK: Survival Evolved"] = new()
                {
                    ["GameUserSettings.ini"] = @"[ServerSettings]
SessionName=My ARK Server
ServerPassword=
ServerAdminPassword=changeme
MaxPlayers=70
DifficultyOffset=1.0
ServerPVE=False
ServerCrosshair=True
ServerMapPlayerLocation=True
ServerHardcore=False
GlobalVoiceChat=False
ProximityChat=False
NoTributeDownloads=False
AllowThirdPersonPlayer=True
AlwaysNotifyPlayerLeft=True
DontAlwaysNotifyPlayerJoined=True
ServerForceNoHUD=False
ShowMapPlayerLocation=True
EnablePVPGamma=True
DisableStructureDecayPvE=False
DisableStructureDecayPvP=False
DisableDinoDecayPvE=False
DisableDinoDecayPvP=False
PvEStructureDecayPeriodMultiplier=1.0
PvEStructureDecayDestructionPeriod=0
Banlist=""""
ResourcesRespawnPeriodMultiplier=1.0
TamingSpeedMultiplier=1.0
HarvestAmountMultiplier=1.0
PlayerCharacterWaterDrainMultiplier=1.0
PlayerCharacterFoodDrainMultiplier=1.0
DinoCharacterFoodDrainMultiplier=1.0
PlayerCharacterStaminaDrainMultiplier=1.0
DinoCharacterStaminaDrainMultiplier=1.0
PlayerCharacterHealthRecoveryMultiplier=1.0
DinoCharacterHealthRecoveryMultiplier=1.0
DinoCountMultiplier=1.0
PvPStructureDecay=False
StructurePreventResourceRadiusMultiplier=1.0
PlatformSaddleBuildAreaBoundsMultiplier=1.0
AllowAnyoneBabyImprintCuddle=False
BabyImprintingStatScaleMultiplier=1.0
BabyCuddleIntervalMultiplier=1.0
BabyCuddleGracePeriodMultiplier=1.0
BabyCuddleLoseImprintQualitySpeedMultiplier=1.0
BabyFoodConsumptionSpeedMultiplier=1.0
DinoTurretDamageMultiplier=1.0
ItemStackSizeMultiplier=1.0
RCONEnabled=True
RCONPort=27020"
                },

                ["DayZ"] = new()
                {
                    ["serverDZ.cfg"] = @"hostname = ""My DayZ Server"";
password = """";
passwordAdmin = ""changeme"";
maxPlayers = 60;
verifySignatures = 2;
forceSameBuild = 1;
disableVoN = 0;
vonCodec = 1;
vonCodecQuality = 20;
disable3rdPerson = 0;
disableCrosshair = 0;
disablePersonalLight = 1;
lightingConfig = 1;
serverTime = ""SystemTime"";
serverTimeAcceleration = 0;
serverNightTimeAcceleration = 1;
serverTimePersistent = 0;
guaranteedUpdates = 1;
loginQueueConcurrentPlayers = 5;
loginQueueMaxPlayers = 500;
instanceId = 1;
storeHouseStateDisabled = false;
storageAutoFix = 1;
defaultVisibility = 1;
defaultObjectViewDistance = 1375;
lightingConfig = 1;
disableBaseDamage = 0;
disableContainerDamage = 0;
disableRespawnDialog = 0;
enableDebugMonitor = 0;
allowFilePatching = 0;
simulatedPlayersBatch = 20;
multithreadedReplication = 1;
speedhackDetection = 1;
networkRangeClose = 20;
networkRangeNear = 150;
networkRangeFar = 1000;
networkRangeDistantEffect = 4000;"
                }
            };
        }

        public static string GetTemplate(string gameName, string fileName)
        {
            var templates = GetTemplates();
            if (templates.TryGetValue(gameName, out var gameTemplates) && 
                gameTemplates.TryGetValue(fileName, out var template))
            {
                return template;
            }
            
            return $"# {fileName} for {gameName}\n# Add your configuration here\n";
        }

        public static List<string> GetConfigFiles(string gameName)
        {
            var templates = GetTemplates();
            return templates.TryGetValue(gameName, out var gameTemplates) 
                ? gameTemplates.Keys.ToList() 
                : new List<string> { "server.cfg" };
        }
    }
}