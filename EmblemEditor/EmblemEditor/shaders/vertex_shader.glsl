#version 430 core

layout(location = 0) in vec3 aPosition;  // Vertex position
layout(location = 1) in vec2 aTexCoord;  // Texture coordinates

out vec2 texCoords;

void main() {
    gl_Position = vec4(aPosition, 1.0);
    texCoords = aTexCoord;
}
