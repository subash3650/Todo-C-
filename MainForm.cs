using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TodoApp
{
    public class MainForm : Form
    {
        // UI Controls
        private readonly TextBox input;
        private readonly Button addBtn;
        private readonly Button removeBtn;
        private readonly Button toggleBtn;
        private readonly Button saveBtn;
        private readonly ListBox listBox;
        private readonly Label statusLabel;

        // Data
        private readonly BindingList<TodoItem> todos = new BindingList<TodoItem>();

        // Persistence path
        private readonly string storagePath;

        public MainForm()
        {
            // Setup window
            Text = "TodoApp — Subash";
            Width = 420;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;

            // Controls
            input = new TextBox { Left = 10, Top = 10, Width = 280 };
            addBtn = new Button { Text = "Add", Left = 300, Top = 8, Width = 90 };
            listBox = new ListBox { Left = 10, Top = 40, Width = 380, Height = 360 };
            toggleBtn = new Button { Text = "Toggle Done", Left = 10, Top = 410, Width = 120 };
            removeBtn = new Button { Text = "Remove Selected", Left = 140, Top = 410, Width = 120 };
            saveBtn = new Button { Text = "Save Now", Left = 270, Top = 410, Width = 120 };
            statusLabel = new Label { Left = 10, Top = 450, Width = 380, Height = 40, AutoSize = false };

            Controls.Add(input);
            Controls.Add(addBtn);
            Controls.Add(listBox);
            Controls.Add(toggleBtn);
            Controls.Add(removeBtn);
            Controls.Add(saveBtn);
            Controls.Add(statusLabel);

            // Data binding
            listBox.DataSource = todos;
            listBox.DisplayMember = "Display";

            // Events
            addBtn.Click += AddBtn_Click;
            removeBtn.Click += RemoveBtn_Click;
            toggleBtn.Click += ToggleBtn_Click;
            saveBtn.Click += SaveBtn_Click;
            listBox.DoubleClick += ListBox_DoubleClick;
            input.KeyDown += Input_KeyDown;

            // persistence file inside %AppData%\TodoApp\todos.json
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "TodoApp");
            Directory.CreateDirectory(folder);
            storagePath = Path.Combine(folder, "todos.json");

            // Load saved todos if any
            LoadTodos();
            UpdateStatus();
        }

        // Add on Add button click
        private void AddBtn_Click(object? sender, EventArgs e)
        {
            var text = input.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                FlashStatus("Type something to add a todo.", true);
                return;
            }

            todos.Add(new TodoItem(text));
            input.Clear();
            input.Focus();

            // Refresh the UI display string
            RefreshListBindings();
            UpdateStatus();

            // optional: autosave each change
            SaveTodos();
        }

        // Remove selected
        private void RemoveBtn_Click(object? sender, EventArgs e)
        {
            var idx = listBox.SelectedIndex;
            if (idx >= 0 && idx < todos.Count)
            {
                todos.RemoveAt(idx);
                RefreshListBindings();
                UpdateStatus();
                SaveTodos();
            }
            else FlashStatus("Select an item to remove.", true);
        }

        // Toggle done state
        private void ToggleBtn_Click(object? sender, EventArgs e) => ToggleSelected();

        // Double click toggles
        private void ListBox_DoubleClick(object? sender, EventArgs e) => ToggleSelected();

        // Enter key in input adds
        private void Input_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddBtn_Click(sender, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ToggleSelected()
        {
            var idx = listBox.SelectedIndex;
            if (idx >= 0 && idx < todos.Count)
            {
                todos[idx].Toggle();
                RefreshListBindings();
                SaveTodos();
                UpdateStatus();
            }
            else FlashStatus("Select an item to toggle.", true);
        }

        // Manual save button
        private void SaveBtn_Click(object? sender, EventArgs e)
        {
            SaveTodos();
            FlashStatus("Todos saved.");
        }

        // Save to JSON
        private void SaveTodos()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(todos, options);
                File.WriteAllText(storagePath, json);
            }
            catch (Exception ex)
            {
                FlashStatus($"Save failed: {ex.Message}", true);
            }
        }

        // Load from JSON
        private void LoadTodos()
        {
            try
            {
                if (!File.Exists(storagePath)) return;
                var json = File.ReadAllText(storagePath);
                var list = JsonSerializer.Deserialize<BindingList<TodoItem>>(json);
                if (list != null)
                {
                    todos.Clear();
                    foreach (var t in list) todos.Add(t);
                    RefreshListBindings();
                }
            }
            catch (Exception ex)
            {
                FlashStatus($"Load failed: {ex.Message}", true);
            }
        }

        // Because Display is derived, refresh the list UI
        private void RefreshListBindings()
        {
            // RefreshListBindings(false) redraws DisplayMember strings
            listBox.DataSource = null;
            listBox.DataSource = todos;
            listBox.DisplayMember = "Display";
        }

        // Update tiny status bar
        private void UpdateStatus()
        {
            var total = todos.Count;
            var done = 0;
            foreach (var t in todos) if (t.Done) done++;
            statusLabel.Text = $"Total: {total}    Done: {done}";
        }

        // Small helper to show a transient status (could be improved)
        private async void FlashStatus(string text, bool isError = false)
        {
            statusLabel.Text = text;
            statusLabel.ForeColor = isError ? System.Drawing.Color.DarkRed : System.Drawing.Color.Black;
            await System.Threading.Tasks.Task.Delay(1200);
            UpdateStatus();
            statusLabel.ForeColor = System.Drawing.Color.Black;
        }
    }
}
