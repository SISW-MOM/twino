using Newtonsoft.Json;

namespace Twino.Protocols.TMQ.Models
{
    /// <summary>
    /// Queue information
    /// </summary>
    public class QueueInformation
    {
        /// <summary>
        /// Queue channel name
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// Queue id
        /// </summary>
        [JsonProperty("id")]
        public ushort Id { get; set; }

        /// <summary>
        /// Pending high priority messages in the queue
        /// </summary>
        [JsonProperty("highPrioMsgs")]
        public int InQueueHighPriorityMessages { get; set; }

        /// <summary>
        /// Pending regular messages in the queue
        /// </summary>
        [JsonProperty("regularMsgs")]
        public int InQueueRegularMessages { get; set; }

        /// <summary>
        /// Queue current status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// If true, messages will send to only first acquirers
        /// </summary>
        [JsonProperty("onlyFirstAcquirer")]
        public bool SendOnlyFirstAcquirer { get; set; }

        /// <summary>
        /// If true, messages will request acknowledge from receivers
        /// </summary>
        [JsonProperty("requestAck")]
        public bool RequestAcknowledge { get; set; }

        /// <summary>
        /// When acknowledge is required, maximum duration for waiting acknowledge message
        /// </summary>
        [JsonProperty("ackTimeout")]
        public int AcknowledgeTimeout { get; set; }

        /// <summary>
        /// When message queuing is active, maximum time for a message wait
        /// </summary>
        [JsonProperty("msgTimeout")]
        public int MessageTimeout { get; set; }

        /// <summary>
        /// If true, server creates unique id for each message.
        /// </summary>
        [JsonProperty("useMsgId")]
        public bool UseMessageId { get; set; } = true;

        /// <summary>
        /// If true, queue does not send next message to receivers until acknowledge message received
        /// </summary>
        [JsonProperty("waitForAck")]
        public bool WaitForAcknowledge { get; set; }

        /// <summary>
        /// If true, server doesn't send client name to receivers in queueus.
        /// </summary>
        [JsonProperty("hideClientName")]
        public bool HideClientNames { get; set; }

        /// <summary>
        /// Total messages received from producers
        /// </summary>
        [JsonProperty("receivedMsgs")]
        public long ReceivedMessages { get; set; }

        /// <summary>
        /// Total messages sent to consumers
        /// </summary>
        [JsonProperty("sentMsgs")]
        public long SentMessages { get; set; }

        /// <summary>
        /// Total message send operation each message to each consumer
        /// </summary>
        [JsonProperty("deliveries")]
        public long Deliveries { get; set; }

        /// <summary>
        /// Total unacknowledged messages
        /// </summary>
        [JsonProperty("unacks")]
        public long Unacknowledges { get; set; }

        /// <summary>
        /// Total acknowledged messages
        /// </summary>
        [JsonProperty("acks")]
        public long Acknowledges { get; set; }

        /// <summary>
        /// Total timed out messages
        /// </summary>
        [JsonProperty("timeoutMsgs")]
        public long TimeoutMessages { get; set; }

        /// <summary>
        /// Total saved messages
        /// </summary>
        [JsonProperty("savedMsgs")]
        public long SavedMessages { get; set; }

        /// <summary>
        /// Total removed messages
        /// </summary>
        [JsonProperty("removedMsgs")]
        public long RemovedMessages { get; set; }
        
        /// <summary>
        /// Total error count
        /// </summary>
        [JsonProperty("errors")]
        public long Errors { get; set; }
        
        /// <summary>
        /// Last message receive date in UNIX milliseconds
        /// </summary>
        [JsonProperty("lastMsgReceived")]
        public long LastMessageReceived { get; set; }
        
        /// <summary>
        /// Last message send date in UNIX milliseconds
        /// </summary>
        [JsonProperty("lastMsgSent")]
        public long LastMessageSent { get; set; }
    }
}