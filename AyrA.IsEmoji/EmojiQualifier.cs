namespace AyrA.IsEmoji
{
    /// <summary>
    /// Emoji qualifiers
    /// </summary>
    public enum EmojiQualifier : byte
    {
        /// <summary>
        /// A fully-qualified emoji (see ED-18 in UTS #51), excluding Emoji_Component
        /// </summary>
        FullyQualified = 1,
        /// <summary>
        /// An unqualified emoji (See ED-19 in UTS #51)
        /// </summary>
        Unqualified = 2,
        /// <summary>
        /// A minimally-qualified emoji (see ED-18a in UTS #51)
        /// </summary>
        MinimallyQualified = 3,
        /// <summary>
        /// An Emoji_Component, excluding Regional_Indicators, ASCII, and non-Emoji.
        /// </summary>
        Component = 4
    }
}