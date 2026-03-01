using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CommandPalette.Core
{
    public class UsageTracker
    {
        private readonly string _filePath;
        private Dictionary<string, int> _counts;

        public UsageTracker()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _filePath = Path.Combine(dir, "usage.json");
            Load();
        }

        public void Track(string commandName)
        {
            if (!_counts.ContainsKey(commandName))
                _counts[commandName] = 0;
            _counts[commandName]++;
            Save();
        }

        public int GetCount(string commandName)
        {
            return _counts.TryGetValue(commandName, out int count) ? count : 0;
        }

        private void Load()
        {
            _counts = new Dictionary<string, int>();

            if (!File.Exists(_filePath)) return;

            string json = File.ReadAllText(_filePath);
            // Парсим вручную: { "name": 5, "name2": 3 }
            json = json.Trim().Trim('{', '}');
            foreach (string line in json.Split(','))
            {
                string[] parts = line.Split(':');
                if (parts.Length != 2) continue;
                string key = parts[0].Trim().Trim('"');
                if (int.TryParse(parts[1].Trim(), out int val))
                    _counts[key] = val;
            }
        }

        private void Save()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            var entries = new List<string>();
            foreach (var kv in _counts)
                entries.Add($"  \"{kv.Key.Replace("\"", "\\\"")}\": {kv.Value}");
            sb.Append(string.Join(",\n", entries));
            sb.AppendLine("\n}");
            File.WriteAllText(_filePath, sb.ToString());
        }
    }
}