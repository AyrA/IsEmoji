namespace AyrA.IsEmoji
{
    /// <summary>
    /// Represents a group of emoji.
    /// The group is the top level of grouping
    /// </summary>
    public class EmojiGroup : IStreamSerializable
    {
        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; private set; } = string.Empty;
        /// <summary>
        /// Subgroups
        /// </summary>
        public List<EmojiSubgroup> Subgroups { get; private set; } = new();

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="name">Group name</param>
        public EmojiGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Deserializes an existing group
        /// </summary>
        /// <param name="br">Binary reader</param>
        public EmojiGroup(BinaryReader br)
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
            bw.Write(Subgroups.Count);
            foreach (var subgroup in Subgroups)
            {
                subgroup.Serialize(bw);
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
            var subgroups = Enumerable.Range(0, br.ReadInt32()).Select(m => new EmojiSubgroup(br)).ToList();

            Name = name;
            Subgroups = subgroups;
        }

        /// <summary>
        /// Converts this instance to a string for display purposes
        /// </summary>
        /// <returns>DIsplay string</returns>
        public override string ToString()
        {
            return $"Emoji group: {Name}";
        }
    }
}