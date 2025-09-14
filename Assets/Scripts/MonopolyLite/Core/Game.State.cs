using System;
using UnityEngine;

namespace MonopolyLite
{
    public partial class MonopolyLiteGame
    {
        [Serializable]
        public struct State
        {
            public uint frame;
            public uint initialSeed;
            public BoardConfig boardConfig;
            public LiveOpsConfig liveOps;
            public RNG rng;
            public const int MaxPlayers = 4;
            public int playerCount;
            public int currentPlayer;
            public int[] pos;
            public int[] cash;
            public int[] jailTurns;
            public int[] doublesInRow;
            public int[] tileOwner;
            public int goPayout;
            public int lastD1;
            public int lastD2;
            public int diceCharges;
            public int diceChargeCap;
            public float diceChargeTimer;
            public float diceChargeInterval;
            public int gainMultiplier;

            public void Init(BoardConfig board, LiveOpsConfig liveOpsConfig, uint seed)
            {
                frame = 0;
                initialSeed = seed;
                rng = new RNG(seed);
                boardConfig = board;
                liveOps = liveOpsConfig;
                playerCount = Mathf.Clamp(liveOps.playerCount, 2, MaxPlayers);
                currentPlayer = 0;
                pos = new int[playerCount];
                cash = new int[playerCount];
                jailTurns = new int[playerCount];
                doublesInRow = new int[playerCount];
                for (int i = 0; i < playerCount; i++)
                {
                    pos[i] = 0;
                    cash[i] = board.startingCash;
                    jailTurns[i] = 0;
                    doublesInRow[i] = 0;
                }

                tileOwner = new int[board.tiles.Length];
                for (int t = 0; t < tileOwner.Length; t++) tileOwner[t] = -1;
                goPayout = board.goPayout;
                lastD1 = 0;
                lastD2 = 0;
                diceCharges = 3;
                diceChargeCap = 20;
                diceChargeTimer = 0f;
                diceChargeInterval = 3f;
                gainMultiplier = 1;
            }
        }
    }
}