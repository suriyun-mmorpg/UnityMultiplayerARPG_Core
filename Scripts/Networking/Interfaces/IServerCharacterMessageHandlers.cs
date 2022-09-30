using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerCharacterMessageHandlers
    {
        UniTaskVoid HandleRequestIncreaseAttributeAmount(
            RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request,
            RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result);

        UniTaskVoid HandleRequestIncreaseSkillLevel(
            RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request,
            RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result);

        UniTaskVoid HandleRequestRespawn(
            RequestHandlerData requestHandler, RequestRespawnMessage request,
            RequestProceedResultDelegate<ResponseRespawnMessage> result);

        UniTaskVoid RequestAvailableIcons(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseAvailableIconsMessage> result);

        UniTaskVoid RequestAvailableFrames(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseAvailableFramesMessage> result);

        UniTaskVoid RequestAvailableTitles(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseAvailableTitlesMessage> result);

        UniTaskVoid RequestSetIcon(
            RequestHandlerData requestHandler, RequestSetIconMessage request,
            RequestProceedResultDelegate<ResponseSetIconMessage> result);

        UniTaskVoid RequestSetFrame(
            RequestHandlerData requestHandler, RequestSetFrameMessage request,
            RequestProceedResultDelegate<ResponseSetFrameMessage> result);

        UniTaskVoid RequestSetTitle(
            RequestHandlerData requestHandler, RequestSetTitleMessage request,
            RequestProceedResultDelegate<ResponseSetTitleMessage> result);
    }
}
