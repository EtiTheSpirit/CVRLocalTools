# CVRLocalTools // "Local Parameter Extender"
Local Parameter Extender is responsible for adding the ability to declare a number of locally-managed animator parameters. This mirrors some VRChat features, such as `#IsLocal` to automatically toggle clientside avatar components when you spawn in.

To use this, simply add a parameter to your animator with a recognized name (the list is down below). The mod will find these parameters and manage their values in realtime. For FBT users, values begin updating after calibration is completed.

# Parameters

**There are certain rules to follow when naming parameters:**
* **PAY ATTENTION TO THE NAME.** Any parameter prefixed with `#` in the list below *will not work unless you make that parameter #local* (the only example of this is #IsLocal, which you don't want to send to other people anyway).
* Unlike in v1, parameters in v2 will *NOT* automatically be computed for other players. Instead, you should use CVR's native technique (prepend `#` to parameters you don't want to send) to manage this.
	* For old behavior, a new config option has been added to compute remote parameters belonging to other clients anyway.
	* This config option will still *NOT* manage parameters they have declared as local.


| Name               | Type  | Description                                                                                                       |
|--------------------|-------|-------------------------------------------------------------------------------------------------------------------|
| `#`IsLocal         | bool  | This is true on the game client of the person using the avatar, and false for everyone else.                      |
| VelocityX          | float | The X component of the avatar's velocity in world space.                                                          |
| VelocityY          | float | The Y component of the avatar's velocity in world space.                                                          |
| VelocityZ          | float | The Z component of the avatar's velocity in world space.                                                          |
| RelativeVelocityX  | float | The X component of the avatar's velocity in local space to the avatar.                                            |
| RelativeVelocityY  | float | The Y component of the avatar's velocity in local space to the avatar.                                            |
| RelativeVelocityZ  | float | The Z component of the avatar's velocity in local space to the avatar.                                            |
| RotationX          | float | The X component (pitch) of the avatar's rotation in world space. Will generally be zero.                          |
| RotationY          | float | The Y component (yaw) of the avatar's rotation in world space.                                                    |
| RotationZ          | float | The Z component (roll) of the avatar's rotation in world space. Will generally be zero.                           |
| RelativePitch      | float | The X component (pitch) of the avatar's rotation in local space to the avatar. Anchored by the yaw of the avatar. |
| RelativeRoll       | float | The Z component (roll) of the avatar's rotation in local space to the avatar. Anchored by the yaw of the avatar.  |
| Upright            | float | The dot product from the avatar's up vector to the world up vector. Will generally be one.                        |
| LookX              | float | The X component of the current viewport camera's forward vector.                                                  |
| LookY              | float | The Y component of the current viewport camera's forward vector.                                                  |
| LookZ              | float | The Z component of the current viewport camera's forward vector.                                                  |
| RotVelocityX       | float | The X component (pitch) of the avatar's rotational velocity in world space.                                       |
| RotVelocityY       | float | The Y component (yaw) of the avatar's rotational velocity in world space.                                         |
| RotVelocityZ       | float | The Z component (roll) of the avatar's rotational velocity in world space.                                        |
| PositionX          | float | The X component of the avatar's position in world space.                                                          |
| PositionY          | float | The Y component of the avatar's position in world space.                                                          |
| PositionZ          | float | The Z component of the avatar's position in world space.                                                          |
| `#`FingerTracking  | bool  | If true, the user's finger tracking is enabled. This is mostly for Index users but may apply elsewhere.           |