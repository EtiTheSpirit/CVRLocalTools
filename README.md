# CVRLocalTools
This mod is a temporary solution to the lack of a predefined `#IsLocal` animator parameter, among other things.

# Parameters

**Note:** The leading `#` is **REQUIRED**. These parameters are not replicated.

When a parameter is said to apply to remote animators, that means your client computes the value for animators on other players (without actually sending it over the network).

| Name               | Type  | Description                                                                                            | Applies To Remote Animators |
|--------------------|-------|--------------------------------------------------------------------------------------------------------|-----------------------------|
| #IsLocal           | bool  | This is true on the game client of the person using the avatar, and false for everyone else.           | ✅                           |
| #VelocityX         | float | The X component of the avatar's velocity in world space.                                               | ✅                           |
| #VelocityY         | float | The Y component of the avatar's velocity in world space.                                               | ✅                           |
| #VelocityZ         | float | The Z component of the avatar's velocity in world space.                                               | ✅                           |
| #RelativeVelocityX | float | The X component of the avatar's velocity in local space to the avatar.                                 | ✅                           |
| #RelativeVelocityY | float | The Y component of the avatar's velocity in local space to the avatar.                                 | ✅                           |
| #RelativeVelocityZ | float | The Z component of the avatar's velocity in local space to the avatar.                                 | ✅                           |
| #RotationX         | float | The X component (pitch) of the avatar's rotation in world space. Will generally be zero.               | ✅                           |
| #RotationY         | float | The Y component (yaw) of the avatar's rotation in world space.                                         | ✅                           |
| #RotationZ         | float | The Z component (roll) of the avatar's rotation in world space. Will generally be zero.                | ✅                           |
| #RelativePitch     | float | The X component (pitch) of the avatar's rotation in local space to the avatar. Will generally be zero. | ✅                           |
| #RelativeRoll      | float | The Z component (roll) of the avatar's rotation in local space to the avatar. Will generally be zero.  | ✅                           |
| #Upright           | float | The dot product from the avatar's up vector to the world up vector. Will generally be one.             | ✅                           |
| #LookX             | float | The X component of the current viewport camera's forward vector.                                       | ❌                           |
| #LookY             | float | The Y component of the current viewport camera's forward vector.                                       | ❌                           |
| #LookZ             | float | The Z component of the current viewport camera's forward vector.                                       | ❌                           |
| #RotVelocityX      | float | The X component (pitch) of the avatar's rotational velocity in world space.                            | ✅                           |
| #RotVelocityY      | float | The Y component (yaw) of the avatar's rotational velocity in world space.                            | ✅                           |
| #RotVelocityZ      | float | The Z component (roll) of the avatar's rotational velocity in world space.                            | ✅                           |
| #PositionX         | float | The X component of the avatar's position in world space.                                               | ✅                           |
| #PositionY         | float | The Y component of the avatar's position in world space.                                               | ✅                           |
| #PositionZ         | float | The Z component of the avatar's position in world space.                                               | ✅                           |