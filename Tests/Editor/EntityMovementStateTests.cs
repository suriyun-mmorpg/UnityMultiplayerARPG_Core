using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    [TestFixtureSource(nameof(HandlerTypes))]
    public class EntityMovementStateTests
    {
        public static IEnumerable<TestFixtureData> HandlerTypes()
        {
            yield return new TestFixtureData(typeof(BuiltInEntityMovementFunctions3D));
            yield return new TestFixtureData(typeof(NavMeshEntityMovement));
            yield return new TestFixtureData(typeof(RigidBodyEntityMovement2D));
            foreach (string name in new[] { "ECM2EntityMovement", "KCCEntityMovement", "AstarCharacterMovement2D" })
            {
                Type type = typeof(BaseGameEntity).Assembly.GetType("MultiplayerARPG." + name);
                if (type != null) yield return new TestFixtureData(type);
            }
        }

        private readonly Type _type;
        private MovementStateTestRig _rig;

        public EntityMovementStateTests(Type type) { _type = type; }
        [SetUp] public void SetUp() { _rig = new MovementStateTestRig(_type); }
        [TearDown] public void TearDown() { _rig?.Dispose(); }

        [TestCase(MovementSecure.NotSecure, false, false)]
        [TestCase(MovementSecure.NotSecure, true, false)]
        [TestCase(MovementSecure.NotSecure, true, true)]
        [TestCase(MovementSecure.ServerAuthoritative, false, false)]
        [TestCase(MovementSecure.ServerAuthoritative, true, false)]
        [TestCase(MovementSecure.ServerAuthoritative, true, true)]
        public void OneShotFlagsReachEveryReceiverThenReturnToUnreliable(MovementSecure secure, bool teleport, bool stillMove)
        {
            _rig.Set("movementSecure", secure);
            _rig.Set("MovementState", MovementState.Forward | MovementState.IsGrounded);
            _rig.Set(teleport ? "_isTeleporting" : "_sendingDash", true);
            _rig.Set("_stillMoveAfterTeleport", stillMove);
            _rig.Set("_isServerWaitingTeleportConfirm", true);
            _rig.Set("_isClientConfirmingTeleport", true);
            _rig.AddReceiver(1);
            _rig.AddReceiver(2);
            _rig.Send(100);

            Assert.That(_rig.Transport.Packets.Count, Is.EqualTo(2));
            foreach (var packet in _rig.Transport.Packets)
            {
                Assert.That(packet.DeliveryMethod, Is.EqualTo(DeliveryMethod.ReliableOrdered));
                var reader = MovementStateTestRig.ReadPacket(packet, 100, 42);
                MovementState state = _rig.Decode(reader);
                Assert.That(state.Has(teleport ? MovementState.IsTeleport : MovementState.IsDash), Is.True);
                Assert.That(state.Has(MovementState.Forward), Is.EqualTo(!teleport || stillMove));
                Assert.That(reader.EndOfData, Is.True);
            }
            Assert.That(_rig.Get<bool>(teleport ? "_isTeleporting" : "_sendingDash"), Is.False);
            Assert.That(_rig.Get<bool>("_stillMoveAfterTeleport"), Is.False);
            Assert.That(_rig.Get<bool>("_isServerWaitingTeleportConfirm"), Is.True);
            Assert.That(_rig.Get<bool>("_isClientConfirmingTeleport"), Is.True);

            _rig.Transport.Packets.Clear();
            _rig.Send(101);
            Assert.That(_rig.Transport.Packets.Count, Is.EqualTo(2));
            foreach (var packet in _rig.Transport.Packets)
            {
                Assert.That(packet.DeliveryMethod, Is.EqualTo(DeliveryMethod.Unreliable));
                MovementState state = _rig.Decode(MovementStateTestRig.ReadPacket(packet, 101, 42));
                Assert.That(state & (MovementState.IsJump | MovementState.IsDash | MovementState.IsTeleport), Is.EqualTo(MovementState.None));
            }
        }

        [Test]
        public void NoSubscribersAndAnInterruptedPassKeepFlagsPending()
        {
            _rig.Set("_sendingDash", true);
            _rig.Send(100);
            Assert.That(_rig.Get<bool>("_sendingDash"), Is.True);
            _rig.AddReceiver(1);
            _rig.AddReceiver(2);
            _rig.Transport.ThrowOnConnection = 2;
            Assert.Throws<InvalidOperationException>(() => _rig.Send(101));
            Assert.That(_rig.Get<bool>("_sendingDash"), Is.True);
            _rig.Transport.ThrowOnConnection = null;
            _rig.Transport.Packets.Clear();
            _rig.Send(102);
            Assert.That(_rig.Transport.Packets.Count, Is.EqualTo(2));
            foreach (var packet in _rig.Transport.Packets)
            {
                Assert.That(_rig.Decode(MovementStateTestRig.ReadPacket(packet, 102, 42)).Has(MovementState.IsDash), Is.True);
            }
            Assert.That(_rig.Get<bool>("_sendingDash"), Is.False);
        }

        [Test]
        public void HostDecodesTheEntireStateAndForceListWithoutApplyingIt()
        {
            _rig.Forces.Add(new EntityMovementForceApplier { Direction = Vector3.right, CurrentSpeed = 4f, Duration = 2f });
            var writer = new NetDataWriter();
            Assert.That(_rig.Handler.WriteServerState(100, writer, out _), Is.True);
            var reader = new NetDataReader(writer.CopyData());
            Assert.DoesNotThrow(() => _rig.Handler.ReadServerStateAtClient(100, reader));
            Assert.That(reader.EndOfData, Is.True);
            Assert.That(_rig.Entity.transform.position, Is.EqualTo(new Vector3(12f, 3f, -4f)));
            _rig.Decode(new NetDataReader(writer.CopyData()), 1);
        }

        [Test]
        public void TruncatedStateFailsSynchronouslyAndTheNextFrameStillReads()
        {
            var reader = new NetDataReader(new byte[] { 0 });
            Assert.Catch(() => _rig.Handler.ReadServerStateAtClient(100, reader));
            reader = new NetDataReader(new byte[] { 0 });
            Assert.That(BaseGameNetworkManager.ReadServerEntityState(_rig.Handler, 100, reader, 1), Is.False);
            Assert.That(reader.Position, Is.EqualTo(1));
            var next = new MovementFrameProbe();
            reader = new NetDataReader(new byte[] { 0, 7 });
            BaseGameNetworkManager.ReadServerEntityState(_rig.Handler, 100, reader, 1);
            Assert.That(BaseGameNetworkManager.ReadServerEntityState(next, 100, reader, 1), Is.True);
            Assert.That(next.LastByte, Is.EqualTo(7));
        }

        [TestCase(MovementSecure.NotSecure)]
        [TestCase(MovementSecure.ServerAuthoritative)]
        public void TeleportConfirmationClearsServerWait(MovementSecure secure)
        {
            _rig.Set("movementSecure", secure);
            _rig.Set("_isServerWaitingTeleportConfirm", true);
            var writer = new NetDataWriter();
            if (secure == MovementSecure.ServerAuthoritative)
            {
                writer.Put((byte)EntityMovementInputState.IsStopped);
                writer.PutPackedUInt((uint)MovementState.IsTeleport);
                if (_rig.Is2D)
                    writer.Put((DirectionVector2)Vector2.down);
            }
            else
            {
                writer.PutPackedUInt((uint)MovementState.IsTeleport);
                writer.Put((byte)ExtraMovementState.None);
                if (_rig.Is2D)
                {
                    writer.PutVector2(_rig.Entity.transform.position);
                    writer.Put((DirectionVector2)Vector2.down);
                }
                else
                {
                    writer.PutVector3(_rig.Entity.transform.position);
                    writer.PutPackedInt(0);
                    if (_rig.Type == typeof(BuiltInEntityMovementFunctions3D)) new EntityMovementPlatformState().Write(writer);
                }
            }
            // Reject simulation as stale after the confirmation is handled.
            _rig.Set("_acceptedPositionTimestamp", 200L);
            _rig.Handler.ReadClientStateAtServer(100, new NetDataReader(writer.CopyData()));
            Assert.That(_rig.Get<bool>("_isServerWaitingTeleportConfirm"), Is.False);
        }
    }

    public class MovementStateTestRig : IDisposable
    {
        public readonly GameObject Root;
        public readonly PlayerCharacterEntity Entity;
        public readonly SimpleTeleportTestManager Manager;
        public readonly SimpleTeleportTestTransport Transport = new SimpleTeleportTestTransport();
        public readonly IEntityMovementDataHandler Handler;
        public readonly MovementTestMotor Motor;
        public readonly Action<long> Send;
        public readonly Type Type;
        public bool Is2D => Handler is RigidBodyEntityMovement2D;
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public MovementStateTestRig(Type type)
        {
            Root = new GameObject("Movement state tests");
            Root.SetActive(false);
            Manager = Root.AddComponent<SimpleTeleportTestManager>();
            Manager.UseTransport(Transport);
            SetRole(true, false);
            Send = (Action<long>)Delegate.CreateDelegate(typeof(Action<long>), Manager,
                typeof(BaseGameNetworkManager).GetMethod("SendServerEntityMovementState", Members));
            var child = new GameObject("Moving entity");
            child.transform.SetParent(Root.transform);
            child.transform.position = new Vector3(12f, 3f, -4f);
            Entity = child.AddComponent<PlayerCharacterEntity>();
            typeof(LiteNetLibIdentity).GetProperty("Manager").SetValue(Entity.Identity, Manager);
            typeof(LiteNetLibIdentity).GetProperty("ObjectId").SetValue(Entity.Identity, 42u);
            typeof(LiteNetLibIdentity).GetProperty("ConnectionId").SetValue(Entity.Identity, 2L);
            IEntityMovementComponent movement;
            if (type == typeof(BuiltInEntityMovementFunctions3D) || typeof(IBuiltInEntityMovement3D).IsAssignableFrom(type))
            {
                var component = (LiteNetLibBehaviour)child.AddComponent(type == typeof(BuiltInEntityMovementFunctions3D)
                    ? typeof(CharacterControllerEntityMovement) : type);
                Motor = new MovementTestMotor(child.transform);
                var functions = new BuiltInEntityMovementFunctions3D(Entity, null, Motor);
                component.GetType().GetProperty("Functions").SetValue(component, functions);
                Handler = functions;
                movement = (IEntityMovementComponent)component;
                component.OnIdentityInitialize();
                Assert.That(Manager.EntityMovementDataHandlers[42], Is.SameAs(Handler));
            }
            else
            {
                var component = child.AddComponent(type);
                Handler = (IEntityMovementDataHandler)component;
                movement = (IEntityMovementComponent)component;
            }
            typeof(BaseGameEntity).GetProperty("Movement").SetValue(Entity, movement);
            Type = Handler.GetType();
            if (!Is2D) Set("_isStarted", true);
            Manager.EntityMovementDataHandlers[42] = Handler;
        }

        public void SetRole(bool server, bool client)
        {
            typeof(LiteNetLibManager.LiteNetLibManager).GetProperty("IsServer").SetValue(Manager, server);
            typeof(LiteNetLibManager.LiteNetLibManager).GetProperty("IsClient").SetValue(Manager, client);
        }

        public void Set(string name, object value)
        {
            var field = Type.GetField(name, Members);
            if (field != null) field.SetValue(Handler, value);
            else Type.GetProperty(name, Members).SetValue(Handler, value);
        }

        public T Get<T>(string name) => (T)Type.GetField(name, Members).GetValue(Handler);
        public List<EntityMovementForceApplier> Forces => Get<List<EntityMovementForceApplier>>("_movementForceAppliers");

        public void AddReceiver(long id, params uint[] objects)
        {
            var player = new LiteNetLibPlayer(Manager, id) { IsReady = true };
            var subscriptions = (HashSet<uint>)typeof(LiteNetLibPlayer).GetField("Subscribings", Members).GetValue(player);
            foreach (uint objectId in objects.Length == 0 ? new uint[] { 42 } : objects) subscriptions.Add(objectId);
            Manager.AddPlayer(player);
        }

        public static NetDataReader ReadPacket(SimpleTeleportTestTransport.Packet packet, long timestamp, uint objectId)
        {
            var reader = new NetDataReader(packet.Data);
            Assert.That(reader.GetPackedUShort(), Is.EqualTo(GameNetworkingConsts.EntityState));
            Assert.That(reader.GetPackedLong(), Is.EqualTo(timestamp));
            Assert.That(reader.GetInt(), Is.EqualTo(1));
            Assert.That(reader.GetPackedUInt(), Is.EqualTo(objectId));
            Assert.That(reader.GetInt(), Is.EqualTo(reader.AvailableBytes));
            return reader;
        }

        public MovementState Decode(NetDataReader reader, int forceCount = 0)
        {
            MovementState state;
            List<EntityMovementForceApplier> forces;
            if (Is2D)
            {
                reader.ClientReadSyncTransformMessage2D(out state, out _, out Vector2 position, out _, out forces);
                Assert.That(position, Is.EqualTo((Vector2)Entity.transform.position));
            }
            else
            {
                reader.ClientReadSyncTransformMessage3D(out state, out _, out Vector3 position, out _, out forces);
                Assert.That(position, Is.EqualTo(Entity.transform.position));
                if (Type == typeof(BuiltInEntityMovementFunctions3D)) EntityMovementPlatformState.Read(reader);
            }
            Assert.That(forces.Count, Is.EqualTo(forceCount));
            if (forceCount > 0)
            {
                Assert.That(forces[0].CurrentSpeed, Is.EqualTo(4f));
                Assert.That(forces[0].Direction, Is.EqualTo(Vector3.right));
            }
            Assert.That(reader.EndOfData, Is.True);
            return state;
        }

        public void Dispose()
        {
            Manager.RestoreSingleton();
            Object.DestroyImmediate(Root);
        }
    }

    public class MovementTestMotor : IBuiltInEntityMovement3D
    {
        private readonly Transform _transform;
        public MovementTestMotor(Transform transform) { _transform = transform; }
        public bool isActiveAndEnabled => true;
        public bool GroundCheck() => true;
        public bool AirborneCheck() => false;
        public void SetPosition(Vector3 position) { _transform.position = position; }
        public void Move(MovementState movementState, ExtraMovementState extraMovementState, Vector3 motion, float deltaTime) { _transform.position += motion; }
        public void RotateY(float angle) { _transform.rotation = Quaternion.Euler(0f, angle, 0f); }
        public void OnJumpForceApplied(float velocity) { }
        public Bounds GetMovementBounds() => new Bounds(_transform.position, Vector3.one * 10f);
        public Vector3 GetSnapToGroundMotion(Vector3 motion, Vector3 platformMotion, Vector3 forceMotion) => Vector3.zero;
    }

    public class MovementFrameProbe : IEntityMovementDataHandler
    {
        public uint ObjectId => 43;
        public long ConnectionId => 2;
        public int BytesToRead = 1;
        public bool Throw;
        public byte LastByte;
        public int Reads;
        public void ReadServerStateAtClient(long timestamp, NetDataReader reader)
        {
            ++Reads;
            for (int i = 0; i < BytesToRead; ++i) LastByte = reader.GetByte();
            if (Throw) throw new InvalidOperationException("Malformed movement frame");
        }
        public void ReadClientStateAtServer(long timestamp, NetDataReader reader) { }
        public bool WriteServerState(long timestamp, NetDataWriter writer, out bool reliable) { reliable = false; writer.Put((byte)7); return true; }
        public bool WriteClientState(long timestamp, NetDataWriter writer, out bool reliable) { reliable = false; return false; }
    }
}
