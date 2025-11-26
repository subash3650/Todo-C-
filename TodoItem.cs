using System;
using System.Text.Json.Serialization;

namespace TodoApp
{
    public class TodoItem
    {
        public string Text { get; set; } = string.Empty;
        public bool Done { get; set; } = false;

        // Derived display property used by the UI. Not serialized as a separate field.
        [JsonIgnore]
        public string Display => Done ? $"[x] {Text}" : $"[ ] {Text}";

        public TodoItem() { }

        public TodoItem(string text)
        {
            Text = text;
        }

        public void Toggle() => Done = !Done;
    }
}
