using CommandPalette.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CommandPalette.UI
{
    public partial class PaletteWindow : Window
    {
        private readonly List<CommandItem> _allCommands;
        private readonly UsageTracker _tracker;
        private List<CommandItem> _current = new List<CommandItem>();
        private bool _closing = false;

        public PaletteWindow(List<CommandItem> commands, UsageTracker tracker)
        {
            InitializeComponent();
            _allCommands = commands;
            _tracker = tracker;

            Loaded += (s, e) =>
            {
                SearchBox.Focus();
                // Клик мимо — через Win32 мониторинг мыши
                this.MouseDown += (ms, me) => { };
            };

            // Клик мимо окна
            Deactivated += (s, e) => SafeClose();

            ShowTop5();
        }

        private void SafeClose()
        {
            if (_closing) return;
            _closing = true;
            Deactivated -= (s, e) => SafeClose();
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => { try { Close(); } catch { } }));
        }

        private void ShowTop5()
        {
            SectionLabel.Text = "НЕДАВНИЕ";
            _current = _allCommands
                .OrderByDescending(c => _tracker.GetCount(c.Name))
                .Take(5)
                .ToList();
            Refresh();
        }

        private void ShowFiltered(string query)
        {
            SectionLabel.Text = "РЕЗУЛЬТАТЫ";
            string q = query.ToLower();
            _current = _allCommands
                .Where(c => FuzzyMatch(c.SearchText, q))
                .OrderByDescending(c => Score(c, q))
                .ToList();
            Refresh();
        }

        private void Refresh()
        {
            CommandList.ItemsSource = null;
            CommandList.ItemsSource = _current;
            if (_current.Any())
                CommandList.SelectedIndex = 0;
        }

        private bool FuzzyMatch(string text, string query)
        {
            int qi = 0;
            foreach (char c in text)
                if (qi < query.Length && c == query[qi]) qi++;
            return qi == query.Length;
        }

        private int Score(CommandItem item, string query)
        {
            int score = 0;
            string name = item.Name.ToLower();
            if (name.StartsWith(query)) score += 100;
            if (name.Contains(query)) score += 50;
            return score;
        }

        private void ExecuteSelected()
        {
            if (CommandList.SelectedItem is CommandItem item)
            {
                _tracker.Track(item.Name);
                CommandExecutor.Instance.RequestExecute(item);
                SafeClose();
            }
        }

        private void SearchBox_TextChanged(object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            string text = SearchBox.Text;
            Placeholder.Visibility = string.IsNullOrEmpty(text)
                ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(text)) ShowTop5();
            else ShowFiltered(text);
        }

        private void CommandList_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Пробрасываем событие в родительский ScrollViewer
            if (!e.Handled)
            {
                e.Handled = true;
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(
                    (System.Windows.DependencyObject)sender);
                while (parent != null && !(parent is ScrollViewer))
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);

                if (parent is ScrollViewer scrollViewer)
                {
                    scrollViewer.RaiseEvent(new System.Windows.Input.MouseWheelEventArgs(
                        e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    });
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (_current.Any())
                    {
                        if (CommandList.SelectedIndex < _current.Count - 1)
                            CommandList.SelectedIndex++;
                        else
                            CommandList.SelectedIndex = 0; // Зацикливание
                        EnsureSelectedVisible();
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (_current.Any())
                    {
                        if (CommandList.SelectedIndex > 0)
                            CommandList.SelectedIndex--;
                        else
                            CommandList.SelectedIndex = _current.Count - 1; // Зацикливание
                        EnsureSelectedVisible();
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    e.Handled = true;
                    ExecuteSelected();
                    break;

                case Key.Space:
                    // Пробел выполняет команду только если поле поиска пустое
                    // или если нажат Ctrl+Space
                    if (string.IsNullOrEmpty(SearchBox.Text) 
                        || (Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    {
                        e.Handled = true;
                        ExecuteSelected();
                    }
                    break;

                case Key.Escape:
                    e.Handled = true;
                    SafeClose();
                    break;
            }
        }

        private void EnsureSelectedVisible()
        {
            if (CommandList.SelectedItem != null)
            {
                CommandList.ScrollIntoView(CommandList.SelectedItem);
                CommandList.UpdateLayout();
            }
        }

        private void CommandList_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        { }

        private void CommandList_MouseLeftButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (CommandList.SelectedItem != null)
                ExecuteSelected();
        }

        private void CommandList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                e.Handled = true;
                ExecuteSelected();
            }
        }
    }
}