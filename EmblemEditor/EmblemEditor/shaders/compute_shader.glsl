#version 450 core

layout (local_size_x = 16, local_size_y = 16) in; // Workgroup size: 16x16 threads

layout (binding = 0) uniform image2D referenceTexture; // Reference image to compare against
layout (binding = 1) uniform image2D renderTexture;   // Rendered image
layout (binding = 2) buffer DiffBuffer {             // Output buffer for differences
    float differences[];
};

void main() {
    ivec2 pixelCoords = ivec2(gl_GlobalInvocationID.xy); // Get pixel coordinates from global invocation ID

    // Read pixel colors from both textures
    vec4 referenceColor = imageLoad(referenceTexture, pixelCoords);
    vec4 renderedColor = imageLoad(renderTexture, pixelCoords);

    // Calculate the squared difference between the colors (RGB only, ignore alpha)
    float diff = distance(referenceColor.rgb, renderedColor.rgb);

    // Store the difference in the buffer
    differences[pixelCoords.y * imageSize(referenceTexture).x + pixelCoords.x] = diff;
}
