namespace MultiplayerARPG
{
    public class MapInfoDatabaseSection : BaseGameDataListSection<BaseMapInfo>
    {
        public override int Order { get { return 13; } }

        public override string MenuTitle { get { return "Map Infos"; } }

        protected override string FieldName { get { return "mapInfos"; } }
    }
}
