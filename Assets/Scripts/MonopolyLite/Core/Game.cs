using System.Collections.Generic;

namespace MonopolyLite
{
    public partial class MonopolyLiteGame
    {
        private uint checksumInterval = 10;
        private Queue<Command> q = new();
        private Recorder rec = new();
        public State state;
        public uint Frame => state.frame;

        public void Init(BoardConfig board, LiveOpsConfig liveOps, uint seed)
        {
            state = new State();
            state.Init(board, liveOps, seed);
            rec.Reset();
        }

        public void Enqueue(Command cmd)
        {
            q.Enqueue(cmd);
            rec.Record(Frame, cmd);
        }

        public void Tick()
        {
            while (q.Count > 0) ApplyCommand(ref state, q.Dequeue());
            RunSystems(ref state);
            state.frame++;
            if (state.frame % checksumInterval == 0) rec.RecordChecksum(state.frame, Checksum.Hash(ref state));
        }

        public bool ReplayAndVerify(out ulong finalHash, out ulong replayFinalHash)
        {
            State r = new();
            r.Init(state.boardConfig, state.liveOps, state.initialSeed);
            uint max = rec.MaxFrame;
            uint idx = 0;
            for (uint f = 0; f < max; f++)
            {
                while (rec.TryGetAt(idx, out Recorder.Event e) && e.frame == f)
                {
                    if (e.type == Recorder.EventType.Command) ApplyCommand(ref r, e.command);
                    idx++;
                }

                RunSystems(ref r);
                r.frame++;
            }

            finalHash = Checksum.Hash(ref state);
            replayFinalHash = Checksum.Hash(ref r);
            return finalHash == replayFinalHash;
        }
    }
}