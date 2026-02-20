using Newtonsoft.Json;

namespace MorrisvilleDiscordBot
{
    readonly struct BotConfig
    {
        public const string JsonLocation = "config.json";

        [JsonProperty("token")]
        public readonly string Token;

        [JsonProperty("email_from")]
        public readonly string EmailFrom;

        [JsonProperty("email_username")]
        public readonly string EmailUsername;

        [JsonProperty("email_password")]
        public readonly string EmailPassword;

        [JsonProperty("smtp_host")]
        public readonly string SmtpHost;

        [JsonProperty("smtp_port")]
        public readonly int SmtpPort;

        [JsonProperty("email_regex")]
        public readonly string EmailRegex;
    }

    public class ServerConfig
    {
        public const string JsonLocation = "serverConfig.json";

        [JsonProperty("role_mappings")]
        public Dictionary<ulong, ulong> guildRoleMappings = new();
    }
}
