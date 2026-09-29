using System;
using System.Linq;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MultiplayerARPG.Tests
{
    public class CharacterItemSocketsTests
    {
        [Test]
        public void IndexerEnumeratorAndListCoverEveryConfiguredSocket()
        {
            CharacterItemSockets sockets = default;
            for (int index = 0; index < CharacterItemSockets.MAX_SOCKETS; ++index)
                sockets[index] = index * 7 - 3;

            Assert.That(sockets.Count, Is.EqualTo(CharacterItemSockets.MAX_SOCKETS));
            Assert.That(sockets.ToArray(), Is.EqualTo(sockets.ToList()));
            for (int index = 0; index < sockets.Count; ++index)
                Assert.That(sockets[index], Is.EqualTo(index * 7 - 3), $"Socket {index}");

            Assert.Throws<IndexOutOfRangeException>(() => { int unused = sockets[-1]; });
            Assert.Throws<IndexOutOfRangeException>(() => sockets[-1] = 1);
            Assert.Throws<IndexOutOfRangeException>(() => { int unused = sockets[sockets.Count]; });
            Assert.Throws<IndexOutOfRangeException>(() => sockets[sockets.Count] = 1);
        }

        [Test]
        public void ListConversionTruncatesPadsAndResetsValues()
        {
            int[] longList = Enumerable.Range(1, CharacterItemSockets.MAX_SOCKETS + 2).ToArray();
            CharacterItemSockets sockets = longList.ToCharacterItemSockets();
            Assert.That(sockets.ToList(), Is.EqualTo(longList.Take(CharacterItemSockets.MAX_SOCKETS)));

            sockets.SetSockets(new[] { 8, 13 });
            Assert.That(sockets[0], Is.EqualTo(8));
            Assert.That(sockets[1], Is.EqualTo(13));
            for (int index = 2; index < sockets.Count; ++index)
                Assert.That(sockets[index], Is.Zero, $"Socket {index} was not cleared");

            sockets.SetSockets(null);
            Assert.That(sockets.ToList(), Is.All.EqualTo(0));
        }

        [Test]
        public void JsonAndNetworkRoundTripsPreserveEverySocket()
        {
            CharacterItemSockets source = default;
            for (int index = 0; index < source.Count; ++index)
                source[index] = index % 2 == 0 ? index + 1 : -index;

            string json = JsonConvert.SerializeObject(source);
            CharacterItemSockets fromJson = JsonConvert.DeserializeObject<CharacterItemSockets>(json);
            CharacterItemSockets fromNull = JsonConvert.DeserializeObject<CharacterItemSockets>("null");
            NetDataWriter writer = new NetDataWriter();
            source.Serialize(writer);
            NetDataReader reader = new NetDataReader();
            reader.SetSource(writer.CopyData());
            CharacterItemSockets fromNetwork = default;
            fromNetwork.Deserialize(reader);

            uint expectedStates = source.Count == 32 ? uint.MaxValue : (1u << source.Count) - 1u;
            Assert.That((uint)source.GetStates(), Is.EqualTo(expectedStates));
            for (int index = 0; index < source.Count; ++index)
            {
                Assert.That(fromJson[index], Is.EqualTo(source[index]), $"JSON socket {index}");
                Assert.That(fromNetwork[index], Is.EqualTo(source[index]), $"Network socket {index}");
                Assert.That(fromNull[index], Is.Zero, $"Null JSON socket {index}");
            }
        }
    }
}
