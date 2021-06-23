cmake_minimum_required(VERSION 3.10)

project(FastObjForUnity VERSION 1.0)

add_library(fast_obj_unity SHARED src/fou.cpp)

target_include_directories(fast_obj_unity PUBLIC
    include
    fast_obj)

target_compile_definitions(fast_obj_unity PUBLIC FOU_API_BUILD_AS_DLL)
