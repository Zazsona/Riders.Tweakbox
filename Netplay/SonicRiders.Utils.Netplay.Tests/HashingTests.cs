﻿using System;
using DotNext.Buffers;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Sewer56.BitStream;
using Sewer56.SonicRiders.Structures.Gameplay;
using Xunit;
using Player = Sewer56.SonicRiders.API.Player;
namespace Riders.Netplay.Messages.Tests;

public class HashingTests
{
    [Fact]
    public void HashIsDeterministic()
    {
        var gameData = Random();

        var bytes = gameData.ToCompressedBytes(out int bytesWritten).Span.Slice(0, bytesWritten).ToArray();
        var gameDataHash = AntiCheatDataHash.FromData(bytes);
        var gameDataHash2 = AntiCheatDataHash.FromData(bytes);
        Assert.Equal(gameDataHash, gameDataHash2);
    }

    [Fact]
    public void SerializeDeserialize()
    {
        var gameData = Random();
        using var rental = new ArrayRental<byte>(32768);
        var bitStream = new BitStream<RentalByteStream>(new RentalByteStream(rental));

        // Write to Stream
        gameData.ToStream(ref bitStream);

        // Read from stream.
        bitStream.BitIndex = 0;
        var gameDataFromBytes = new GameData();
        gameDataFromBytes.FromStream(ref bitStream);

        Assert.Equal(gameData, gameDataFromBytes);
    }

    /// <summary>
    /// Internal use only.
    /// </summary>
    public static unsafe GameData Random()
    {
        var data = new GameData();
        data.Gears = new ExtremeGear[Player.OriginalNumberOfGears];
        data.RunningPhysics1 = new RunningPhysics();
        data.RunningPhysics2 = new RunningPhysics2();
        data.RaceSettings = new RaceSettings();

        // Assumes sequential layout.
        var random = new Random();
        fixed (void* gearPtr = &data.Gears[0])
        {
            var gearBytePtr = (byte*)gearPtr;
            var gearsNumBytes = StructArray.GetSize<ExtremeGear>(Player.OriginalNumberOfGears);
            for (int x = 0; x < gearsNumBytes; x++)
                gearBytePtr[x] = (byte)random.Next();
        }

        var restBytePtr = (byte*)&data.RunningPhysics1;
        var restNumBytes = sizeof(RunningPhysics) + sizeof(RunningPhysics2) + sizeof(RaceSettings);
        for (int x = 0; x < restNumBytes; x++)
            restBytePtr[x] = (byte)random.Next();

        return data;
    }
}
