using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class Mail : INetSerializable
    {
        public string Id { get; set; }
        public string EventId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Gold { get; set; }
        public int Cash { get; set; }
        public Dictionary<int, int> Currencies { get; } = new Dictionary<int, int>();
        public Dictionary<int, short> Items { get; } = new Dictionary<int, short>();
        public bool IsRead { get; set; }
        public long ReadTimestamp { get; set; }
        public bool IsClaim { get; set; }
        public long ClaimTimestamp { get; set; }
        public bool IsDelete { get; set; }
        public long DeleteTimestamp { get; set; }
        public long SentTimestamp { get; set; }

        public bool HaveItemsToClaim()
        {
            return Gold != 0 || Cash != 0 || Currencies.Count > 0 || Items.Count > 0;
        }

        public Dictionary<int, int> ReadCurrencies(string currencies)
        {
            Currencies.Clear();
            string[] splitSets = currencies.Split(';');
            foreach (string set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                string[] splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                Currencies[int.Parse(splitData[0])] = int.Parse(splitData[1]);
            }
            return Currencies;
        }

        public string WriteCurrencies()
        {
            string result = string.Empty;
            foreach (KeyValuePair<int, int> keyValue in Currencies)
            {
                result += $"{keyValue.Key}:{keyValue.Value};";
            }
            return result;
        }

        public Dictionary<int, short> ReadItems(string items)
        {
            Items.Clear();
            string[] splitSets = items.Split(';');
            foreach (string set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                string[] splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                Items[int.Parse(splitData[0])] = short.Parse(splitData[1]);
            }
            return Items;
        }

        public string WriteItems()
        {
            string result = string.Empty;
            foreach (KeyValuePair<int, short> keyValue in Items)
            {
                result += $"{keyValue.Key}:{keyValue.Value};";
            }
            return result;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(EventId);
            writer.Put(SenderId);
            writer.Put(SenderName);
            writer.Put(ReceiverId);
            writer.Put(Title);
            writer.Put(Content);
            writer.Put(Gold);
            writer.Put(Cash);
            writer.Put(WriteCurrencies());
            writer.Put(WriteItems());
            writer.Put(IsRead);
            writer.Put(ReadTimestamp);
            writer.Put(IsClaim);
            writer.Put(ClaimTimestamp);
            writer.Put(IsDelete);
            writer.Put(DeleteTimestamp);
            writer.Put(SentTimestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
            EventId = reader.GetString();
            SenderId = reader.GetString();
            SenderName = reader.GetString();
            ReceiverId = reader.GetString();
            Title = reader.GetString();
            Content = reader.GetString();
            Gold = reader.GetInt();
            Cash = reader.GetInt();
            ReadCurrencies(reader.GetString());
            ReadItems(reader.GetString());
            IsRead = reader.GetBool();
            ReadTimestamp = reader.GetInt();
            IsClaim = reader.GetBool();
            ClaimTimestamp = reader.GetInt();
            IsDelete = reader.GetBool();
            DeleteTimestamp = reader.GetInt();
            SentTimestamp = reader.GetInt();
        }
    }
}
