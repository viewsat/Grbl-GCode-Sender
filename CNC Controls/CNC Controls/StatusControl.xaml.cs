﻿/*
 * StatusControl.xaml.cs - part of CNC Controls library for Grbl
 *
 * v0.03 / 2019-12-06 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2019, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

· Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

· Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

· Neither the name of the copyright holder nor the names of its contributors may
be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CNC.Core;

namespace CNC.Controls
{

    public partial class StatusControl : UserControl
    {
        private Brush HomeButtonColor;
        private enum StatusButton
        {
            Home,
            Unlock,
            Reset,
            Check
        }

        public StatusControl()
        {
            InitializeComponent();

       //     DataContextChanged += StatusControl_DataContextChanged;

            HomeButtonColor = btnHome.Background;

            btnHome.Tag = StatusButton.Home;
            btnReset.Tag = StatusButton.Reset;
            btnUnlock.Tag = StatusButton.Unlock;
            chkCheckMode.Tag = StatusButton.Check;
        }

        private void StatusControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is INotifyPropertyChanged)
                ((INotifyPropertyChanged)e.OldValue).PropertyChanged -= OnDataContextPropertyChanged;
            if (e.NewValue != null && e.NewValue is INotifyPropertyChanged)
                ((INotifyPropertyChanged)e.NewValue).PropertyChanged += OnDataContextPropertyChanged;
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GrblViewModel && e.PropertyName == nameof(GrblViewModel.HomedState))
                SetHomingState(((GrblViewModel)sender).HomedState);
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((StatusButton)((Control)sender).Tag)
            {
                case StatusButton.Reset:
                    Grbl.Reset();
                    break;

                case StatusButton.Unlock:
                    ((GrblViewModel)DataContext).ExecuteMDI(GrblConstants.CMD_UNLOCK);
                    break;

                case StatusButton.Home:
                    // ((Control)sender).Background = Brushes.LightSkyBlue;
                    ((GrblViewModel)DataContext).ExecuteMDI(GrblConstants.CMD_HOMING);
                    break;

                case StatusButton.Check:
                    GrblStates state = ((GrblViewModel)DataContext).GrblState.State;
                    if(state == GrblStates.Check && ((CheckBox)sender).IsChecked == false)
                        Grbl.Reset();
                    else if (state == GrblStates.Idle && ((CheckBox)sender).IsChecked == true)
                        ((GrblViewModel)DataContext).ExecuteMDI(GrblConstants.CMD_CHECK);
                    break;
            }
        }

        public void SetHomingState(HomedState state)
        {
            //switch (state)
            //{
            //    case HomedState.Unknown:
            //        btnHome.Background = HomeButtonColor;
            //        break;

            //    case HomedState.NotHomed:
            //        btnHome.Background = Brushes.LightYellow;
            //        break;

            //    case HomedState.Homed:
            //        btnHome.Background = Brushes.LightGreen;
            //        break;
            //}
        }
    }
}
