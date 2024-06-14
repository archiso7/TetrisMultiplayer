#version 330 core

layout(location = 0) in vec2 aPosition;

uniform vec2 uWindowSize;  // Size of the window in pixels
uniform vec2 uPosition;    // Position offset
uniform vec2 uScale;       // Scaling factors
uniform vec2 uPositionOffset; // Position offset

void main()
{
    vec2 scaledPosition = ((aPosition + uPosition) / uWindowSize * uScale) + (uPositionOffset / uWindowSize) * 2.0;
    gl_Position = vec4(scaledPosition, 0.0, 1.0);
}
