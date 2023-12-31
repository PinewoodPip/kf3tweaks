
This is a mod for Kemono Friends 3 (DMM version) that improves the graphics quality, adds some keyboard controls, and alters some text fields to display better when using the translation mod.

It functions similarly to the translation mod, in the sense that no game files are modified. As such, it should be as safe/unsafe to use as the translation mod itself, and is compatible with it.

## Installation
Requires BepInEx, which you'll already have if you're using the translation patch.

Extract contents into the BepInEx folder, such that the contents of `plugins` goes into your `plugins` directory and likewise for the `patchers` and `core` folder.

- `kf3tweaks.dll` is the main plugin, with the features described below that aren't related to text
- `TextFitting.dll` is the plugin that edits text fields; this is also distributed separately with Vorked's translation pack
- The MonoMod files and `BepInEx.MonoMod.HookGenPatcher.dll` are required to generate hooks for the game's methods; `LighterPatcher.dll` strips the auto-generated MMHOOK dll to reduce bloat
    - This should allow the mod to keep working after game updates, so long as the patched methods are not significantly altered

## Features
### Graphical improvements
- Unlocked resolution and aspect ratio
    - Not all UIs will be usable with unusual aspect ratios
- Makes the game use up to 8x MSAA instead of 2x/1x, which greatly increases the quality of the anti-aliasing and makes far-away friends look a lot less jagged
- Doubled the size of textures used with camera render-to-texture, which makes friends look a lot less blurry in UIs where they are superposed over a background (ex. friend growth, friend details, Meerkat/Dhole/Peach Panther/Mirai in the UIs)
- Press `Right Alt + Enter` to toggle borderless windowed mode
- Press `Right Ctrl` to toggle VSync, which will also set the FPS cap of the game to your monitor's

![Comparison of friend details between unmodded 1600x900 and 1080p with the plugin.](images/friend_details_comparison.png)
<i>Comparison of friend details between unmodded 1600x900 and 1080p with the plugin.</i>

---

![Dialogue at 1080p with 8x MSAA.](images/dialogue.png)
<i>Dialogue at 1080p with 8x MSAA.</i>

### Text-fitting
Some text fields have been modified to try to prevent text overflow when using the translation mod. This is done by the `TextFitting.dll` plugin.

The following text fields are affected:

- Quest dialogue, as well as the log and text within choice buttons
- Friend text in quest selection
- Friendship level up messages
- Daily login bonus friend text
- Gacha newcomer greeting
- Skill descriptions (in the friend details UI)
- Buttons in the shop

Text in these fields will wrap, and font size will be reduced if the text were to overflow.

![Dialogue text fitting.](images/dialogue_text_fit.png)
![Quest flair text fitting.](images/chapterselect_text_fit.png)

Additionally, usernames within the PVP enemy selection or the helper selection UIs will (try to) no longer be translated. Unfortunately due to quirks with the translator plug-in, they might still get translated if you back out of a menu (ex. select a helper and go back) or toggle the translation off & on.

### Keyboard controls
Some keyboard controls were added to combat:

- Number keys 1-5 will issue orders; these correspond to friends from left to right
    - If `Left Shift` is also held, miracles will be issued instead
- `Escape` will cancel an order
- `Spacebar` uses a refill
- `F` toggles fast mode
- `A` toggles autoplay

These hotkeys should function the same way as touching the corresponding UI elements.

### Other stuff

- Camera rotation when petting friends is no longer restricted
    - Due to how the camera movement works, in some places it might clip out of bounds. Zooming in/out will not help as it only affects FoV.
    - You cannot do 360 rotations around a friend; once you reach the maximum +/-180 degrees on either axis, you'll have to rotate in the opposite direction to get to the other side

![Ostrich-san waiting for her back massage.](images/home_friend.png)
<i>Ostrich-san waiting for her back massage.</i>

## Known Issues
- `Right Ctrl` key to toggle VSync appears to not work in some places; use it in the home or combat scenes if you run into this issue (and it will persist)
- Clothing might shake a bit in the picnic scene above 30FPS; this is probably too much of a hassle to fix
- There is no longer a prompt before closing the game, as it is tightly coupled to the resolution lock. Personally I prefer it that way, but if there is demand, I could add it back.

## Planned features
- Rebinding hotkeys
- BepInEx config file support
- Option to use 4x MSAA instead (though I assure you your GPU is not the bottleneck in this game)