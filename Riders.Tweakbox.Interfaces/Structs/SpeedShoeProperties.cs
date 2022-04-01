﻿using System.Runtime.InteropServices;
namespace Riders.Tweakbox.Interfaces.Structs;

// TODO: Remove this when removing legacy physics config support
[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public struct SpeedShoeProperties
{
    // Reminder: Update SpeedShoePropertiesSerializer in Riders.Tweakbox if updating.

    /// <summary>
    /// Mode describing how speed shoes operate.
    /// </summary>
    [FieldOffset(0)]
    public SpeedShoesMode Mode;

    /// <summary>
    /// The amount of speed set when in fixed mode.
    /// </summary>
    [FieldOffset(4)]
    public float FixedSpeed;

    /// <summary>
    /// The amount of speed set when in additive mode.
    /// </summary>
    [FieldOffset(8)]
    public float AdditiveSpeed;

    /// <summary>
    /// The decimal by which to increase player's current speed.
    /// 1.2 indicates a speed increase of 20%.
    /// </summary>
    [FieldOffset(12)]
    public float MultiplicativeSpeed;

    /// <summary>
    /// Minimum speed to allow in multiplicative mode.
    /// </summary>
    [FieldOffset(16)]
    public float MultiplicativeMinSpeed;

    public static SpeedShoeProperties Default()
    {
        return new SpeedShoeProperties()
        {
            Mode = SpeedShoesMode.Vanilla,
            FixedSpeed = 1.075000f,
            AdditiveSpeed = 0.10f,
            MultiplicativeSpeed = 0.20f,
            MultiplicativeMinSpeed = 0.95f,
        };
    }
}

/// <summary>
/// Mode for custom speed shoe behaviour.
/// </summary>
public enum SpeedShoesMode
{
    /// <summary>
    /// Fixed speed, vanilla values.
    /// </summary>
    Vanilla,

    /// <summary>
    /// Fixed speed.
    /// </summary>
    Fixed,

    /// <summary>
    /// Add a fixed amount of speed to
    /// </summary>
    Additive,

    /// <summary>
    /// Multiplies the player's current speed.
    /// </summary>
    Multiplicative,

    /// <summary>
    /// Multiplies the player's current speed or sets to a fixed speed.
    /// Whichever of the two will yield greater speed.
    /// </summary>
    MultiplyOrFixed,
}
