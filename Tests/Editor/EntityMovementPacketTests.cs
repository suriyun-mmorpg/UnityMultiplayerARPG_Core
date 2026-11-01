using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class EntityMovementPacketTests
    {
        private MovementStateTestRig _rig;
        private LoggerManager _previousLogger;

        [SetUp]
        public void SetUp()
        {
            _previousLogger = LogManager.LoggerManager;
            LogManager.LoggerManager = new LoggerManager(new DefaultLoggerFactory(null));
            _rig = new MovementStateTestRig(typeof(BuiltInEntityMovementFunctions3D));
        }

        [TearDown]
        public void TearDown()
        {
            _rig?.Dispose();
            LogManager.LoggerManager = _previousLogger;
        }

        [Test]
        public void JumpReachesBothObserversAndIsConsumedOnce()
        {
            _rig.Set("_sendingJump", true);
            _rig.AddReceiver(1);
            _rig.AddReceiver(2);
            _rig.Send(100);
            Assert.That(_rig.Transport.Packets.Count, Is.EqualTo(2));
            foreach (var packet in _rig.Transport.Packets)
            {
                Assert.That(packet.DeliveryMethod, Is.EqualTo(DeliveryMethod.ReliableOrdered));
                Assert.That(_rig.Decode(MovementStateTestRig.ReadPacket(packet, 100, 42)).Has(MovementState.IsJump), Is.True);
            }
            Assert.That(_rig.Get<bool>("_sendingJump"), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PlayersReceiveOnlyTheirSubscribedStates(bool reliable)
        {
            _rig.Manager.EntityMovementDataHandlers[43] = new PacketWriterProbe(reliable, 1);
            _rig.Set("_sendingJump", reliable);
            _rig.AddReceiver(1, 42);
            _rig.AddReceiver(2, 43);
            _rig.Send(100);
            Assert.That(_rig.Transport.Packets.Count, Is.EqualTo(2));
            _rig.Decode(MovementStateTestRig.ReadPacket(_rig.Transport.Packets[0], 100, 42));
            var reader = MovementStateTestRig.ReadPacket(_rig.Transport.Packets[1], 100, 43);
            Assert.That(reader.GetByte(), Is.EqualTo(7));
            Assert.That(reader.EndOfData, Is.True);
        }

        [Test]
        public void UnreliablePacketSplittingDoesNotDuplicateOrLeakStatesBetweenPlayers()
        {
            var ids = new uint[100];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = (uint)(100 + i);
                _rig.Manager.EntityMovementDataHandlers[ids[i]] = new PacketWriterProbe(false, 40);
            }
            _rig.AddReceiver(1, ids);
            _rig.AddReceiver(2, 42);
            _rig.Send(100);
            var received = new HashSet<uint>();
            int ownerPackets = 0;
            foreach (var packet in _rig.Transport.Packets)
            {
                if (packet.ConnectionId == 2)
                {
                    ++ownerPackets;
                    _rig.Decode(MovementStateTestRig.ReadPacket(packet, 100, 42));
                    continue;
                }
                var reader = new NetDataReader(packet.Data);
                Assert.That(reader.GetPackedUShort(), Is.EqualTo(GameNetworkingConsts.EntityState));
                Assert.That(reader.GetPackedLong(), Is.EqualTo(100));
                int count = reader.GetInt();
                Assert.That(count, Is.GreaterThan(0));
                for (int i = 0; i < count; ++i)
                {
                    Assert.That(received.Add(reader.GetPackedUInt()), Is.True, "A subscribed state was repeated.");
                    int length = reader.GetInt();
                    Assert.That(length, Is.EqualTo(40));
                    reader.SkipBytes(length);
                }
                Assert.That(reader.EndOfData, Is.True);
            }
            CollectionAssert.AreEquivalent(ids, received);
            Assert.That(ownerPackets, Is.EqualTo(1));
            Assert.That(_rig.Transport.Packets.Count, Is.GreaterThan(2));
        }

        [TestCase(0, false)]
        [TestCase(2, false)]
        [TestCase(1, true)]
        public void StateFramingRecoversFromUnderReadOverReadAndExceptions(int bytesToRead, bool throws)
        {
            var first = new MovementFrameProbe { BytesToRead = bytesToRead, Throw = throws };
            var second = new MovementFrameProbe();
            _rig.Manager.EntityMovementDataHandlers[42] = first;
            _rig.Manager.EntityMovementDataHandlers[43] = second;
            var writer = PacketHeader(2);
            WriteFrame(writer, 42, 1, new byte[] { 3 });
            WriteFrame(writer, 43, 1, new byte[] { 7 });
            var reader = new NetDataReader(writer.CopyData());
            _rig.Manager.ReadMovementPacket(reader);
            Assert.That(second.Reads, Is.EqualTo(1));
            Assert.That(second.LastByte, Is.EqualTo(7));
            Assert.That(reader.EndOfData, Is.True);
        }

        [TestCase(-1)]
        [TestCase(1000)]
        public void InvalidStateLengthStopsBeforeApplyingAnyState(int length)
        {
            var handler = new MovementFrameProbe();
            _rig.Manager.EntityMovementDataHandlers[43] = handler;
            var writer = PacketHeader(1);
            WriteFrame(writer, 43, length, new byte[] { 7 });
            Assert.DoesNotThrow(() => _rig.Manager.ReadMovementPacket(new NetDataReader(writer.CopyData())));
            Assert.That(handler.Reads, Is.Zero);
        }

        [Test]
        public void UnknownEntityIsSkippedWithoutLosingTheNextState()
        {
            var handler = new MovementFrameProbe();
            _rig.Manager.EntityMovementDataHandlers[43] = handler;
            var writer = PacketHeader(2);
            WriteFrame(writer, 999, 2, new byte[] { 8, 9 });
            WriteFrame(writer, 43, 1, new byte[] { 7 });
            _rig.Manager.ReadMovementPacket(new NetDataReader(writer.CopyData()));
            Assert.That(handler.Reads, Is.EqualTo(1));
            Assert.That(handler.LastByte, Is.EqualTo(7));
        }

        [Test]
        public void TeleportReadsPlatformBytesBeforeAwaitAndResetsInterpolationAtDestination()
        {
            _rig.SetRole(false, true);
            var preparer = new DelayedMovementTeleportPreparer();
            _rig.Set("TeleportPreparer", preparer);
            _rig.Set("_startInterpPosition", Vector3.zero);
            _rig.Set("_endInterpPosition", Vector3.one);
            _rig.Set("_interpElapsedTime", 4f);
            Vector3 destination = new Vector3(50f, 2f, 75f);
            var writer = new NetDataWriter();
            writer.PutPackedUInt((uint)MovementState.IsTeleport);
            writer.Put((byte)ExtraMovementState.None);
            writer.PutVector3(destination);
            writer.PutPackedInt(EntityMovementFunctions.GetCompressedAngle(90f));
            writer.PutList(new List<EntityMovementForceApplier>());
            new EntityMovementPlatformState { objectId = 123, localPosition = Vector3.right }.Write(writer);
            int stateLength = writer.Length;
            writer.Put((byte)7);
            var reader = new NetDataReader(writer.CopyData());
            Assert.That(BaseGameNetworkManager.ReadServerEntityState(_rig.Handler, 100, reader, stateLength), Is.True);
            Assert.That(preparer.IsPreparingToTeleport, Is.True);
            Assert.That(_rig.Entity.transform.position, Is.Not.EqualTo(destination));
            Assert.That(reader.GetByte(), Is.EqualTo(7));
            // Reuse the shared buffer before the asynchronous application resumes.
            Array.Clear(reader.RawData, 0, reader.RawData.Length);
            preparer.Complete();
            Assert.That(_rig.Entity.transform.position, Is.EqualTo(destination));
            Assert.That(_rig.Get<Vector3>("_startInterpPosition"), Is.EqualTo(destination));
            Assert.That(_rig.Get<Vector3>("_endInterpPosition"), Is.EqualTo(destination));
            Assert.That(_rig.Get<float>("_interpElapsedTime"), Is.Zero);
            Assert.That(_rig.Get<EntityMovementPlatformState>("_receivedPlatform").objectId, Is.Zero);
        }

        private static NetDataWriter PacketHeader(int count)
        {
            var writer = new NetDataWriter();
            writer.PutPackedLong(100);
            writer.Put(count);
            return writer;
        }

        private static void WriteFrame(NetDataWriter writer, uint objectId, int length, byte[] payload)
        {
            writer.PutPackedUInt(objectId);
            writer.Put(length);
            writer.Put(payload);
        }

        private class PacketWriterProbe : MovementFrameProbe, IEntityMovementDataHandler
        {
            private readonly bool _reliable;
            private readonly int _length;
            public PacketWriterProbe(bool reliable, int length) { _reliable = reliable; _length = length; }
            bool IEntityMovementDataHandler.WriteServerState(long timestamp, NetDataWriter writer, out bool reliable)
            {
                reliable = _reliable;
                for (int i = 0; i < _length; ++i) writer.Put((byte)7);
                return true;
            }
        }
    }

    public class DelayedMovementTeleportPreparer : IEntityTeleportPreparer
    {
        private readonly UniTaskCompletionSource _completion = new UniTaskCompletionSource();
        public bool IsPreparingToTeleport { get; private set; }
        public Vector3 PrepareTeleportPosition { get; private set; }
        public UniTask PrepareToTeleport(Vector3 position, Quaternion rotation)
        {
            IsPreparingToTeleport = true;
            PrepareTeleportPosition = position;
            return _completion.Task;
        }
        public void Complete() { IsPreparingToTeleport = false; _completion.TrySetResult(); }
    }
}
