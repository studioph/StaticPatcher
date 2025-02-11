# Static Items Synthesis Patcher

Configurable Synthesis patcher to allow making specific categories of items in specific locations static so that they no longer randomly go flying around for no reason.

> **Early-Release Status**. This patcher is functional, but still being actively developed and subject to breaking changes.
>
> _I am actively seeking feedback from early users, particularly on the UX and flexibiliy of configuring the patcher_

## Usage
Requires [Disable Havok Script Tweak Resource](https://www.nexusmods.com/skyrimspecialedition/mods/93426).

- Add to your Synthesis pipeline using the patcher browser
- If using multiple Synthesis groups, run this patcher in the same group as other patchers that modify placed objects to ensure changes are merged properly.

The patcher will log which records it updated. These can be viewed in the Synthesis log files or in the UI itself.

**Note**: The patcher can take a minute or two depending on how many mods you have and more importantly your configuration (see below) as this affects how many records need to be patched.

### Available Patcher Settings
 - **`Verbose logging`**: Enables debug logging which provides insight into the classification decisions made for locations and objects to aid in troubleshooting. This vastly increases the amount of log output and will slow down the patcher so don't enable this unless you are providing logs for a bug report.
 - **`Settings file name`**: The name of the configuration file consumed by the patcher. See the sections below for more information.


## How it works
For every `PlacedObject`(`REFR`) record, the patcher looks up the parent cell, the cell's `LCTN` field (if set), and the keywords of the linked location record. These are used to classify the location of the object into several predefined categories (see [Available Categories](#available-categories) below).

The placed reference itself is categorized based on its base object, using keywords, explicit lists of items, and name fuzzy-matching (does the object's name have "bowl" in it).

The patcher consumes a configuration file from the [user data folder](https://mutagen-modding.github.io/Synthesis/devs/User-Input/#user-data-folder) (separate from the patcher settings available in the Synthesis UI, see the [Config File](#config-file) section below) that contains a mapping of the aforementioned location categories and item categories to make static.

The patcher adds the `NoHavokSettle` flag and the `defaultDisableHavokOnLoad` script to targeted references, so the items can still be picked up and interacted with.


## Config File
The configuration of which locations and types of items you want to make static comes from a [TOML](https://toml.io) configuration file added to the Synthesis [user data folder](https://mutagen-modding.github.io/Synthesis/devs/User-Input/#user-data-folder). By default the file name is `StaticPatcherSettings.toml`, but this can be changed in the settings UI as mentioned above.

The reason for a seprate config file is the Synthesis settings UI is not well suited to complex mappings and other composite objects that are needed to make the patcher flexible. TOML was chosen as it is similar to the INI format that many users are familiar with, while offering many improvements over traditional INI files.

The config file contains key-value entries that map a location category to one or more item categories:

```toml
[location]
# Will make food and armor items inside stores static
store = ["food", "armor"]

# Make food, silverare (plates, cups, bowls, etc) and soul gems inside your homes static
playerhome = ["food", "silverware", "soulgem"]
```

The config file is case-insensitive.

The configuration file can be written with "dotted keys" syntax:
```toml
location.nordicruin = ["clutter"]
```

See the [TOML site](https://toml.io) for a full overview of the format.

## Available Categories
Below are the current available pre-defined categories for locations and items:


### Location categories
<details>
<summary>Show</summary>

- BanditCamp
- Barracks
- CastlePalace
- Cave
- City
- Dungeon
- DwemerRuin
- Farm
- Forsworn
- Fort
- GiantCamp
- Guild
- HallOfTheDead
- House
- Inn
- Jail
- Mill
- Mine
- NordicRuin
- PlayerHome
- Settlement
- Ship
- Store
- Stronghold
- Temple
- Town
</details>

### Item Categories
<details>
<summary>Show</summary>

- Alchemy
- Ammo
- Armor
- Bone
- Book
- BuildingMaterial
- Clothing
- Clutter
- Food
- Gem
- Gold
- Ingot
- Jewelry
- Key
- Ore
- Pelt
- Potion
- Scrap
- Scroll
- Silverware
- SoulGem
- Tool
- Weapon
</details>

## Caveats

The patcher relies heavily on keywords for classifying most records, especially for mod-added items which is the main value proposition of a dynamic patcher like this. Unfortunately, in my own testing the quality and accuracy of keywords in mods varies greatly, with even some well-known and popular mods adding incorrect keywords, or removing correct ones from records. Sadly there's not much I can do about that if a mod-added item or location uses incorrect keywords and gets mis-categorized.

However, there are different approaches that can be used for classification that could mitigate some of these issues, and I am seeking feedback from early-adopters that will help shape future development.

## Possible Future Plans
- Letting users define their own categories using the keyword/name matching or other criteria instead of pre-defined categories.
- More complex selector criteria such as AND/OR/negation of attributes
- Different patching strategies - i.e. creating new `STAT` records using the same meshes as the existing movable objects so they are truly static. This would make the items not interactable, but some users may prefer this.

## Reporting Bugs/Issues
Please include the following to help me help you:
- Synthesis log file(s)
    - Please turn on verbose logging as this provides a lot more information for me
- `Plugins.txt`
- Specific record(s) that are problematic
  - Screenshots/videos not required, but appreciated

**NOTE**: I will close issues from users not using the fixed Havok scripts from Andrealphus. There are too many issues with the vanilla scripts that will cause things to not work properly and I don't have the bandwidth to deal with false bug-reports that are a result of the vanilla script bugs.

## Credits
**AndrealphusVIII** for his fixes to the disable Havok script which allow the changes made by the patcher to actually behave properly in-game.