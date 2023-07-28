using System.Text;
using System.Text.RegularExpressions;

namespace AyrA.IsEmoji
{
    /// <summary>
    /// Provides checks about whether something is an emoji or not.
    /// Functions independent of the system emoji support
    /// by updating the emoji list from the unicode consortium
    /// </summary>
    public static class Emoji
    {
        /// <summary>
        /// Link from the unicode consortium that contains all emoji sequences in groups
        /// </summary>
        private const string EmojiList = "https://unicode.org/Public/emoji/latest/emoji-test.txt";
        /// <summary>
        /// Regex to parse emoji lines
        /// </summary>
        private const string RegexFilter = @"^([A-F\d\s]+);([^#]+)#\s+(\S+)\s+(\S+)\s+(.+)$";

        /// <summary>
        /// Date+Time of the last data update
        /// </summary>
        private static DateTime? _lastUpdate = null;
        /// <summary>
        /// Emoji list with the emoji as key
        /// </summary>
        private static readonly Dictionary<string, EmojiInfo> _emoji = new();
        /// <summary>
        /// Hierarchical emoji list
        /// </summary>
        private static readonly List<EmojiGroup> _emojiGroups = new();

        /// <summary>
        /// Gets if emoji data is present
        /// </summary>
        /// <remarks>
        /// Use <see cref="UpdateFromInternet()"/>
        /// or <see cref="LoadFromCache"/> to populate the emoji list after application start
        /// </remarks>
        public static bool HasData => _emojiGroups.Count > 0;

        /// <summary>
        /// Automatically initializes the emoji list and handles local caching.
        /// This will try to load the list from an existing cache,
        /// then update the cache from the internet if it's stale,
        /// then save the cache back to disk
        /// </summary>
        /// <param name="portable">True, to use portable mode, false otherwise</param>
        public static async Task AutoInit(bool portable)
        {
            await LoadFromCache();
            try
            {
                if (await UpdateFromInternet(TimeSpan.FromDays(30)))
                {
                    await SaveToCache(portable);
                }
            }
            catch (Exception ex)
            {
                if (!HasData)
                {
                    throw new IOException(
                        "Failed to obtain emoji data from the internet, " +
                        $"and no cached version exists on this machine yet. {ex.Message}", ex);
                }
                //Don't save list to cache if we cannot update it
            }
        }

        /// <summary>
        /// Get all loaded emoji
        /// </summary>
        /// <returns>Emoji</returns>
        public static string[] GetAllEmoji()
        {
            EnsureHasData();
            return _emoji.Keys.ToArray();
        }

        /// <summary>
        /// Gets all emoji groups
        /// </summary>
        /// <returns>Emoji groups</returns>
        public static EmojiGroup[] GetAllGroups()
        {
            EnsureHasData();
            return _emojiGroups.ToArray();
        }

        /// <summary>
        /// Gets emoji information
        /// </summary>
        /// <param name="emoji">Emoji</param>
        /// <returns>EMoji information. Null if not found</returns>
        public static EmojiInfo? GetEmoji(string emoji)
        {
            EnsureHasData();
            return _emoji.TryGetValue(emoji, out var value) ? value : null;
        }

        /// <summary>
        /// Checks if the given string consists of an emoji
        /// </summary>
        /// <param name="emoji">Emoji</param>
        /// <returns>true, if an emoji, false otherwise</returns>
        public static bool IsEmoji(string emoji) => GetEmoji(emoji) != null;

        /// <summary>
        /// Updates the list from the internet without considering the local cache
        /// </summary>
        public static Task<bool> UpdateFromInternet() => UpdateFromInternet(TimeSpan.Zero);

        /// <summary>
        /// Updates the list from the internet if it's missing or too old.
        /// Consider using <see cref="AutoInit(bool)"/> instead
        /// </summary>
        /// <param name="cacheAge">Maximum permitted cache age</param>
        /// <returns>true, if data was loaded, false if cached data could be reused.</returns>
        /// <remarks>
        /// Do not update too frequently. The unicode emoji list is not expanded too often.
        /// Checking about every 30 days will be sufficient
        /// </remarks>
        public static async Task<bool> UpdateFromInternet(TimeSpan cacheAge)
        {
            //Don't update if the cache is recent
            if (cacheAge != TimeSpan.Zero && _lastUpdate.HasValue && DateTime.UtcNow.Subtract(cacheAge) < _lastUpdate.Value)
            {
                return false;
            }

            EmojiGroup? group = null;
            EmojiSubgroup? subgroup = null;

            var groups = new List<EmojiGroup>();

            using var cli = new HttpClient();
            using var response = await cli.GetAsync(EmojiList);
            response.EnsureSuccessStatusCode();
            var lines = (await response.Content.ReadAsStringAsync())
                .Split('\n')
                .Select(x => x.Trim())
                .ToArray();
            foreach (var line in lines)
            {
                var gMatch = Regex.Match(line, @"^\s*#\s+((?:sub)?group):\s+(.+)");
                if (gMatch.Success)
                {
                    var name = gMatch.Groups[2].Value;
                    if (gMatch.Groups[1].Value == "group")
                    {
                        if (group != null)
                        {
                            groups.Add(group);
                        }
                        group = new EmojiGroup(name);
                    }
                    else
                    {
                        if (group == null)
                        {
                            //Skip subgroup declaration because no group has been created yet
                            continue;
                        }
                        if (subgroup != null)
                        {
                            group.Subgroups.Add(subgroup);
                        }
                        subgroup = new EmojiSubgroup(name);
                    }
                }
                else if (subgroup != null)
                {
                    var eMatch = Regex.Match(line, RegexFilter);
                    if (eMatch.Success)
                    {
                        var codePoints = eMatch.Groups[1].Value
                            .Trim()
                            .Split(' ')
                            .Select(m => int.Parse(m, System.Globalization.NumberStyles.HexNumber))
                            .ToArray();
                        var qualifier = ParseQualifier(eMatch.Groups[2].Value);
                        var emoji = eMatch.Groups[3].Value.Trim();
                        var standard = eMatch.Groups[4].Value.Trim();
                        var name = eMatch.Groups[5].Value.Trim();
                        var emojiInfo = new EmojiInfo(name, emoji, standard, qualifier, codePoints);
                        subgroup.Emoji.Add(emojiInfo);
                    }
                }
            }
            //Add last group we worked on
            if (group != null)
            {
                groups.Add(group);
                if (subgroup != null)
                {
                    group.Subgroups.Add(subgroup);
                }
            }

            //Update internal state after success
            _lastUpdate = DateTime.UtcNow;
            _emojiGroups.Clear();
            _emojiGroups.AddRange(groups);
            PopulateEmojiDict();
            return true;
        }

