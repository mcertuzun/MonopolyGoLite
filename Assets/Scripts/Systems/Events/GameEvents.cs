using MonopolyLite.Data;
using MonopolyLite.Logic;
using MonopolyLite.State;

namespace MonopolyLite.Events
{
    public struct RollEvent { public int Die1; public int Die2; public int Total; public bool IsDoubles; public bool PassedGo; }
    public struct TileEvent { public TileResolveType Type; public int Amount; public CardDef? DrawnCard; }
    public struct LandmarkUpgradedEvent { public ColorGroup Group; public int NewLevel; }
    public struct BoardCompleteEvent { }
    public struct BoardTransitionEvent { public string NewBoardId; public string Theme; }
    public struct CoinChangeEvent { public int Amount; public bool IsGain; public string Source; }
    public struct DiceRegenEvent { public int Amount; }
    public struct MilestoneEvent { public int MilestoneIndex; }
    public struct DailyRewardEvent { public int Day; public int Coins; public int Dice; }
    public struct HeistEvent { public HeistResult Result; public string TargetName; }
    public struct ShutdownStartEvent { public TargetProfile Target; }
    public struct ShutdownResolveEvent { public ShutdownResult Result; }
    public struct MissionCompleteEvent { public string Description; public int CoinReward; public int DiceReward; }
    public struct AllMissionsCompleteEvent { }
    public struct StickerGrantEvent { public int StickerId; public string StickerName; }
    public struct GameSavedEvent { }
    public struct GameLoadedEvent { }
}
