﻿using System.Numerics;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.Structures.Enums;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class UnreliablePacket
    {
        [Fact]
        public void SerializeUnreliablePacketAllData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var rings     = (byte) 52;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;
            var air       = (uint) 123456;

            var player            = new UnreliablePacketPlayer(position, air, rings, PlayerState.ElectricShockCrash, rotation, new Vector2(velocityX, velocityY));
            var unreliablePacket  = new Messages.UnreliablePacket(new[] { player });

            var bytes        = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }

        [Fact]
        public void SerializeUnreliablePacketPartialData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;

            var player = new UnreliablePacketPlayer(position, 0, null, default, rotation, new Vector2(velocityX, velocityY));
            var unreliablePacket = new Messages.UnreliablePacket(new[] { player });

            var bytes = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }
    }
}
