using System;
using System.Collections.Generic;
using System.Reflection;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    [TestFixtureSource(nameof(HandlerTypes))]
    public class SimpleMovementTeleportTests
    {
        private const uint ObjectId = 42;
        private const long OwnerId = 2;
        private const long ObserverId = 1;
        private const long Timestamp = 100;
        private const BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly Type _handlerType;
        private GameObject _root;
        private SimpleTeleportTestManager _manager;
        private SimpleTeleportTestTransport _transport;
        private IEntityMovementDataHandler _handler;
        private FieldInfo _stateField;
        private Action<long> _send;

        public SimpleMovementTeleportTests(Type handlerType)
        {
            _handlerType = handlerType;
        }

        public static IEnumerable<TestFixtureData> HandlerTypes()
        {
            yield return new TestFixtureData(typeof(BuiltInSimpleEntityMovementFunctions3D));
            yield return new TestFixtureData(typeof(SimpleNavMeshEntityMovement));
            yield return new TestFixtureData(typeof(SimpleCharacterControllerEntityMovement));
            // Exercise optional adapters when installed, without making Core depend on them.
            foreach (string name in new[] { "SimpleAgentsNavigationEntityMovement", "SimpleECM2EntityMovement", "SimpleKCCEntityMovement" })
            {
                Type type = typeof(BaseGameEntity).Assembly.GetType("MultiplayerARPG." + name);
                if (type != null)
                {
                    yield return new TestFixtureData(type);
                }
            }
        }

        private MovementTeleportState State
        {
            get => (MovementTeleportState)_stateField.GetValue(_handler);
            set => _stateField.SetValue(_handler, value);
        }

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Simple teleport tests");
            _root.SetActive(false);
            _manager = _root.AddComponent<SimpleTeleportTestManager>();
            _transport = new SimpleTeleportTestTransport();
            _manager.UseTransport(_transport);
            _send = (Action<long>)Delegate.CreateDelegate(typeof(Action<long>), _manager,
                typeof(BaseGameNetworkManager).GetMethod("SendServerEntityMovementState", NonPublicInstance));

            var entityObject = new GameObject("Teleporting character");
            entityObject.transform.SetParent(_root.transform, false);
            entityObject.transform.position = new Vector3(12f, 3f, -4f);
            entityObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            var entity = entityObject.AddComponent<PlayerCharacterEntity>();
            entityObject.AddComponent<LiteNetLibTransform>();
            typeof(LiteNetLibIdentity).GetProperty("ObjectId").SetValue(entity.Identity, ObjectId);
            typeof(LiteNetLibIdentity).GetProperty("ConnectionId").SetValue(entity.Identity, OwnerId);
            typeof(LiteNetLibIdentity).GetProperty("Manager").SetValue(entity.Identity, _manager);
            if (_handlerType == typeof(BuiltInSimpleEntityMovementFunctions3D))
            {
                _handler = new BuiltInSimpleEntityMovementFunctions3D(entity, null, null);
                ((BuiltInSimpleEntityMovementFunctions3D)_handler).OnIdentityInitialize();
            }
            else
            {
                var component = (LiteNetLibBehaviour)entityObject.AddComponent(_handlerType);
                PropertyInfo functions = _handlerType.GetProperty("Functions");
                if (functions != null)
                {
                    functions.SetValue(component, new BuiltInSimpleEntityMovementFunctions3D(entity, null, (IBuiltInEntityMovement3D)component));
                }
                component.OnIdentityInitialize();
                _handler = _manager.EntityMovementDataHandlers[ObjectId];
            }
            _stateField = _handler.GetType().GetField("_serverTeleportState", NonPublicInstance);
            _manager.EntityMovementDataHandlers[ObjectId] = _handler;
        }

        [TearDown]
        public void TearDown()
        {
            if (_handler is BuiltInSimpleEntityMovementFunctions3D functions)
            {
                functions.EntityOnDestroy();
            }
            _manager?.RestoreSingleton();
            Object.DestroyImmediate(_root);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void TeleportReachesOwnerAndObserverBeforeRequestIsConsumed(bool ownerFirst, bool stillMove)
        {
            AddReceivers(ownerFirst);
            RequestTeleport(stillMove);
            _send(Timestamp);

            AssertPackets(ownerFirst, stillMove);
            Assert.That(State, Is.EqualTo(MovementTeleportState.WaitingForResponse));
            _transport.Packets.Clear();
            _send(Timestamp + 1);
            Assert.That(_transport.Packets, Is.Empty, "A sent request must not repeat while awaiting confirmation.");
            Assert.That(State, Is.EqualTo(MovementTeleportState.WaitingForResponse));

            var response = new NetDataWriter();
            response.Put((byte)MovementTeleportState.Responding);
            _handler.ReadClientStateAtServer(Timestamp + 2, new NetDataReader(response.CopyData()));
            Assert.That(State, Is.EqualTo(MovementTeleportState.None), "Only the owner's acknowledgement finishes the wait.");
        }

        [Test]
        public void NoSubscribersPreservesRequestUntilAReceiverAppears()
        {
            AddPlayer(ObserverId, false);
            RequestTeleport(true);
            MovementTeleportState pending = State;
            _send(Timestamp);
            Assert.That(_transport.Packets, Is.Empty);
            Assert.That(State, Is.EqualTo(pending));

            AddPlayer(OwnerId);
            _send(Timestamp);
            Assert.That(_transport.Packets.Count, Is.EqualTo(1));
            AssertPacket(_transport.Packets[0], OwnerId, true);
            Assert.That(State, Is.EqualTo(MovementTeleportState.WaitingForResponse));
        }

        [Test]
        public void InterruptedSendPassRetainsRequestForBothReceiversOnRetry()
        {
            AddReceivers(false);
            RequestTeleport(true);
            MovementTeleportState pending = State;
            _transport.ThrowOnConnection = OwnerId;
            Assert.Throws<InvalidOperationException>(() => _send(Timestamp));
            Assert.That(_transport.Packets.Count, Is.EqualTo(1));
            Assert.That(State, Is.EqualTo(pending));

            _transport.ThrowOnConnection = null;
            _transport.Packets.Clear();
            _send(Timestamp);
            AssertPackets(false, true);
            Assert.That(State, Is.EqualTo(MovementTeleportState.WaitingForResponse));
        }

        [Test]
        public void ConsumingAnIdleOrAcknowledgedHandlerDoesNotStartAnotherWait()
        {
            var flags = _handler as IEntityMovementServerStateFlags;
            Assert.That(flags, Is.Not.Null);
            flags.ConsumeServerStateFlags();
            Assert.That(State, Is.EqualTo(MovementTeleportState.None));

            RequestTeleport(false);
            var response = new NetDataWriter();
            response.Put((byte)MovementTeleportState.Responding);
            _handler.ReadClientStateAtServer(Timestamp, new NetDataReader(response.CopyData()));
            flags.ConsumeServerStateFlags();
            Assert.That(State, Is.EqualTo(MovementTeleportState.None));
        }

        [Test]
        public void TruncatedTeleportThrowsSynchronously()
        {
            Assert.Catch(() => _handler.ReadServerStateAtClient(Timestamp,
                new NetDataReader(new[] { (byte)MovementTeleportState.Requesting })));
        }

        [Test]
        public void HostDecodesTeleportWithoutApplyingIt()
        {
            typeof(LiteNetLibManager.LiteNetLibManager).GetProperty("IsServer").SetValue(_manager, true);
            RequestTeleport(true);
            var writer = new NetDataWriter();
            Assert.That(_handler.WriteServerState(Timestamp, writer, out _), Is.True);
            var reader = new NetDataReader(writer.CopyData());
            Assert.DoesNotThrow(() => _handler.ReadServerStateAtClient(Timestamp, reader));
            Assert.That(reader.EndOfData, Is.True);
            Assert.That(State.Has(MovementTeleportState.Requesting), Is.True);
        }

        private void RequestTeleport(bool stillMove)
        {
            State = MovementTeleportState.Requesting | MovementTeleportState.WaitingForResponse;
            if (stillMove)
            {
                State |= MovementTeleportState.StillMoveAfterTeleport;
            }
        }

        private void AddReceivers(bool ownerFirst)
        {
            AddPlayer(ownerFirst ? OwnerId : ObserverId);
            AddPlayer(ownerFirst ? ObserverId : OwnerId);
        }

        private void AddPlayer(long connectionId, bool subscribing = true)
        {
            var player = new LiteNetLibPlayer(_manager, connectionId) { IsReady = true };
            if (subscribing)
            {
                // Seed interest without spawning a network session or running the unrelated spawn pipeline.
                var subscriptions = (HashSet<uint>)typeof(LiteNetLibPlayer)
                    .GetField("Subscribings", NonPublicInstance).GetValue(player);
                subscriptions.Add(ObjectId);
            }
            _manager.AddPlayer(player);
        }

        private void AssertPackets(bool ownerFirst, bool stillMove)
        {
            Assert.That(_transport.Packets.Count, Is.EqualTo(2), "Both receivers need their own teleport request.");
            AssertPacket(_transport.Packets[0], ownerFirst ? OwnerId : ObserverId, stillMove);
            AssertPacket(_transport.Packets[1], ownerFirst ? ObserverId : OwnerId, stillMove);
        }

        private static void AssertPacket(SimpleTeleportTestTransport.Packet packet, long connectionId, bool stillMove)
        {
            Assert.That(packet.ConnectionId, Is.EqualTo(connectionId));
            Assert.That(packet.DeliveryMethod, Is.EqualTo(DeliveryMethod.ReliableOrdered));
            Assert.That(packet.Channel, Is.EqualTo(BaseGameEntity.MOVEMENT_DATA_CHANNEL));
            var reader = new NetDataReader(packet.Data);
            Assert.That(reader.GetPackedUShort(), Is.EqualTo(GameNetworkingConsts.EntityState));
            Assert.That(reader.GetPackedLong(), Is.EqualTo(Timestamp));
            Assert.That(reader.GetInt(), Is.EqualTo(1), "Each packet must contain exactly one state.");
            Assert.That(reader.GetPackedUInt(), Is.EqualTo(ObjectId));
            Assert.That(reader.GetInt(), Is.EqualTo(reader.AvailableBytes));
            MovementTeleportState expected = MovementTeleportState.Requesting | MovementTeleportState.WaitingForResponse;
            if (stillMove)
            {
                expected |= MovementTeleportState.StillMoveAfterTeleport;
            }
            Assert.That((MovementTeleportState)reader.GetByte(), Is.EqualTo(expected));
            Assert.That(reader.GetFloat(), Is.EqualTo(12f));
            Assert.That(reader.GetFloat(), Is.EqualTo(3f));
            Assert.That(reader.GetFloat(), Is.EqualTo(-4f));
            Assert.That(Mathf.HalfToFloat(reader.GetPackedUShort()), Is.EqualTo(90f));
            Assert.That(reader.EndOfData, Is.True);
        }
    }

    public class SimpleTeleportTestManager : LanRpgNetworkManager
    {
        private BaseGameNetworkManager _previousSingleton;
        protected override void Awake() { }
        protected override void OnDestroy() { }

        public void UseTransport(ITransport transport)
        {
            _previousSingleton = Singleton;
            Singleton = this;
            Server = new LiteNetLibServer(this) { Transport = transport };
        }

        public void RestoreSingleton() { Singleton = _previousSingleton; }

        public void AddPlayer(LiteNetLibPlayer player)
        {
            Players.Add(player.ConnectionId, player);
        }

        public void ReadMovementPacket(NetDataReader reader)
        {
            HandleServerEntityStateAtClient(new MessageHandlerData(GameNetworkingConsts.EntityState, Server, 0, reader));
        }
    }

    public class SimpleTeleportTestTransport : ITransport
    {
        public struct Packet
        {
            public long ConnectionId;
            public byte Channel;
            public DeliveryMethod DeliveryMethod;
            public byte[] Data;
        }

        public readonly List<Packet> Packets = new List<Packet>();
        public long? ThrowOnConnection;
        public int ServerPeersCount => 0;
        public int ServerMaxConnections => 0;
        public bool IsClientStarted => false;
        public bool IsServerStarted => true;
        public bool HasImplementedPing => false;
        public bool IsReliableOnly => false;
        public bool StartClient(string address, int port) => false;
        public bool ClientSend(byte dataChannel, DeliveryMethod deliveryMethod, NetDataWriter writer) => false;
        public bool ClientReceive(out TransportEventData eventData) { eventData = default; return false; }
        public void StopClient() { }
        public bool StartServer(int port, int maxConnections) => false;
        public bool ServerSend(long connectionId, byte dataChannel, DeliveryMethod deliveryMethod, NetDataWriter writer)
        {
            if (connectionId == ThrowOnConnection)
            {
                throw new InvalidOperationException("Simulated interrupted teleport send pass.");
            }
            Packets.Add(new Packet { ConnectionId = connectionId, Channel = dataChannel,
                DeliveryMethod = deliveryMethod, Data = writer.CopyData() });
            return true;
        }
        public bool ServerReceive(out TransportEventData eventData) { eventData = default; return false; }
        public bool ServerDisconnect(long connectionId) => false;
        public void StopServer() { }
        public void Destroy() { }
        public long GetClientRtt() => 0;
        public long GetServerRtt(long connectionId) => 0;
    }
}
