﻿using SolidShineUi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PathfinderJson
{
    /// <summary>
    /// Interaction logic for AbilityScoreIconEditor.xaml
    /// </summary>
    public partial class AbilityScoreIconEditor : UserControl
    {
        public AbilityScoreIconEditor()
        {
            InitializeComponent();
        }

        #region Dependency Properties
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public static DependencyProperty TitleProperty
            = DependencyProperty.Register("Title", typeof(string), typeof(AbilityScoreIconEditor));

        public string Abbreviation { get => (string)GetValue(AbbreviationProperty); set => SetValue(AbbreviationProperty, value); }

        public static DependencyProperty AbbreviationProperty
            = DependencyProperty.Register("Abbreviation", typeof(string), typeof(AbilityScoreIconEditor));

        public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        public static DependencyProperty ValueProperty
            = DependencyProperty.Register("Value", typeof(int), typeof(AbilityScoreIconEditor),
            new PropertyMetadata(10, new PropertyChangedCallback(OnValueChanged)));

        public int Modifier { get => (int)GetValue(ModifierProperty); private set => SetValue(ModifierPropertyKey, value); }

        private static DependencyPropertyKey ModifierPropertyKey
            = DependencyProperty.RegisterReadOnly("Modifier", typeof(int), typeof(AbilityScoreIconEditor),
            new PropertyMetadata(0));

        public static DependencyProperty ModifierProperty = ModifierPropertyKey!.DependencyProperty;

        public ColorScheme ColorScheme { get => (ColorScheme)GetValue(ColorSchemeProperty); set => SetValue(ColorSchemeProperty, value); }

        public static DependencyProperty ColorSchemeProperty
            = DependencyProperty.Register("ColorScheme", typeof(ColorScheme), typeof(AbilityScoreIconEditor),
                new PropertyMetadata(new ColorScheme()));

        protected static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is AbilityScoreIconEditor editor)
            {
                editor.OnInternalValueChanged(e);
                editor.ValueChanged?.Invoke(sender, e);
            }
        }

        public event DependencyPropertyChangedEventHandler? ValueChanged;

        protected void OnInternalValueChanged(DependencyPropertyChangedEventArgs e)
        {
            Modifier = CoreUtils.CalculateModifierInt(Value);
        }

        #endregion

        private void btnTempAbi_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}