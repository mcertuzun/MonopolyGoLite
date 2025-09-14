using UnityEngine;

namespace MonopolyLite
{
    public class SingleBootstrap : MonoBehaviour
    {
        public uint seed = 12345;
        public int ticksPerSecond = Deterministic.FixedHz;
        public float boardSide = 12f;
        public float tileSize = 1.8f;

        private void Awake()
        {
            BoardConfig boardCfg = JsonLoader.LoadBoardFromJson("board");
            LiveOpsConfig liveOps = JsonLoader.LoadLiveOpsFromJson("liveops");

            GameObject root = new("MonopolyLiteRoot");
            root.transform.SetParent(transform, false);

            GameObject camGO = new("MainCamera");
            camGO.transform.SetParent(root.transform, false);
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0, 0, -10);
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);

            GameObject boardGO = new("BoardView");
            boardGO.transform.SetParent(root.transform, false);
            BoardView boardView = boardGO.AddComponent<BoardView>();
            boardView.Init(boardCfg);
            boardView.sideLength = boardSide;
            boardView.tileSize = tileSize;

            GameObject drvGO = new("GameDriver");
            drvGO.transform.SetParent(root.transform, false);

            GameDriver driver = drvGO.AddComponent<GameDriver>();
            driver.Init(boardCfg, liveOps);
            driver.seed = seed;
            driver.ticksPerSecond = ticksPerSecond;
            driver.mode = GameMode.Player;

            GameObject visGO = new("GameVisuals");
            visGO.transform.SetParent(root.transform, false);
            GameVisualDriver vis = visGO.AddComponent<GameVisualDriver>();
            vis.boardView = boardView;
            vis.driver = driver;

            CameraPortraitFit camFit = camGO.AddComponent<CameraPortraitFit>();
            camFit.boardView = boardView;
            camFit.targetWidth = 1080;
            camFit.targetHeight = 1920;
            camFit.marginWorld = 1.0f;

            GameObject rollGO = new("RollButton");
            rollGO.transform.SetParent(root.transform, false);
            RollButtonView roll = rollGO.AddComponent<RollButtonView>();
            roll.driver = driver;

            GameObject multGO = new("MultiplierButton");
            multGO.transform.SetParent(root.transform, false);
            MultiplierButtonView mult = multGO.AddComponent<MultiplierButtonView>();
            mult.driver = driver;
            mult.anchor = roll;

            GameObject statsGO = new("StatsView");
            statsGO.transform.SetParent(root.transform, false);
            statsGO.AddComponent<StatsView>().driver = driver;

            GameObject diceGO = new("DiceView");
            diceGO.transform.SetParent(root.transform, false);
            diceGO.AddComponent<DiceView>().driver = driver;
        }
    }
}