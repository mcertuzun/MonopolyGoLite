using System;
using System.Collections.Generic;

namespace MonopolyLite
{
    public class Recorder
    {
        public enum EventType
        {
            Command,
            Checksum
        }

        private List<Event> eventsList = new();
        public uint MaxFrame { get; private set; }

        public void Reset()
        {
            eventsList.Clear();
            MaxFrame = 0;
        }

        public void Record(uint frame, Command cmd)
        {
            eventsList.Add(new Event
            { type = EventType.Command, frame = frame, command = cmd });
            if (frame > MaxFrame) MaxFrame = frame;
        }

        public void RecordChecksum(uint frame, ulong hash)
        {
            eventsList.Add(new Event
            { type = EventType.Checksum, frame = frame, checksum = hash });
            if (frame > MaxFrame) MaxFrame = frame;
        }

        public bool TryGetAt(uint index, out Event e)
        {
            if (index < eventsList.Count)
            {
                e = eventsList[(int)index];
                return true;
            }

            e = default;
            return false;
        }

        [Serializable]
        public struct Event
        {
            public EventType type;
            public uint frame;
            public Command command;
            public ulong checksum;
        }
    }
}