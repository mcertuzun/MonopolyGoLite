using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public partial class Main : MonoBehaviour
    {
        public GameConfig config;
        private float accum;
        private Transform boardRoot;
        private Camera cam;
        private TextMeshPro diceATxt;
        private TextMeshPro diceBTxt;
        private int diceCharges, diceChargeCap;
        private float diceChargeTimer, diceChargeInterval;
        private Transform diceRoot;
        private int gainMultiplier;
        private int lastD1, lastD2;
        private Vector3[] localTilePositions;
        private Transform multBtn;
        private TextMeshPro multBtnLabel;
        private int playerCount = 1, currentPlayer = 0;
        private int[] pos, cash, jailTurns, doublesInRow, tileOwner;
        private RNG rng;
        private Transform rollBtn;
        private TextMeshPro rollBtnLabel;
        private SpriteRenderer rollBtnSR;
        private TextMeshPro[] statsLines;
        private Transform statsRoot;
        private TextMeshPro[] tileLabels;
        private SpriteRenderer[] tileSprites;
        private TextMeshPro[] tokenLabels;
        private SpriteRenderer[] tokenSprites;
        private Vector3[] worldTilePositions;

        private int[] developmentLevels;
        private List<int> chanceDeckOrder, communityChestDeckOrder;
        private int chanceDrawIndex, communityChestDrawIndex;
        private HashSet<int> declinedProperties;
        private bool gameOver;
        private bool playerWon;
        private int totalTurns;
        private int totalMoneyEarned;
        private int pendingBuyTileIndex = -1;
        private float cardRevealTimer;
        private string lastCardDescription = "";

        private void Awake()
        {
            Application.targetFrameRate = 60;
            config = ConfigLoader.LoadOrDefault();
            cam = new GameObject("MainCamera").AddComponent<Camera>();
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.orthographic = true;
            FitCamera();
            InitState();
            BuildBoard();
            BuildTokens();
            BuildUI();
        }

        private void Update()
        {
            accum += Time.deltaTime;
            float step = 1f / Mathf.Max(1, config.ticksPerSecond);
            while (accum >= step)
            {
                accum -= step;
                TickSystems();
            }

            UpdateUIPositions();
            UpdateTokens();
            UpdateStats();
        }

        private void FitCamera()
        {
            float aspect = (float)config.targetWidth / Mathf.Max(1f, config.targetHeight);
            float half = config.sideLength * 0.5f + config.cameraMargin;
            float sizeByHeight = half, sizeByWidth = half / aspect;
            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
            cam.transform.position = new Vector3(0, 0, -10);
            cam.transform.rotation = Quaternion.identity;
        }

        private void InitState()
        {
            rng = new RNG(config.seed);
            playerCount = 1;
            currentPlayer = 0;
            pos = new int[playerCount];
            cash = new int[playerCount];
            jailTurns = new int[playerCount];
            doublesInRow = new int[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                pos[i] = 0;
                cash[i] = config.startingCash;
                jailTurns[i] = 0;
                doublesInRow[i] = 0;
            }

            tileOwner = new int[config.tiles.Length];
            for (int i = 0; i < tileOwner.Length; i++) tileOwner[i] = -1;
            lastD1 = 0;
            lastD2 = 0;
            diceCharges = config.initialCharges;
            diceChargeCap = config.chargeCap;
            diceChargeTimer = 0f;
            diceChargeInterval = config.chargeInterval;
            gainMultiplier = 1;

            developmentLevels = new int[config.tiles.Length];
            declinedProperties = new HashSet<int>();
            gameOver = false;
            playerWon = false;
            totalTurns = 0;
            totalMoneyEarned = 0;
            pendingBuyTileIndex = -1;
            cardRevealTimer = 0f;
            lastCardDescription = "";

            chanceDeckOrder = ShuffleDeck(config.chanceCards != null ? config.chanceCards.Length : 0);
            communityChestDeckOrder = ShuffleDeck(config.communityChestCards != null ? config.communityChestCards.Length : 0);
            chanceDrawIndex = 0;
            communityChestDrawIndex = 0;
        }

        private List<int> ShuffleDeck(int count)
        {
            List<int> deck = new(count);
            for (int i = 0; i < count; i++) deck.Add(i);
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            return deck;
        }

        private void BuildBoard()
        {
            if (boardRoot != null) DestroyImmediate(boardRoot.gameObject);
            boardRoot = new GameObject("Board").transform;
            Vector3[] posArr = Layout.Perimeter(config.tiles.Length, config.sideLength, config.tileSize, 0.1f);
            localTilePositions = posArr;
            worldTilePositions = new Vector3[posArr.Length];
            for (int i = 0; i < posArr.Length; i++) worldTilePositions[i] = boardRoot.TransformPoint(posArr[i]);
            tileSprites = new SpriteRenderer[config.tiles.Length];
            tileLabels = new TextMeshPro[config.tiles.Length];
            for (int i = 0; i < config.tiles.Length; i++)
            {
                TileDef t = config.tiles[i];
                GameObject go = new($"Tile_{i}_{t.name}");
                go.transform.SetParent(boardRoot, false);
                go.transform.localPosition = posArr[i];
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprites.Square(64, ColorForTile(t));
                go.transform.localScale = new Vector3(config.tileSize, config.tileSize, 1);
                GameObject lgo = new("Label");
                lgo.transform.SetParent(go.transform, false);
                lgo.transform.localPosition = new Vector3(0, 1.1f, 0);
                TextMeshPro tmp = lgo.AddComponent<TextMeshPro>();
                tmp.text = t.name;
                tmp.fontSize = 2.5f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.rectTransform.sizeDelta = new Vector2(6, 1.5f);
                tileSprites[i] = sr;
                tileLabels[i] = tmp;
            }
        }

        private void BuildTokens()
        {
            tokenSprites = new SpriteRenderer[playerCount];
            tokenLabels = new TextMeshPro[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                GameObject go = new("Token_" + i);
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprites.Circle(64, i == 0 ? config.tokenA : config.tokenB);
                go.transform.localScale = Vector3.one * 0.7f;
                TextMeshPro l = new GameObject("Label").AddComponent<TextMeshPro>();
                l.transform.SetParent(go.transform, false);
                l.transform.localPosition = new Vector3(0, 0.9f, 0);
                l.text = "P" + i;
                l.fontSize = 2.5f;
                l.alignment = TextAlignmentOptions.Center;
                l.rectTransform.sizeDelta = new Vector2(3, 1);
                tokenSprites[i] = sr;
                tokenLabels[i] = l;
                go.transform.position = worldTilePositions[0];
            }
        }

        private void BuildUI()
        {
            rollBtn = new GameObject("RollButton").transform;
            rollBtn.SetParent(transform, false);
            rollBtnSR = rollBtn.gameObject.AddComponent<SpriteRenderer>();
            rollBtnSR.sprite = Sprites.Circle(128, config.rollColor);
            rollBtn.localScale = Vector3.one * 1.6f;
            TextMeshPro lbl = new GameObject("Label").AddComponent<TextMeshPro>();
            lbl.transform.SetParent(rollBtn, false);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 4;
            rollBtnLabel = lbl;
            multBtn = new GameObject("MultiplierButton").transform;
            multBtn.SetParent(transform, false);
            SpriteRenderer mSR = multBtn.gameObject.AddComponent<SpriteRenderer>();
            mSR.sprite = Sprites.Square(96, config.multColor);
            multBtn.localScale = Vector3.one * 0.9f;
            TextMeshPro ml = new GameObject("Label").AddComponent<TextMeshPro>();
            ml.transform.SetParent(multBtn, false);
            ml.alignment = TextAlignmentOptions.Center;
            ml.fontSize = 3.5f;
            multBtnLabel = ml;
            diceRoot = new GameObject("Dice").transform;
            diceRoot.SetParent(transform, false);
            GameObject a = new("A");
            GameObject b = new("B");
            a.transform.SetParent(diceRoot, false);
            b.transform.SetParent(diceRoot, false);
            SpriteRenderer aSR = a.AddComponent<SpriteRenderer>();
            aSR.sprite = Sprites.Square(64, Color.white);
            SpriteRenderer bSR = b.AddComponent<SpriteRenderer>();
            bSR.sprite = Sprites.Square(64, Color.white);
            a.transform.localScale = b.transform.localScale = Vector3.one * 1.2f;
            TextMeshPro aT = new GameObject("AT").AddComponent<TextMeshPro>();
            aT.transform.SetParent(a.transform, false);
            aT.alignment = TextAlignmentOptions.Center;
            aT.fontSize = 4;
            TextMeshPro bT = new GameObject("BT").AddComponent<TextMeshPro>();
            bT.transform.SetParent(b.transform, false);
            bT.alignment = TextAlignmentOptions.Center;
            bT.fontSize = 4;
            diceATxt = aT;
            diceATxt.color = Color.black;
            diceBTxt = bT;
            diceBTxt.color = Color.black;
            UpdateDiceLabels(0, 0);
            statsRoot = new GameObject("Stats").transform;
            statsRoot.SetParent(transform, false);
            statsLines = new TextMeshPro[2 + playerCount];
            for (int i = 0; i < statsLines.Length; i++)
            {
                TextMeshPro t = new GameObject("S" + i).AddComponent<TextMeshPro>();
                t.transform.SetParent(statsRoot, false);
                t.fontSize = 3;
                t.alignment = TextAlignmentOptions.Left;
                statsLines[i] = t;
            }
        }
    }
}