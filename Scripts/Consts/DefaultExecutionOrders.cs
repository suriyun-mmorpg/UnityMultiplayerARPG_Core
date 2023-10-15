namespace MultiplayerARPG
{
    public partial class DefaultExecutionOrders
    {
        public const int GAME_INSTANCE = int.MinValue;

        // Game Entity
        public const int BASE_GAME_ENTITY = 2;
        public const int CHARACTER_MODEL_MANAGER = 100;
        public const int GAME_ENTITY_MODEL = 101;

        // Controller
        public const int PLAYER_CHARACTER_CONTROLLER = 1;

        // IK
        public const int CHARACTER_ALIGN_ON_GROUND = -105;
        public const int PITCH_IK = -104;

        // UIs
        public const int UI_CRAFTING_QUEUE_ITEMS = 100;
        public const int UI_ITEM_CRAFT_FORMULAS = 101;
    }
}
