
# D3DMesh Utilities

This tool is designed to be a modern, capable solution for converting from (and soon to) the Telltale d3dmesh format. Currently, only Poker Night at the Inventory Remastered is officially supported for use with this tool, though it will likely work with most if not all assets from games newer than and including TWDDS (2020). Support is planned for most games that use the newer D3DMesh format.

Currently, the application is capable of exporting bundled textures (though GLTF does not natively support detail maps), skeletons, and models from game assets' .d3dmesh files into .glb GLTF files, which most 3D applications support.

# Usage
**Important: If something doesn't work, please first try to give the tool access to more of the game's assets! The more it has access to, the better it will work.**

## GUI
The main way to use this app is through the GUI, by just launching the application. I will write more on this later, but it should be relatively self-explanatory.

First, either select a singular archive or the entire game directory and hit load load archive/game folder. If you're loading the game folder, you'll need to select the archive containing the mesh you want to extract, and then hit load archive. Next, select the mesh or meshes you want to load. To multiselect, either hold shift or control and click on entries. Then hit Load >>> to enter the directory to output converted meshes in.

Todo: better GUI usage

## CLI

The other way to use this is through the command line. Below is a list of arguments and what they do.

For each of these, values are set by an equals sign followed by a string, separated by spaces, but quotes can escape spaces. The string following should be a value matching the type.

### Arguments
| Flag               | Name                           | Description                                                                                                                                                          |
|--------------------|--------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| -gameDir           | Game Archives Directory        | This sets the path of the game directory, and loads it automatically, unless the autoLoad flag is set to false.                                                      |
| -game              | Game                           | This sets the game that the app will use settings for, like encryption key, MetaStream version, and Oodle compression.                                               |
| -archive           | Archive                        | This sets the archive from the game archives that will be loaded, unless the aa flag is set to false.                                                                |
| -m[odel]           | Model                          | This sets a model inside of the chosen archive to be automatically converted, unless the am flag is set to false.                                                    |
| -models [ms]       | Models                         | This sets a semicolon-separated list of models to be automatically converted, same above.                                                                            |
| -o[ut]             | Out Model Directory            | This sets the directory models will be automatically converted into, unless the ac flag is set to false.                                                             |
| -po (-profilerOut) | Profiler Output Mode/Directory | This sets the mode and directory of the profiling system, defaulting to off by not specifying, printing to stdout with "print", and a file with any valid file path. |
| -testImportMesh    | Test Import Mesh Directory     | This sets the file path of a small, WIP, testing gltf -> d3d importer. IT DOES NOT OUTPUT ANYTHING YET.                                                              |


### Flags
Simple booleans that help control CLI behavior when used with other flags

| Name                     | Description                                                                                                                                                                                                                   |
|--------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| -al (-autoLoad)          | Controls automatic loading of a specified game directory, used with  (If false, the game data directory field will be populated but the archives will not be loaded)                                                          |
| -aa (-autoChooseArchive) | Controls automatic archive selection from the list of game archives (If false, the archive will be selected by default but allow for the user to change the selection in the GUI before manually moving on to model choosing) |
| -am (-autoChooseModels)  | Controls automatic model selection from the list of models (If false, the models will be selected by default but allow for the user to change the selection in the GUI before manually moving on to conversion)               |
| -ac (-autoConvert)       | Controls automatic conversion of models. (If false, the app will wait for user input to start conversion)                                                                                                                     |
| -aq (-autoQuit)          | Controls automatic exiting of the application on finishing converting the models. (If true, the app will exit after finishing conversion)                                                                                     |
| -dpoc (-dumpProfilerOnConversion) | Controls wether to dump profiler output on conversion task finish or when the program closes. When the program closes can be unreliable. |

<sub><sup>Written with [StackEdit](https://stackedit.io/).</sup></sub>
