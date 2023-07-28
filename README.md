# AyrA.IsEmoji

This is a library that checks if an input string is an emoji or not.
It uses official tables by the unicode consortium, and can update automatically.

## Executable

*To use this project in your own application, use the dll directly instead of the exe*
*See further below for instructions*

The release contains a `CreateJson.exe` executable.
This executable will dump emoji data into two json files for further processing by other tools.

- `emoji-group.json` Contains all emoji sorted into groups and subgroups
- `emoji-list.json` Contains all emoji as plain string array

You want to use the list for simply checking if something is an emoji,
and the grouped output to get more information like the group an emoji is in,
or the name of the emoji.

### Group JSON Structure

The group json produced by the executable contains an array of groups.

#### Groups

Groups have a "Name" (string) and "Subgroups" (Subgroup) property.

#### Subgroup

Subgroups have a "Name" (string) and "Emoji" (EmojiInfo) property.

#### EmojiInfo

Each EmojiInfo has:

- Name `string`: Name of the emoji
- Emoji `string`: String that represents the emoji
- Codepoints `int[]`: Unicode codepoints that represent the emoji
- Specification `string`: Unicode specification that describes the emoji
- Qualifier `EmojiQualifier`: How the emoji is qualified

#### EmojiQualifier

This is an enumeration (as integer):

| Name               | Int | Description                                                         |
|--------------------|-----|---------------------------------------------------------------------|
| FullyQualified     |   1 | a fully-qualified emoji (see ED-18 in UTS #51), excluding Component |
| Unqualified        |   2 | a unqualified emoji (See ED-19 in UTS #51)                          |
| MinimallyQualified |   3 | a minimally-qualified emoji (see ED-18a in UTS #51)                 |
| Component          |   4 | a component, excluding regional indicators, ASCII, and non-emoji    |

Some emoji exist multiple times because they can be constructed in different ways.
These identical emoji will have a different qualifier.

## DLL

The `AyrA.IsEmoji.dll` file contains everything needed to check emoji in your .NET projects.
It requires .NET 6 and may run in newer versions.
This behavior is untested, but nothing deprecated has been used.

### Initializing the Library

Simply reference the DLL in your project,
then call `AyrA.IsEmoji.Emoji.AutoInit(bool);` to fully automatically initialize the cache.

The behavior of this is as follows:

1. Load emoji data from cache if it exists
2. Update the cache if does not exist, or is older than 30 days
3. Save new data to cache if it was just updated or created

You can also perform these steps manually using the
`LoadFromCache`, `UpdateFromInternet` and `SaveToCache` functions.
The cache uses files by default. If you want to use your own impmentation,
you can call the `Serialize` and `Deserialize` functions instead which take streams as argument.

### Usage

To use the library after you've initialized it,
call the `.IsEmoji(string)` function to check if a string is an emoji.
You can also use `.GetEmoji(string)` to get information about an emoji.

### Lists

Call `GetAllEmoji()` or `GetAllGroups()` to get all emoji or groups respectively.
