﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SolidShineUi;
using static PathfinderJson.CoreUtils;

namespace PathfinderJson
{
    /// <summary>
    /// Interaction logic for AcItemEditor.xaml
    /// </summary>
    public partial class AcItemEditor : SelectableUserControl
    {
        public AcItemEditor()
        {
            InitializeComponent();
        }

        public void LoadAcItem(AcItem a)
        {
            txtBonus.Text = a.Bonus;
            txtPenalty.Text = a.ArmorCheckPenalty;
            txtSpellFailure.Text = a.SpellFailure;
            txtWeight.ValueString = a.Weight ?? "";
            txtName.Text = a.Name;
            txtNotes.Text = a.Properties;
            txtType.Text = a.Type;
        }

        public AcItem GetAcItem()
        {
            AcItem a = new AcItem()
            {
                Bonus = GetStringOrNull(txtBonus.Text),
                ArmorCheckPenalty = GetStringOrNull(txtPenalty.Text),
                SpellFailure = GetStringOrNull(txtSpellFailure.Text),
                Name = GetStringOrNull(txtName.Text),
                Properties = GetStringOrNull(txtNotes.Text),
                Weight = GetStringOrNull(txtWeight.ValueString),
                Type = GetStringOrNull(txtType.Text)
            };

            return a;
        }

        public bool EnableSpellCheck
        {
            get
            {
                return SpellCheck.GetIsEnabled(txtNotes);
            }
            set
            {
                SpellCheck.SetIsEnabled(txtNotes, value);
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            rowDetails.Height = new GridLength(1, GridUnitType.Star);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            rowDetails.Height = new GridLength(0);
        }

        // event just to update main window's "isDirty" value
        public event EventHandler? ContentChanged;

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ContentChanged?.Invoke(this, e);
        }

        private void txtWeight_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
