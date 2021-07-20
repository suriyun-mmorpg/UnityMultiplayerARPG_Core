using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientMailActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMailListMessage> onResponseMailList;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseReadMailMessage> onResponseReadMail;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseClaimMailItemsMessage> onResponseClaimMailItems;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDeleteMailMessage> onResponseDeleteMail;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSendMailMessage> onResponseSendMail;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMailNotificationMessage> onResponseMailNotification;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseClaimAllMailsItemsMessage> onResponseClaimAllMailsItems;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDeleteAllMailsMessage> onResponseDeleteAllMails;

        public static void ResponseMailList(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMailListMessage response)
        {
            if (onResponseMailList != null)
                onResponseMailList.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseReadMail(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseReadMailMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseReadMail != null)
                onResponseReadMail.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseClaimMailItems(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseClaimMailItemsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseClaimMailItems != null)
                onResponseClaimMailItems.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseDeleteMail(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeleteMailMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeleteMail != null)
                onResponseDeleteMail.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseSendMail(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendMailMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSendMail != null)
                onResponseSendMail.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseMailNotification(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMailNotificationMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseMailNotification != null)
                onResponseMailNotification.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseClaimAllMailsItems(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseClaimAllMailsItemsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseClaimAllMailsItems != null)
                onResponseClaimAllMailsItems.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseDeleteAllMails(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeleteAllMailsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeleteAllMails != null)
                onResponseDeleteAllMails.Invoke(requestHandler, responseCode, response);
        }
    }
}
