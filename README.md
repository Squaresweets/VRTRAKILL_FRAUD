# VRTRAKILL_FRAUD

YOUTUBE VID HERE

## IMPORTANT:
It is neccessary to downgrade Ultrakill to the first fraud hotfix for this to work. You can install that using:

`download_depot 1229490 1229491 5628746843149106870`

In the steam console. A good tutorial can be found [here](https://www.youtube.com/watch?v=vfXyy3KWqAI). You'll probably want to bring your saves across as well.

## Intro:
This is a fork of the VRTRAKILL mod with support for Fraud. If you are looking for ULTRA_REVAMP support, there is a release for that.
Annoyingly just before I released this beta a hotfix was released that breaks stuff and I don't have time currently to fix it, sorry!
When recording videos, I'd recommend you record the Steam VR view instead of the game itself.

## Known issues:
- Portals seen in mirrors don't render properly
- Minor flickering when going through portals
- Whiplash is buggy through portals
- Desktop camera would require a third portal renderer
- Haven't even touched the avatarRig, weapon wheel or 4-S
- Oil and blood (Except for 7-S) don't render due to the new rendering system
- HUD sometimes disapears
- No models of enemies in shop
- Sooooo many other bugs, I haven't played through the whole thing, it doesn't have nearly as much polish as the previous version

## Installation:
Drag the contents of the ULTRAKILL_COPYTOROOT folder to the root of your game. You should get a message asking if you want to replace the files in the destination, say yes. You will obviously need BepInEx if you haven't already got it.

## Building:
Build process is pretty messy, I just got everything into the folders manually and got vs to copy over the dlls when I changed them. I used vscode 2022 so just open the .sln file in the root.
