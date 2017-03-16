namespace YoutubeExplode.Models
{
    /// <summary>
    /// User metadata
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// URL of the channel
        /// </summary>
        public string ChannelUrl => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// User's actual name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// User's public name
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Title of the user's channel
        /// </summary>
        public string ChannelTitle { get; internal set; }

        /// <summary>
        /// Whether the user's channel is paid
        /// </summary>
        public bool IsPaid { get; internal set; }

        internal UserInfo()
        {
        }

        /// <inhertidoc />
        public override string ToString()
        {
            return $"{DisplayName}";
        }
    }
}