        /// <summary>
        /// Serializes all cached data into the given stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <remarks>
        /// Use <see cref="SaveToCache(bool)"/> instead
        /// to serialize to a file that's stored in the appropriate location
        /// </remarks>
        public static void Serialize(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var bw = new BinaryWriter(stream, Encoding.UTF8, true);
            bw.Write(_lastUpdate?.Ticks ?? long.MinValue);
            bw.Write(_emojiGroups.Count);
            foreach (var group in _emojiGroups)
            {
                group.Serialize(bw);
            }
            bw.Flush();
        }

        /// <summary>
        /// Deserializes emoji data previously stored with <see cref="Serialize(Stream)"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        public static void Deserialize(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var br = new BinaryReader(stream, Encoding.UTF8, true);
            var lu = br.ReadInt64();
            int groupCount = br.ReadInt32();

            var groups = Enumerable.Range(0, groupCount).Select(m => new EmojiGroup(br)).ToList();

            //Set internal fields only after complete read

            _lastUpdate = lu == long.MinValue ? null : new DateTime(lu, DateTimeKind.Utc);
            _emojiGroups.Clear();
            _emojiGroups.AddRange(groups);
            PopulateEmojiDict();
        }

        /// <summary>
        /// Saves all loaded data to a file based cache
        /// Consider using <see cref="AutoInit(bool)"/> instead
        /// </summary>
        /// <param name="portable">If true, the data is stored at <see cref="AppContext.BaseDirectory"/>.</param>
        public static Task SaveToCache(bool portable)
        {
            var p = GetEmojiFilePath(portable);
            var dir = Path.GetDirectoryName(p);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Task.Run(() =>
            {
                using var fs = File.Create(p);
                Serialize(fs);
            });
        }

        /// <summary>
        /// Loads data previously saved with <see cref="SaveToCache(bool)"/>
        /// Consider using <see cref="AutoInit(bool)"/> instead
        /// </summary>
        /// <returns>
        /// true, if data could be loaded, false otherwise
        /// </returns>
        /// <remarks>
        /// This automatically checks portable and local cache storage locations,
        /// preferring the portable location.
        /// </remarks>
        public static Task<bool> LoadFromCache()
        {
            var p = new string[]
            {
                GetEmojiFilePath(true),
                GetEmojiFilePath(false)
            };
            return Task.Run(() =>
            {
                foreach (var path in p)
                {
                    FileStream fs;
                    try
                    {
                        fs = File.OpenRead(path);
                    }
                    catch
                    {
                        continue;
                    }
                    using (fs)
                    {
                        Deserialize(fs);
                    }

                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Gets the portable or user local file path
        /// </summary>
        /// <param name="portable">Use portable path</param>
        /// <returns>File path</returns>
        private static string GetEmojiFilePath(bool portable)
        {
            if (portable)
            {
                return Path.Combine(AppContext.BaseDirectory, "emoji-cache.bin");
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmojiList", "emoji-cache.bin");
        }

        /// <summary>
        /// Populates <see cref="_emoji"/> from <see cref="_emojiGroups"/>
        /// </summary>
        private static void PopulateEmojiDict()
        {
            _emoji.Clear();
            foreach (var entry in _emojiGroups.SelectMany(m => m.Subgroups.SelectMany(m => m.Emoji)))
            {
                _emoji[entry.Emoji] = entry;
            }
        }

        /// <summary>
        /// Parses the qualifier in the way it appears in the unicode file into an enumeration
        /// </summary>
        /// <param name="qualifier">Qualifier string value</param>
        /// <returns>Parsed qualifier</returns>
        /// <remarks>This will throw if an unknown qualifier is encountered</remarks>
        private static EmojiQualifier ParseQualifier(string qualifier)
        {
            if (string.IsNullOrWhiteSpace(qualifier))
            {
                throw new ArgumentException($"'{nameof(qualifier)}' cannot be null or whitespace.", nameof(qualifier));
            }

            return Enum.Parse<EmojiQualifier>(qualifier.Trim().Replace("-", ""), true);
        }

        /// <summary>
        /// Ensure that data has been loaded and is not empty
        /// </summary>
        /// <exception cref="InvalidOperationException">Data not loaded or empty</exception>
        private static void EnsureHasData()
        {
            if (_lastUpdate == null)
            {
                throw new InvalidOperationException("No emoji data has been loaded yet");
            }
            if (!HasData)
            {
                throw new InvalidOperationException("The loaded emoji data is empty");
            }
        }
    }
}