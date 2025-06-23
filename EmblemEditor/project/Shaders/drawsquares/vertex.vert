#version 330 core
layout(location = 0) in vec2 inPosition;   // e.g. a full‐screen quad: (-1,−1)→(+1,+1)
layout(location = 1) in vec2 inTexCoord;

out vec2 fsTexCoord;

void main()
{
    gl_Position = vec4(inPosition, 0.0, 1.0);
    fsTexCoord = inTexCoord;
}
