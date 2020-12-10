using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IMailRequests
    {
        bool RequestMailList(bool onlyNewMails, ResponseDelegate<ResponseMailListMessage> callback);
        bool RequestReadMail(string mailId, ResponseDelegate<ResponseReadMailMessage> callback);
        bool RequestClaimMailItems(string mailId, ResponseDelegate<ResponseClaimMailItemsMessage> callback);
        bool RequestDeleteMail(string mailId, ResponseDelegate<ResponseDeleteMailMessage> callback);
        bool RequestSendMail(string receiverName, string title, string content, int gold, ResponseDelegate<ResponseSendMailMessage> callback);
    }
}
