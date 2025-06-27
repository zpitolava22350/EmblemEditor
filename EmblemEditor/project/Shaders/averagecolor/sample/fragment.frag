#version 330 core
uniform sampler2D uMask;
uniform sampler2D uRef;
in vec2 vUV;
out vec4 fragColor;

void main() {
    vec4 ref = texture(uRef, vUV);
    float mask = texture(uMask, vUV).r;
    fragColor = ref * mask; // Keep only the shape-covered color
}
