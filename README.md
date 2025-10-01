# VRTRAKILL_REVAMP

I put this together in a week, there is a lot of hacky stuff in my changes I'd certainly not recommend putting all the changes in, but there is certainly some useful code, for example the new Plugin.cs and the transpilers. Also the build system isn't great, It's held together by hopes and dreams is what im saying. If you manage to get it to compile on your PC I will be impressed.

The hardest bit was getting it to actually run, the patcher wasn't working so I manually went into globalgamemanagers with uabea and added the enabled vr devices ("Oculus", "OpenVR", "None") to BuildSettings. All the dlls had to be gotten from the exact right version of unity and a couple had to be publicised. I also had to add the UnitySubsystems folder to ULTRAKILL_Data and entirelly rewrite the vr initilisation stuff in Plugin.cs.

**Known issues:**
- Haven't even touched the avatarRig, weapon wheel or 4-S
- Oil and blood (Except for 7-S) don't render due to the new rendering system
- Limbo skybox doesn't properly line up in VR
- HUD sometimes disapears
- No models of enemies in shop
- Sooooo many other bugs, I haven't played through the whole thing, it doesn't have nearly as much polish as the previous version

**Installation:**
Drag the ULTRAKILL_COPYTOROOT folder to the root of your game.

**Building**
Build process is pretty messy, I just got everything into the folders manually and got vs to copy over the dlls when I changed them. I used vscode 2022 so just open the .sln file in the root.

I'd love to work on this mod more in the futhre when I have more time, also yes I know this will all be out of date when Fraud comes out, but I really wanted to play ULTRA_REVAMP in vr sooo....