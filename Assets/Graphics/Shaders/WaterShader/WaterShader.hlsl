#ifndef WATERSHADER_INCLUDED
#define WATERSHADER_INCLUDED

static const float maxFloat = 3.402823466e+38;


// Big thanks to Sebastian Lague and StackOverflow for this code
/** Returns distanceToSphere, distanceThroughSphere
* If inside sphere, distanceToSphere = 0
* If ray misses sphere, distanceToSphere = max float value, distanceThroughSphere = 0
* Given rayDirection must be normalized
*/
void RaySphere_float(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDirection, out float distanceToSphere, out float distanceThroughSphere) {
    float3 offset = rayOrigin - sphereCenter;
    const float a = 1; // set to dot(rayDirection rayDirection) if rayDirection may not be normalized
    float b = 2 * dot(offset, rayDirection);
    float c = dot(offset, offset) - sphereRadius * sphereRadius;
    bool intersected = false;

    float discriminant = b*b - 4*a*c;
    // No intersections : discriminant < 0
    // 1  intersection  : discriminant = 0
    // 2  intersections : discriminant > 0
    if (discriminant > 0) {
        float s = sqrt(discriminant);
        float distanceToSphereNear = max(0, (-b - s) / (2 * a));
        float distanceToSphereFar  = (-b + s) / (2 * a);

        if (distanceToSphereFar >= 0) {
            distanceToSphere = distanceToSphereNear;
            distanceThroughSphere = distanceToSphereFar - distanceToSphereNear;
            intersected = true;
        }
    }

    if (!intersected) {
        // Ray did not intersect sphere
        distanceToSphere = maxFloat;
        distanceThroughSphere = 0;
    }
}

void isDepthGreaterZero_float(float passVal, float4 originalCol, float comparaisonVal, out float4 outputColor) {
    /* if (comparaisonVal > passVal)
        outputColor = float4(1, 1, 1, 1);
    else
        outputColor = originalCol; */
    // blanc : ne traverse pas la sphere
    if (comparaisonVal < 3.402823466e+38)
        outputColor = originalCol;
    else
        outputColor = float4(1, 1, 1, 1);
}

void passedThroughSphere_float(float val, float4 yes, float4 no, out float4 col) {
    if (val > 0) {
        col = yes;
    }
    else {
        col = no;
    }
}

#endif