using UnityEngine;

namespace MonopolyLite
{
    public sealed class Logger
    {
        public readonly LoggerFormatting Formatting;
        public readonly string Name;

        public Logger(string name, LoggerFormatting formatting)
        {
            Name = name;
            Formatting = formatting;
        }

        public void Info(string m, string caller = "")
        {
            Debug.Log($"<color={Formatting.NameColor}>[{Name}]</color> <color={Formatting.MessageColor}>{m}</color> {(string.IsNullOrEmpty(caller) ? "" : $"<color={Formatting.CallerColor}>({caller})</color>")}");
        }

        public void Warn(string m, string caller = "")
        {
            Debug.LogWarning($"<color={Formatting.NameColor}>[{Name}]</color> <color={Formatting.MessageColor}>{m}</color> {(string.IsNullOrEmpty(caller) ? "" : $"<color={Formatting.CallerColor}>({caller})</color>")}");
        }

        public void Error(string m, string caller = "")
        {
            Debug.LogError($"<color={Formatting.NameColor}>[{Name}]</color> <color={Formatting.MessageColor}>{m}</color> {(string.IsNullOrEmpty(caller) ? "" : $"<color={Formatting.CallerColor}>({caller})</color>")}");
        }

        public readonly struct LoggerFormatting
        {
            public readonly string NameColor;
            public readonly string MessageColor;
            public readonly string CallerColor;

            public LoggerFormatting(string n, string m, string c)
            {
                NameColor = n;
                MessageColor = m;
                CallerColor = c;
            }
        }
    }
}