﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace FikusIn.Utils
{
    public static class FocusBehavior
    {
        public static readonly DependencyProperty IsFocusProperty;

        public static void SetIsFocus(DependencyObject DepObject, bool value)
        {
            DepObject.SetValue(IsFocusProperty, value);
        }

        public static bool GetIsFocus(DependencyObject DepObject)
        {
            return (bool)DepObject.GetValue(IsFocusProperty);
        }

        static FocusBehavior()
        {
            IsFocusProperty = DependencyProperty.RegisterAttached("IsFocus",
                                                                  typeof(bool),
                                                                  typeof(FocusBehavior),
                                                                  new UIPropertyMetadata(false, IsFocusTurn));
        }

        private static void IsFocusTurn(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Control;

            if (element != null)
            {
                if (e.NewValue is bool && (bool)e.NewValue == true)
                {
                    element.Loaded += ElementLoaded;
                }
            }
        }

        private static void ElementLoaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;

            if (control != null)
            {
                if (control is TextBox)
                {
                    Keyboard.Focus(control);
                }
                else
                {
                    control.Focus();
                }
            }
        }
    }
}
