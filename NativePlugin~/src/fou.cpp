#include "fou.h"

#define FAST_OBJ_IMPLEMENTATION

#include "fast_obj.h"

void* read_obj(const char* filename) {
	fastObjMesh* mesh = fast_obj_read(filename);
	return mesh;
}

void destroy_mesh(void* mesh) {
	fast_obj_destroy((fastObjMesh*)mesh);
}
