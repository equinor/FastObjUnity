#ifndef __FOU_H__
#define __FOU_H__

#if _WIN32
#ifdef FOU_API_BUILD_AS_DLL
#define FOU_API __declspec(dllexport)
#else
#define FOU_API __declspec(dllimport)
#endif
#else
#define FOU_API
#endif

extern "C"
{
    FOU_API void* read_obj(const char* filename);
    FOU_API void destroy_mesh(void* mesh);
}

#endif // __FOU_H__