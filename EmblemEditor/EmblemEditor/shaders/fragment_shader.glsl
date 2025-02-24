#version 430 core

in vec2 texCoords;
out vec4 fragColor;

uniform sampler2D framebufferTexture; // The texture to compare
uniform sampler2D referenceTexture;   // The reference texture

void main() {
    vec4 framebufferColor = texture(framebufferTexture, texCoords);
    vec4 referenceColor = texture(referenceTexture, texCoords);
    //fragColor = abs(framebufferColor - referenceColor); // Color difference
    //float diff = abs(framebufferColor - referenceColor).r + abs(framebufferColor - referenceColor).g + abs(framebufferColor - referenceColor).b + abs(framebufferColor - referenceColor).a;
    //diff /= 4;
    //fragColor = vec4(diff, diff, diff, 1);
    fragColor = referenceColor;
}
