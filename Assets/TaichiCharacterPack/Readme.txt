
-------------------------------------------

GameAssetStudio
http://gameassetstudio.com/

-------------------------------------------

===========================================
 Ver 1.16 Update Information
===========================================
1. The viewer script has been changed from javascript to C#.

===========================================
How to use a Model
===========================================
1. Select a model you wish to use from project, then right-click on it.
2. Choose Export Package…
3. Export all of the assets while Include Dependencies.
4. Specify an arbitrary directory and a package name, then create a package file.
5. Open a project in which you wish to use the model.
6. Select Asset > Import Package > Custom Package...
7. Select the Package you chose the above step 4 and import it.
8. Once the model is imported to the project, it is ready for use.

===========================================
Taichi Character Pack Setting
===========================================
1. Animations with "_p" at the end of their file names have had their motion components removed and will remain at the origin.
2. Animations with "_a", "_b", or "_c" at the end of their file names divide the animation into 3 parts, respectively.
3. Some animations are intended for use with a pair of male or female characters.
This applies to the following animations:

- m01@hugshoulder_00　/　f05@hugshoulder_00
- m01@kiss_00　/　f05@kiss_00
- m01@hughip_00　/　f05@hughip_00
- m01@pathead_00　/　f05@pathead_00
- m01@hugshouldersit_00　/　f05@hugshouldersit_00

===========================================
Unity Asset Setting
===========================================
In order to use all of the character asset states in The Succubus Twins Character Pack,
please be sure to use the following settings.

Method 1
- 1. When dragging a model from the project folder to the scene, you must modify the number of bones to use per vertex for - skinning.
- 2. Bring the model into the scene and then double click on it.
- 3. In the Inspector Tab look for the "Quality" setting and change it from "Auto" to "4 Bones."
- 4. Now you are finished.
- 5. In addition, this procedure must be followed for each model you bring into the scene.

Method 2
- 1. Choose Edit > Project > Settings > Quality.
- 2. In the Inspector tab, choose Other > Blend Weights and set that to "4 Bones."
- 3. With this method there is no need to adjust model configurations each time they are brought into the scene.

Method 3
- 1. You can now set the Alpha cutoff of the toon_cutoff shader. 0.9 is the recommended setting.

Method 4
- If your intensity setting on the light is high, white clipping may occur.
- This can be fixed by reducing either the shininess setting on the Shader or the intensity on the light.

-------------------------------------------


This product also supports Mecanim.
To use Mecanim, please use the settings below.

- 1. When you select a Mecanim animation in the project folder, the animation's details are displayed in the Inspector tab.
- 2. Choose the Rig tab.
- 3. "Source" is displayed as "m01_schoolwear_000_hAvatar".
Load "Avatar" of the costume saved in Asset > TaichiCharacterPack > Resources > Taichi > Models Mecanim.
*The Mecanim animations in this product only correspond to the Taichi Character Pack.



===========================================
Changing the facial expression of M and L sized models
===========================================

1. Select Assets > TaichiCharacterPack > Resources > Taichi > Materials and then m01_face_00_m or m01_face_00_l
2. In the Inspector tab where material information is displayed, select Texture. 
3. By selecting "m01_face_xxx" in the texture overview, the facial expression can be changed.
* For facial expressions, please refer to the following correspondence table.

- Neutral
m01_face_00_m.png
m01_face_00_l.png

- Angry
m01_face_010_m.png
m01_face_010_l.png

- Sad
m01_face_020_m.png
m01_face_020_l.png

- Embarrassed
m01_face_030_m.png
m01_face_030_l.png

- Wink
m01_face_040_m.png
m01_face_040_l.png

- Kiss
m01_face_050_m.png
m01_face_050_l.png

- Eyes Closed
m01_face_060_m.png
m01_face_060_l.png

- Damage
m01_face_070_m.png
m01_face_070_l.png

===========================================
Unity Program Setting
===========================================
Asset Viewer ( WebPlayer )
Asset Twin Viewer ( WebPlayer )
Only able to use legacy animations

SceneFile:
TaichiCharacterPack > Viewer > taichi_viewer.unity
TaichiCharacterPack > Twin Viewer > twin_viewer.unity

Recommended Settings:
- PlayerSettings > Setting for Web Player > Resolution and Presentation
- Default Screen Width 960
- Default Screen Width 600

===========================================
About the TwinViewer
===========================================
TaichiCharacterPack > TwinViewer > twin_viewer.scene
With the TwinViewer, you can view all GameAssetStudio characters and animations that have been imported into the project.
You can change position, rotate, and play animations to check which characters and animations will work best with your game.

*The TwinViewer supports the latest version of each asset.
*To display characters other than Taichi in the TwinViewer, please update each character.

===========================================
How to use the TwinViewer
===========================================
Characters can be added from the Asset Store or previously downloaded packages.

How to Package import.

1. Choose the Root directory and right click.

 (Honoka Futaba Basic Set)
 HonokaFutabaBasicSet

 (Aoi Character Pack)
 Aoi Character Pack

 (Succubus Twins Character Pack)
 Succubus Twins Character Pack

 (Satomi Character Pack)
 SatomiCharacterPack

2. Choose Export Package…
3. Export all of the assets while Include Dependencies.
4. Specify an arbitrary directory and a package name, then create a package file. 
5. Open a project in which you wish to use the model. 
6. Select Asset > Import Package > Custom Package...
7. Select the Package you chose the above step 4 and import it. 
8. Once the model is imported to the project, it is ready for use.


===========================================
Description of directory
===========================================
[Taichi Character Pack]

  [Resources]
  : The animations and models that make up the main asset are stored here. Please use them as you see fit.
    [Taichi]
      [Animations Legacy]  : Legacy Animation Data
      [Animations Mecanim] : Mecanim Animation Data
      [Materials]          : Material Data
      [Models Legacy]      : Legacy Model Data
      [Models Mecanim]     : Mecanim Model Data
      [Textures]           : Texture Data
    [Shaders]              : Shader Data

  [Viewer]
  : The files in the hierarchy below this are used by Viewer, so please do not move or rename any of the files.

    taichi_viewer.scene     : Viewer Scene File
    [GUI]                   : Viewer GUI Images
    [Resources]             : Resources used by Viewer
      [Taichi]
        [Viewer Settings]   : Viewer Setting File
        [Viewer BackGround] : Viewer Background Images
    [Scripts]               : Viewer Scripts

  [Twin Viewer]
  : By importing Game Asset Studio Character Series, you can view 2 characters at once.
    so please do not move or rename any of the files.
    [GUI]
    [Resources] : Resources used by Twin Viewer
      [Taichi]
        [TwinViewer BackGrounds] : Viewer Background Images    
        [TwinViewer Settings]    : Twin Viewer Setting File              
          [F01]        : Honoka Futaba Basic Set Setting File
          [F02]        : Aoi Character Pack Setting File
          [F03]        : Succubus Twins Character Pack Setting File
            [Arum]     : Succubus Twins Character Pack Arum  Setting File
            [Asphodel] : Succubus Twins Character Pack Asphodel  Setting File
          [F05]        : Satomi Character Pack Setting File  
          [M01]        : Taichi Character Pack Setting File 
          [Puppet]     : Puppet Setting File 
    [Scripts]          : Twin Viewer Scripts