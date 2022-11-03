# CVRLocalTools
This mod is a temporary solution to the lack of a predefined `#IsLocal` animator parameter, among other things.

# Parameters

**Note:** The leading `#` is **REQUIRED**. These parameters are not replicated.

When a parameter is said to apply to remote animators, that means your client computes the value for animators on other players (without actually sending it over the network).

| Name          | Type  | Description                                                                                  | Applies To Remote Animators |
|---------------|-------|----------------------------------------------------------------------------------------------|-----------------------------|
| #IsLocal      | bool  | This is true on the game client of the person using the avatar, and false for everyone else. | Yes                         |
| #VelocityX    | float | The X component of the avatar's velocity in world space.                                     | Yes                         |
| #VelocityY    | float | The Y component of the avatar's velocity in world space.                                     | Yes                         |
| #VelocityZ    | float | The Z component of the avatar's velocity in world space.                                     | Yes                         |
| #RotationX    | float | The X component (pitch) of the avatar's rotation in world space. Will generally be zero.     | Yes                         |
| #RotationY    | float | The Y component (yaw) of the avatar's rotation in world space.                               | Yes                         |
| #RotationZ    | float | The Z component (roll) of the avatar's rotation in world space. Will generally be zero.      | Yes                         |
| #RotVelocityX | float | The X component (pitch) of the avatar's rotational velocity in world space.                  | Yes                         |
| #RotVelocityY | float | The Y component (pitch) of the avatar's rotational velocity in world space.                  | Yes                         |
| #RotVelocityZ | float | The Z component (pitch) of the avatar's rotational velocity in world space.                  | Yes                         |
| #PositionX    | float | The X component of the avatar's position in world space.                                     | Yes                         |
| #PositionY    | float | The Y component of the avatar's position in world space.                                     | Yes                         |
| #PositionZ    | float | The Z component of the avatar's position in world space.                                     | Yes                         |