namespace AyrA.IsEmoji
{
    /// <summary>
    /// Interface for stream serializable and deserializable components
    /// </summary>
    public interface IStreamSerializable
    {
        /// <summary>
        /// Deserializes the current instance from a stream
        /// previously created with <see cref="Serialize(BinaryWriter)"/>
        /// </summary>
        /// <param name="br">Binary reader</param>
        void Deserialize(BinaryReader br);
        /// <summary>
        /// Serializes data into a stream for later deserialization using <see cref="Deserialize(BinaryReader)"/>
        /// </summary>
        /// <param name="bw">Binary writer</param>
        void Serialize(BinaryWriter bw);
    }
}