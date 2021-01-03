# ELS Cinematic
Allowing the cinematic camera when using ELS...

**About:** 

Emergency Lighting System (ELS) currently disables the in game cinematic view due to keybinding issues. However, if you change the keybindings to avoid this - the in game view is still not available in ELS vehicles. 

This script will simply allow you to set your own keybinding to activate / deactivate the in game cinematic view at will for both ELS and non ELS vehicles. (If not using ELS however, you do not need this unless you would like to use cinematic view while in 1st person). 

**Note:** I strongly suggest you avoid setting it to a keybinding that you use for ELS (or for any other mod for that matter). I also suggest not setting the same as the in game keybinding for cinematic view (as it can be seen as a double press, turning the view on and off again).

List of potential keybindings can be found: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=netframework-4.8 or to disable a binding (e.g. the modifier key) set it to None

Controller buttons are disabled (None) by default but can be set to: DPadLeft, DPadRight, DPadUp, DPadDown, X, Y, A, B, LeftThumb, RightThumb, LeftShoulder, RightShoulder, Back or Start

**Current limitations:**

- Does not work in 1st person on motorcycles or bicycles (seems to detect 1st person as a cinematic view
- Cannot "press and hold" for temporary activation like "in game" control 
- If the keybinding is same as "in game" binding, it will turn it on and off in quick succession for non-ELS vehicles

**Installation:**

1. Copy the plugins folder into your "Grand Theft Auto V" install folder 
2. Edit the .ini to meet your requirements
**Note:** Avoid conflicting keys / buttons with ELS or the in game controls

**Version history:** 

1.0.2
- Remember if 1st person if exiting vehicle in cinematic view

1.0.1
- Enable cinematic view via controller
- Enable cinematic view while in 1st person

1.0.0
- Initial release to the public
