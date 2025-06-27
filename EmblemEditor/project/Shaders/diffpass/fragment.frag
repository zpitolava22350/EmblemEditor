#version 330 core
in vec2 vUV;
uniform sampler2D uFb;
uniform sampler2D uRef;

out float fragValue;

void main() {
    vec4 fb = texture(uFb, vUV);
    vec4 ref = texture(uRef, vUV);

    float diff = abs(ref.r - fb.r) + abs(ref.g - fb.g) + abs(ref.b - fb.b);

    fragValue = diff;
}