#version 330 core
in vec2 fsTexCoord;

uniform sampler2D uSceneTex;      // binding = 0
uniform sampler2D uReferenceTex;  // binding = 1

out float outDiff;  // write to a single‐channel R32f

void main()
{
    vec3 sceneColor = texture(uSceneTex, fsTexCoord).rgb;
    vec3 refColor   = texture(uReferenceTex, fsTexCoord).rgb;

    // Compute per‐pixel difference; choose your metric.
    // Example: L1 difference (sum of absolute differences in R,G,B)
    vec3 diff3 = abs(sceneColor - refColor);
    float pixelError = diff3.r + diff3.g + diff3.b;

    outDiff = pixelError;
}
