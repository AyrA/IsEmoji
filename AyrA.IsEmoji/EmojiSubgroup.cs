namespace AyrA.IsEmoji
{
    /// <summary>
    /// Represents a subgroup of emoji.
    /// This is the second level of grouping
    /// </summary>
    public class EmojiSubgroup : IStreamSerializable
    {
        /// <summary>
        /// Subgroup name
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// List of emojis in the current subgroup
        /// </summary>
        public List<EmojiInfo> Emoji { get; private set; } = new();

        /// <summary>
        /// Creates a new subgroup
        /// </summary>
        /// <param name="name">Subgroup name</param>
        public EmojiSubgroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Deserializes an existing subgroup
        /// </summary>
        /// <param name="br">Binary reader</param>
        public EmojiSubgroup(BinaryReader br)
        {
            Deserialize(br);
        }

        /// <summary>
        /// Serializes data into a stream for later deserialization using <see cref="Deserialize(BinaryReader)"/>
        /// </summary>
        /// <param name="bw">Binary writer</param>
        public void Serialize(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Emoji.Count);
            foreach (var emoji in Emoji)
            {
                emoji.Serialize(bw);
            }
        }

        /// <summary>
        /// Deserializes the current instance from a stream
        /// previously created with <see cref="Serialize(BinaryWriter)"/>
        /// </summary>
        /// <param name="br">Binary reader</param>
        public void Deserialize(BinaryReader br)
        {
            var name = br.ReadString();
            var infos = Enumerable.Range(0, br.ReadInt32()).Select(m => new EmojiInfo(br)).ToList();

            Name = name;
            Emoji = infos;
        }

        /// <summary>
        /// Converts this instance to a string for display purposes
        /// </summary>
        /// <returns>DIsplay string</returns>
        public override string ToString()
        {
            return $"Emoji subgroup: {Name}";
        }
    }
}