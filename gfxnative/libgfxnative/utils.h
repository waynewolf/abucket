#ifndef UTILS_H
#define UTILS_H

#include <GLES3/gl3.h>

class GLUtil {
public:
    static void printGLString(const char *name, GLenum s);
    static void checkGlError(const char* op);
    static const char* eglStatusStr();
    static GLuint loadShader(GLenum shaderType, const char* pSource);
    static GLuint createProgram(const char* pVertexSource, const char* pFragmentSource);
};

#endif //UTILS_H