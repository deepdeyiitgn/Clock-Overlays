using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TransparentClock
{
    public static class TodoStorage
    {
        private static readonly string AppFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays");

        private static readonly string TodoFilePath = Path.Combine(AppFolderPath, "todos.json");

        public static List<TodoItem> Load()
        {
            try
            {
                if (!File.Exists(TodoFilePath))
                {
                    return new List<TodoItem>();
                }

                string json = File.ReadAllText(TodoFilePath);
                var items = JsonSerializer.Deserialize<List<TodoItem>>(json);
                return items ?? new List<TodoItem>();
            }
            catch
            {
                return new List<TodoItem>();
            }
        }

        public static void Save(List<TodoItem> items)
        {
            try
            {
                Directory.CreateDirectory(AppFolderPath);
                string json = JsonSerializer.Serialize(items, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(TodoFilePath, json);
            }
            catch
            {
            }
        }
    }
}
