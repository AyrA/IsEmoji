namespace AyrA.IsEmoji
{
    /// <summary>
    /// Holds information about an emoji
    /// </summary>
    public class EmojiInfo : IStreamSerializable
    {
        /// <summary>
        /// Unocode codepoints to make up the current emoji
        /// </summary>
        /// <remarks>
        /// These are true unicode codepoints, and not UTF encoded
        /// </remarks>
        public List<int> CodePoints { get; private set; } = new List<int>();

        /// <summary>
        /// The emoji as a string
        /// </summary>
        public string Emoji { get; private set; } = string.Empty;

        /// <summary>
        /// The Unicode specification that describes the emoji
        /// </summary>
        public string Specification { get; private set; } = string.Empty;

        /// <summary>
        /// The qualifier of the emoji
        /// </summary>
        public EmojiQualifier Qualifier { get; private set; }

        /// <summary>
        /// The name of the emoji
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Creates a new emoji
        /// </summary>
        /// <param name="name">Emoji name</param>
        /// <param name="emoji">Emoji as string</param>
        /// <param name="specification">Unicode specification</param>
        /// <param name="qualifier">Emoji qualifier</param>
        /// <param name="codePoints">Unicode codepoints</param>
        public EmojiInfo(string name, string emoji, string specification, EmojiQualifier qualifier, IEnumerable<int> codePoints)
        {
            CodePoints.AddRange(codePoints);
            Emoji = emoji;
            Specification = specification;
            Qualifier = qualifier;
            Name = name;
        }

        /// <summary>
        /// Deserializes an existing emoji
        /// </summary>
        /// <param name="br"></param>
        public EmojiInfo(BinaryReader br)
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
            bw.Write(Emoji);
            bw.Write(Specification);
            bw.Write((byte)Qualifier);
            bw.Write(CodePoints.Count);
            foreach (int codePoint in CodePoints)
            {
                bw.Write(codePoint);
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
            var emoji = br.ReadString();
            var specification = br.ReadString();
            var qualifier = (EmojiQualifier)br.ReadByte();
            var codePoints = Enumerable.Range(0, br.ReadInt32()).Select(m => br.ReadInt32()).ToList();

            if (!Enum.IsDefined(typeof(EmojiQualifier), qualifier))
            {
                throw new InvalidDataException("Unknown enum value in serialized data");
            }

            Name = name;
            Emoji = emoji;
            Specification = specification;
            Qualifier = qualifier;
            CodePoints = codePoints;
        }

        /// <summary>
        /// Converts this instance to a string for display purposes
        /// </summary>
        /// <returns>DIsplay string</returns>
        public override string ToString()
        {
            return $"{Emoji} {Name}";
        }
    }
}