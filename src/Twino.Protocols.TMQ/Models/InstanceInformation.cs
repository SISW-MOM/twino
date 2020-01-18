using Newtonsoft.Json;

namespace Twino.Protocols.TMQ.Models
{
    /// <summary>
    /// Instance information
    /// </summary>
    public class InstanceInformation
    {
        /// <summary>
        /// If true, connection is outgoing.
        /// This server is sending data to remote.
        /// If false, connection is incoming.
        /// Remote server is sending data tis server.
        /// </summary>
        [JsonProperty("slave")]
        public bool IsSlave { get; set; }

        /// <summary>
        /// True, if connection is alive
        /// </summary>
        [JsonProperty("connected")]
        public bool IsConnected { get; set; }

        /// <summary>
        /// Instance host name
        /// </summary>
        [JsonProperty("host")]
        public string Host { get; set; }

        /// <summary>
        /// Instance unique id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Instance name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// lifetime in milliseconds
        /// </summary>
        [JsonProperty("lifetime")]
        public long Lifetime { get; set; }
    }
}