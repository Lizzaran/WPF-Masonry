#region License

/*
 Copyright 2013 - 2016 Nikita Bernthaler
 TextBoxMaskBehavior.cs is part of Masonry.Example.

 Masonry.Example is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 Masonry.Example is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with Masonry.Example. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

namespace Masonry.Example.Common
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class TextBoxMaskBehavior
    {
        #region Static Fields

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(TextBoxMaskBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback));

        public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached(
            "Mask",
            typeof(MaskType),
            typeof(TextBoxMaskBehavior),
            new FrameworkPropertyMetadata(MaskChangedCallback));

        #endregion

        #region Public Methods and Operators

        public static MaskType GetMask(DependencyObject obj)
        {
            return (MaskType)obj.GetValue(MaskProperty);
        }

        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        public static void SetMask(DependencyObject obj, MaskType value)
        {
            obj.SetValue(MaskProperty, value);
        }

        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        #endregion

        #region Methods

        private static bool IsSymbolValid(MaskType mask, string str)
        {
            switch (mask)
            {
                case MaskType.Any:
                    return true;

                case MaskType.Integer:
                    if (str == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        return true;
                    }
                    break;

                case MaskType.Decimal:
                    if (str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator
                        || str == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        return true;
                    }
                    break;
            }

            if (mask.Equals(MaskType.Integer) || mask.Equals(MaskType.Decimal))
            {
                return str.All(char.IsDigit);
            }

            return false;
        }

        private static void MaskChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = e.OldValue as TextBox;
            if (box != null)
            {
                box.PreviewTextInput -= TextBoxPreviewTextInput;
                DataObject.RemovePastingHandler(box, TextBoxPastingEventHandler);
            }

            var _this = d as TextBox;
            if (_this == null)
            {
                return;
            }

            if ((MaskType)e.NewValue != MaskType.Any)
            {
                _this.PreviewTextInput += TextBoxPreviewTextInput;
                DataObject.AddPastingHandler(_this, TextBoxPastingEventHandler);
            }

            ValidateTextBox(_this);
        }

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as TextBox;
            ValidateTextBox(_this);
        }

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as TextBox;
            ValidateTextBox(_this);
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            var _this = sender as TextBox;
            var clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = ValidateValue(GetMask(_this), clipboard, GetMinimumValue(_this), GetMaximumValue(_this));
            if (!string.IsNullOrEmpty(clipboard))
            {
                if (_this != null)
                {
                    _this.Text = clipboard;
                }
            }
            e.CancelCommand();
            e.Handled = true;
        }

        private static void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var _this = sender as TextBox;
            var isValid = IsSymbolValid(GetMask(_this), e.Text);
            e.Handled = !isValid;
            if (isValid)
            {
                if (_this != null)
                {
                    var caret = _this.CaretIndex;
                    var text = _this.Text;
                    var textInserted = false;
                    var selectionLength = 0;

                    if (_this.SelectionLength > 0)
                    {
                        text = text.Substring(0, _this.SelectionStart)
                               + text.Substring(_this.SelectionStart + _this.SelectionLength);
                        caret = _this.SelectionStart;
                    }

                    if (e.Text == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    {
                        while (true)
                        {
                            var ind = text.IndexOf(
                                NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,
                                StringComparison.Ordinal);
                            if (ind == -1)
                            {
                                break;
                            }

                            text = text.Substring(0, ind) + text.Substring(ind + 1);
                            if (caret > ind)
                            {
                                caret--;
                            }
                        }

                        if (caret == 0)
                        {
                            text = "0" + text;
                            caret++;
                        }
                        else
                        {
                            if (caret == 1 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign)
                            {
                                text = NumberFormatInfo.CurrentInfo.NegativeSign + "0" + text.Substring(1);
                                caret++;
                            }
                        }

                        if (caret == text.Length)
                        {
                            selectionLength = 1;
                            textInserted = true;
                            text = text + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "0";
                            caret++;
                        }
                    }
                    else if (e.Text == NumberFormatInfo.CurrentInfo.NegativeSign)
                    {
                        textInserted = true;
                        if (_this.Text.Contains(NumberFormatInfo.CurrentInfo.NegativeSign))
                        {
                            text = text.Replace(NumberFormatInfo.CurrentInfo.NegativeSign, string.Empty);
                            if (caret != 0)
                            {
                                caret--;
                            }
                        }
                        else
                        {
                            text = NumberFormatInfo.CurrentInfo.NegativeSign + _this.Text;
                            caret++;
                        }
                    }

                    if (!textInserted)
                    {
                        text = text.Substring(0, caret) + e.Text
                               + (caret < _this.Text.Length ? text.Substring(caret) : string.Empty);

                        caret++;
                    }

                    try
                    {
                        var val = Convert.ToDouble(text);
                        var newVal = ValidateLimits(GetMinimumValue(_this), GetMaximumValue(_this), val);
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        if (val != newVal)
                        {
                            text = newVal.ToString(CultureInfo.InvariantCulture);
                        }
                        else if (val == 0)
                        {
                            if (!text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                            {
                                text = "0";
                            }
                        }
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                    }
                    catch
                    {
                        text = "0";
                    }

                    while (text.Length > 1 && text[0] == '0'
                           && string.Empty + text[1] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    {
                        text = text.Substring(1);
                        if (caret > 0)
                        {
                            caret--;
                        }
                    }

                    while (text.Length > 2 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign
                           && text[1] == '0'
                           && string.Empty + text[2] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    {
                        text = NumberFormatInfo.CurrentInfo.NegativeSign + text.Substring(2);
                        if (caret > 1)
                        {
                            caret--;
                        }
                    }

                    if (caret > text.Length)
                    {
                        caret = text.Length;
                    }

                    _this.Text = text;
                    _this.CaretIndex = caret;
                    _this.SelectionStart = caret;
                    _this.SelectionLength = selectionLength;
                }
                e.Handled = true;
            }
        }

        private static double ValidateLimits(double min, double max, double value)
        {
            if (!min.Equals(double.NaN))
            {
                if (value < min)
                {
                    return min;
                }
            }

            if (!max.Equals(double.NaN))
            {
                if (value > max)
                {
                    return max;
                }
            }

            return value;
        }

        private static void ValidateTextBox(TextBox _this)
        {
            if (GetMask(_this) != MaskType.Any)
            {
                _this.Text = ValidateValue(GetMask(_this), _this.Text, GetMinimumValue(_this), GetMaximumValue(_this));
            }
        }

        private static string ValidateValue(MaskType mask, string value, double min, double max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            value = value.Trim();

            switch (mask)
            {
                case MaskType.Integer:
                    {
                        int val;
                        if (int.TryParse(value, out val))
                        {
                            val = (int)ValidateLimits(min, max, val);
                            return val.ToString();
                        }

                        return string.Empty;
                    }
                case MaskType.Decimal:
                    {
                        double val;
                        if (double.TryParse(value, out val))
                        {
                            val = ValidateLimits(min, max, val);
                            return val.ToString(CultureInfo.InvariantCulture);
                        }

                        return string.Empty;
                    }
            }

            return value;
        }

        #endregion
    }

    public enum MaskType
    {
        Any,

        Integer,

        Decimal
    }
}