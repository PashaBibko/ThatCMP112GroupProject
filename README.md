# Cyberkart Racer

Public repository is available [here](https://github.com/PashaBibko/ThatCMP112GroupProject). No Generative AI was used for this project.

### How to play

- W - Accelerate
- D - Reverse
- A - Turn Left
- D - Turn Right

### Programming Problems

##### Pasha

The main problem that I encountered on this project was getting the tracks to create nice curves without
forming gaps or too sharp turns. For most of the track sections I was able to do this by calculating the
intersection of the two lines:

1. Position as the start of the current section, with its direction being the direction of the end of the last section
2. The bisector of the start and end of the current spline section

And as long as there was an intersection in front of the start position of the section and within a short
distance (15 to 50). I could use this point as the control for a Bézier curve, and it created a 
smooth line that blended nicely with the previous track sections. I was also able to use a similar premise
for the final spline section so it could create a nice loop, but I also had to take into consideration the
final points direction. So I had to find a point that intersects with the two lines:

1. Start of the section, with its direction being the direction of the end of the last section
2. End of the section, with its direction being the starting direction of the whole track loop

However, as this point quite often cannot exist, it instead is checked for a lot during the second half of
the generation to see if it can exist. Even still it often cannot so the entire generation process has to
be repeated all over again. Often this process takes around 100-200 full track generations, which might be
a performance concern if the project were to continue development. But with the current (relatively) simple
generation it takes less than a second to generate a track so the issue is negligible.

##### Jamie

The most difficult part of the prototype was creating the kart. Initially I tried using Unity’s wheel colliders
but after some struggling, I found that the common consensus online was using ray casts as it was easier and more
suited for the style of kart racing style of game we were going for compared to wheel colliders that are more often
used for realistic simulators. Multiple videos online showcased this effect while walking the viewer through how it
was achieved without showing the scripts. This meant that there was a lot of trial and error where some aspects such
as the spring force and damping were relatively easy to implement compared to others such as the tire grip which I
found especially difficult. For the tire grip the solution I found was to get the local velocity of the tires and
dampen it on the right leading to the kart not sliding compared to before which makes the game feel much better
to control in comparison to before.

### External

##### Skybox
The skybox asset was downloaded from [here](https://assetstore.unity.com/packages/2d/textures-materials/sky/free-skyboxes-sci-fi-fantasy-184932) on the Unity Asset Store. Asset files are located at `Assets/FREE Skyboxes - Sci-Fi & Fantasy`.

##### Track Material
The shader used for the track material was taken from one of Pasha's previous projects. The shader source code is located at `Assets/RaceGeneratorScirpts/TrackSplineRender.shader`.
