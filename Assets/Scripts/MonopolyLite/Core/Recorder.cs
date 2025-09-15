using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonopolyLite
{
    public enum CmdType
    {
        Roll,
        ToggleMultiplier
    }

    [Serializable]
    public struct Cmd
    {
        public int tick;
        public CmdType type;
        public int p0, p1, p2;
        public string s0;
    }

    [Serializable]
    public class ReplayData
    {
        public uint seed;
        public GameConfigJson cfg; // istersen kopyasını tut
        public List<Cmd> log = new();
    }

    public class Recorder
    {
        private int cursor = 0;
        public bool Recording { get; private set; }
        public bool Replaying { get; private set; }
        public ReplayData Data { get; private set; } = new();

        public void BeginRecord(uint seed, GameConfig cfg)
        {
            Recording = true;
            Replaying = false;
            cursor = 0;
            Data = new ReplayData
            { seed = seed, cfg = ToJson(cfg) };
            Data.log.Clear();
        }

        public void Add(Cmd cmd)
        {
            if (Recording) Data.log.Add(cmd);
        }

        public void BeginReplay(ReplayData d)
        {
            Recording = false;
            Replaying = true;
            Data = d;
            cursor = 0;
        }

        // tick'te uygulanacak tüm komutları geri döndür
        public IEnumerable<Cmd> CommandsAtTick(int tick)
        {
            if (!Replaying) yield break;
            while (cursor < Data.log.Count && Data.log[cursor].tick == tick)
                yield return Data.log[cursor++];
        }

        public void Save(string file)
        {
            string json = JsonUtility.ToJson(Data, false);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, file), json);
        }

        public ReplayData Load(string file)
        {
            string path = Path.Combine(Application.persistentDataPath, file);
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ReplayData>(json);
        }

        private GameConfigJson ToJson(GameConfig c)
        {
            return new GameConfigJson
            { sideLength = c.sideLength, tileSize = c.tileSize, tiles = c.tiles,
              startingCash = c.startingCash, goPayout = c.goPayout, jailTileIndex = c.jailTileIndex,
              seed = c.seed, ticksPerSecond = c.ticksPerSecond, targetWidth = c.targetWidth, targetHeight = c.targetHeight,
              cameraMargin = c.cameraMargin, initialCharges = c.initialCharges, chargeCap = c.chargeCap, chargeInterval = c.chargeInterval };
        }
    }
}