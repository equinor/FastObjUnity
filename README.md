# Fast OBJ importer for Unity

This package uses fast_obj implementation for reading OBJ files.

## How to build

### Requirements

- CMake 3.10 or later
- Visual Studio 2019 or later

```cmd
git clone https://github.com/equinor/FastObjUnity
cd FastObjUnity
git submodule update --init
cd NativePlugin~
mkdir build
cd build
cmake ..
```

Open generated solution file.

Build.

## How to use

Unity does not allow to override native importers so we have to cheat. There are two possible workarounds:

- Rename your OBJ files to *.obj_fast before import
- Add definition flag **FASTOBJ_AUTORENAME** in Project Settings -> Script Compilation -> Scripting Define Symbols of your Unity project
