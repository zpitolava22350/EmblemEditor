#version 330 core
in vec2 vUV;
uniform sampler2D uFb;
uniform sampler2D uRef;

out float fragValue;

void main() {
    vec4 fb = texture(uFb, vUV);
    vec4 ref = texture(uRef, vUV);

    float diff = (ref.r - fb.r) + (ref.g - fb.g) + (ref.b - fb.b);

    fragValue = diff;
}