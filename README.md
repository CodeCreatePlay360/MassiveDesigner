## MassiveDesigner

MassiveDesigner is a level design tool for unity.

## Installation
Download this repository or install from one of the packages. There is a pro version of this tool with some more features available through patreon [link]().

## Creating a MassiveDesigner instance

To create a new **MassiveDesigner** instance, create an empty game object and add the **MassiveDesigner** component.

The main window has 3 subsections
1. **World** is where the level design tools are located.
2. **LocationEditor** can be used to create and define locations in game world.
3. **Settings** contains global settings common to all tools. 

![Image](Images/00.png)

****
# World
This section mostly contains tools for designing the game world such as vegetating painting, creating roads, rivers, path ways etc.

## Settings up a SpawnTiles instance

Before any operations in **World** section you need to have a working **SpawnTiles** setup, objects will be spawned and stored in these tiles, to initialize **SpawnTtiles** **Settings > InitializeSpawnTiles.**  
Turn on **DebugTiles** under **DebugOptions** to see spawned tiles.

![Image](Images/01.png)

## Layer

![Image](images/02.png)

To group together similar objects **MassiveDesigner** uses concept of **Layers**, **Layers** should contain only one type of closely related objects for example trees, grasses and rocks should be placed in separate layers. The type of objects stored in a layer can be set in layer properties.  

To disable a **Layer** uncheck the toggle button next to a **Layer**.  
You can also copy a **Layer's settings** and paste it on another Layer (using the copy/paste buttons next to Layer).  

>  _In MassiveDesigner almost all GUI elements have an associated tooltip detailing its usage, to see a tooltip hover mouse cursor over any button or GUI element._

| Setting | Description |
| --- | --- |
| ItemsType | Type of item stored in this layer |
| Priority | Layer's priority |
| LayerMask | Layer you want to paint on |
| SplatLayer | The index of unity terrain splat layer to paint on, paint meshes will only be spawned on this terrain layer, set this to -1 to turn this feature off, to spawn at more than one layer put a comma in between different indexes for example (1, 3, 4) |
| UseTerrainTextureStrength | If true, less objects will be spawned in areas with lower terrain texture strength/opacity |

## PaintMesh

![Image](images/03.png)

A **PaintMesh** is the spawn object, any object you want to spawn via any tool should have this component.  
To create a new **PaintMesh**, add the **PaintMesh** component to any GameObject you want to spawn.  
**PaintMeshes** can be spawned only after they have been added to **MassiveDesigner Layer**, to add a **PaintMesh** to a **MassiveDesigner Layer**, drag them to the **LayerPrototypes** section of a **MassiveDesigner Layer**.  
**MassiveDesigner** uses its own collision system, a **PaintMesh** can contain 2 colliders at most, trees are defined using 2 colliders, one for tree trunk and one for top branches, all other object use first collider.  
All collision related settings are defined under **PaintMesh > CollisionSettings**.
* First collider of any **PaintMesh** is origin at pivot of GameObject, it is defined using **FirstColliderRadius**.
* The second collider is defined using **SecondColliderRadius** and its position can be offset using **SecondColliderOffset**.

**Setting**

| Setting | Description |
| --- | --- |
| SpecieName | Name of this specie, items of similar species for example different variations of **Birch trees** should have same specie name, this  |
| **SpecieSettings** | |
| SpawnProbability | Increases the chance of selecting this paint mesh compared to other **PaintMeshes** in this layer |
| SurvivalRate | Increases chances of survival ( only valid for _GlobalFoliageSimulation_ ) |
| DispersionStrength | Probability of spawn of items of same specie close to each other |
| **CollisionSettings** | |
| FirstColliderRadius | Radius of first collider, origin at pivot point |
| SecondColliderRadius | Radius of second collider, origin at SecondColliderOffset (valid only if layer items types is trees) |
| SecondColliderOffset | Offset of second collider from origin |
| **Variation** | |
| ScaleMultiplier | Scales the size of PaintMeshes, use this if you want to globally increase or decrease size of this PaintMesh during any spawn operation |
| RotationVariation | Rotation variation along x-z plane |
| **Debug** | |
| Debug | Draw a visual representation of colliders for debugging. |

 ## Debugging a PaintMesh
  At most you would want to visualize colliders of a **PaintMesh** for debugging purpose, to debug import any GameObject with **PaintMesh** component into scene and toggle on Debug under **PaintMesh** debug settings.



## FoliagePainter

![Image](images/04.png)

FoliagePainter is manual vegetation painting system of **MassiveDesigner**, it is fast and more useful at painting large objects like trees, however it can still be used to scatter small objects like grasses at a very fast rate if brush size is small.  
To begin painting a valid unity terrain should exist in scene with a **SpawnTiles** instance covering entire terrain, after everything is setup  
* control + right click drag to start painting
* shift + right click drag to remove paint

**Setting**

| Setting | Description |
| --- | --- |
| **PaintSettings** | |
| PaintMode |**Normal:** Object is not spawned if a collision is detected |
| | **OverPaint:** Replaces objects with lower priorities with higher priority objects, if a collision occurs between them during spawn (PaintMesh priority is same as a MassiveDesigner's Layer priority). |
| PaintRadius | (Green circle) Spawning will take place in this radius |
| Use All Layers | If this checked, PaintBrush will select PaintMeshes from all Layers for spawning, otherwise PaintMeshes only from selected Layer will be spawned |
| WeightedSelection | If this is checked, More spawn chance will be given to PaintMeshes with higher spawn priorities. |
| **EraseSettings** | |
| RemoveRadius | (Red circle) Spawned objects will be removed in this radius |
| RemoveStrength | Probability of removing paint meshes |
| RemoveOnlyOnSelectedLayer | Set this to true to only remove PaintMeshes from selected layer |
| **BrushSettings** | |
| SpawnDelay | Delay before next spawn operation, for most cases this should be set to **0.1**, settings this to a lower number will effect performance a lot. |
| Opacity | Scales number of spawns per each spawn operations... by default number of spawns for each spawn op is same as PaintRadius |

# Location_Editor
**_Documentation for this section is incomplete_**

****
## Known issues


## Roadmap
**For version 1.0**
1. Right now there is no dedicated renderer for MassiveDesigner, foliage is spawned as Unity terrain tree object or a terrain detail object, so a foliage renderer is top most priority for v1.0.
2. Add builtin foliage shaders.

## Support
* [Discord server](https://discord.gg/ZbYRKtN6pg)