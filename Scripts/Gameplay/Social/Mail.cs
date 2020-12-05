using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class Mail
    {
        public string Id { get; set; }
        public string EventId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Gold { get; set; }
        public Dictionary<int, int> Currencies { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, short> Items { get; set; } = new Dictionary<int, short>();
        public bool IsRead { get; set; }
        public int ReadTimestamp { get; set; }
        public bool IsClaim { get; set; }
        public int ClaimTimestamp { get; set; }
        public bool IsDelete { get; set; }
        public int DeleteTimestamp { get; set; }
        public int SentTimestamp { get; set; }

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
    }
}