using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
namespace TestingUI
{
    public partial class MainWindow : Window
    {
        private Calculator _calculator;
        private bool _isExpanded = false;

        public MainWindow()
        {
            InitializeComponent();
            _calculator = new Calculator(UpdateDisplay, UpdateExpression);
            AttachButtonEvents();
            this.KeyDown += Window_KeyDown;
        }

        private void AttachButtonEvents()
        {
            foreach (UIElement el in ButtonGrid.Children)
            {
                if (el is Button button && button.Name != "BackspaceButton")
                {
                    button.Click += Button_Click;
                }
            }
        }

        private void UpdateDisplay(string value)
        {
            TextSize.Text = value;
        }

        private void UpdateExpression(string value)
        {
           TextLabel.Text = value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string content = button.Name;
                if (button.Content is string contentStr)
                {
                    _calculator.ProcessInput(contentStr);
                }

            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            _isExpanded = !_isExpanded;
            ExtraColumn.Width = _isExpanded ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            MenuIcon.Kind = _isExpanded ? MaterialDesignThemes.Wpf.PackIconKind.ChevronLeft : MaterialDesignThemes.Wpf.PackIconKind.HamburgerMenu;

            foreach (UIElement element in ButtonGrid.Children)
            {
                if (Grid.GetColumn(element) == 4)
                    element.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0: case Key.NumPad0: _calculator.ProcessInput("0"); break;
                case Key.D1: case Key.NumPad1: _calculator.ProcessInput("1"); break;
                case Key.D2: case Key.NumPad2: _calculator.ProcessInput("2"); break;
                case Key.D3: case Key.NumPad3: _calculator.ProcessInput("3"); break;
                case Key.D4: case Key.NumPad4: _calculator.ProcessInput("4"); break;
                case Key.D5: case Key.NumPad5: _calculator.ProcessInput("5"); break;
                case Key.D6: case Key.NumPad6: _calculator.ProcessInput("6"); break;
                case Key.D7: case Key.NumPad7: _calculator.ProcessInput("7"); break;
                case Key.D8: case Key.NumPad8: _calculator.ProcessInput("8"); break;
                case Key.D9: case Key.NumPad9: _calculator.ProcessInput("9"); break;
                case Key.OemPeriod: case Key.Decimal: _calculator.ProcessInput("."); break;
                case Key.Add: case Key.OemPlus: _calculator.ProcessInput("+"); break;
                case Key.Subtract: case Key.OemMinus: _calculator.ProcessInput("-"); break;
                case Key.Multiply: _calculator.ProcessInput("*"); break;
                case Key.Divide: case Key.Oem2: _calculator.ProcessInput("/"); break;
                case Key.Enter: _calculator.ProcessInput("="); break;
                case Key.Back: _calculator.ProcessInput("Backspace"); break;
                case Key.Delete: _calculator.ProcessInput("C"); break;
                case Key.Escape: _calculator.ProcessInput("CE"); break;
                case Key.Z when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
                    _calculator.Undo_Click(null, null);
                    break;
                case Key.P: _calculator.ProcessInput("π"); break;
                case Key.E: _calculator.ProcessInput("e"); break;
                case Key.L: _calculator.ProcessInput("ln"); break;
                case Key.OemOpenBrackets: _calculator.ProcessInput("("); break;
                case Key.OemCloseBrackets: _calculator.ProcessInput(")"); break;
            }
        }
        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            _calculator.ProcessInput("Backspace");
        }
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _calculator.Undo_Click(sender, e);
        }
    }
   
}
