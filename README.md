# Fast OBJ importer for Unity

This package uses [fast_obj]([https://](https://github.com/thisistherk/fast_obj)) implementation for reading OBJ files.

## Benchmarks

Intel Xeon Gold 6136 3.00GHz,
128 GB RAM,
Samsung SSD 970 EVO Plus 2TB

OBJ size | Groups | Triangles | Vertices | Unity 2020.3.12f1 native | FastObjUnity (raw) | FastObjUnity (Optimize)
-|-|-|-|-|-|-
182 MB | 1 | 1 533 557 | 1 561 241 | 00:32:25 | 00:00:16 | 00:06:54

## How to build native library

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

## Limitations and known issues

- Only ASCII file paths are supported
- Only ASCII file encoding is supported
- Only triangulated meshes are supported
- Skips materials and textures
