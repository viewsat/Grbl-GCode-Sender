﻿/*
 * WizardConfig.cs - part of CNC Controls library for Grbl
 *
 * v0.01 / 2019-11-07 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2019, Io Engineering (Terje Io)
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

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CNC.Core;
using CNC.GCode;

namespace CNC.Controls.Lathe
{
    public class ActiveProfile
    {
        private ProfileData active = null;

        public ActiveProfile()
        {
            ZDirection = WizardConfig.zdir;
            xmode = WizardConfig.xmode;
            xmodelock = WizardConfig.xmodelock;

            Update();
        }

        public ProfileData Profile
        {
            get { return active; }
            set
            {
                active = value;
                SetLimits();
            }
        }
        public bool IsLoaded { get { return active != null; } }
        public string ProfileName { get { return active.Name; } }
        public double PassDepthFirst { get { return active.PassDepthFirst; } }
        public double PassDepthLast { get { return active.PassDepthLast; } }
        public double PassDepthMin { get { return active.PassDepthMin; } }
        public double XClearance { get { return active.XClearance; } }
        public double ZClearance { get { return active.ZClearance; } }
        public double Feedrate { get { return active.Feedrate; } }
        public double FeedrateLast { get { return active.FeedrateLast; } }
        public double FeedrateMax { get { return active.FeedrateMax; } }
        public double RPM { get { return active.RPM; } }
        public bool CSS { get { return active.CSS; } }
        public double CSSMaxRPM { get { return active.CSSMaxRPM; } }

        public double RpmMin { get; private set; }
        public double RpmMax { get; private set; }
        public bool metric { get; private set; }
        public double XMaxFeedRate { get; private set; }
        public double XAcceleration { get; private set; }
        public double ZMaxFeedRate { get; private set; }
        public double ZAcceleration { get; private set; }
        public double ZDirection { get; private set; }
        public LatheMode xmode { get; private set; }
        public bool xmodelock { get; private set; }

        private void SetLimits()
        {
            if (IsLoaded)
            {
                if (!CSS)
                    active.RPM = Math.Min(Math.Max(RPM, RpmMin), RpmMax);
                else
                    active.CSSMaxRPM = Math.Min(Math.Max(active.CSSMaxRPM, RpmMin), RpmMax);
            }
        }

        public bool Update()
        {
            if (GrblSettings.Loaded)
            {
                XMaxFeedRate = GrblSettings.GetDouble(GrblSetting.AxisSetting_XMaxRate);
                XAcceleration = GrblSettings.GetDouble(GrblSetting.AxisSetting_XAcceleration);
                ZMaxFeedRate = GrblSettings.GetDouble(GrblSetting.AxisSetting_ZMaxRate);
                ZAcceleration = GrblSettings.GetDouble(GrblSetting.AxisSetting_ZAcceleration);
                RpmMin = GrblSettings.GetDouble(GrblSetting.RpmMin);
                RpmMax = GrblSettings.GetDouble(GrblSetting.RpmMax);

                GrblParserState.Get();

                metric = GrblParserState.IsMetric;
                if (!xmodelock)
                    xmode = GrblParserState.LatheMode;

                SetLimits();
            }

            return GrblSettings.Loaded;
        }
    }

    [Serializable]
    public class ProfileData : ViewModelBase
    {
        private double
            _PassDepthFirst = 0.1d,
            _PassDepthLast = 0.02d,
            _PassDepthMin = 0.02d,
            _XClearance = 0.5d,
            _ZClearance = 0.5d,
            _RPM = 100.0d,
            _Feedrate = 200.0d,
            _FeedrateLast = 100.0d,
            _FeedrateMax = 200.0d,
            _CSSMaxRPM = 0.0d;

        private bool _CSS = false;

        [XmlIgnore]
        public int Id { get; set; }
        public string Name { get; set; }

        #region DependencyProperties
        public double PassDepthFirst
        {
            get { return _PassDepthFirst; }
            set { _PassDepthFirst = value; OnPropertyChanged(); }
        }
        public double PassDepthLast
        {
            get { return _PassDepthLast; }
            set { _PassDepthLast = value; OnPropertyChanged(); }
        }
        public double PassDepthMin
        {
            get { return _PassDepthMin; }
            set { _PassDepthMin = value; OnPropertyChanged(); }
        }

        public double XClearance
        {
            get { return _XClearance; }
            set { _XClearance = value; OnPropertyChanged(); }
        }
        public double ZClearance
        {
            get { return _ZClearance; }
            set { _ZClearance = value; OnPropertyChanged(); }
        }
        public double Feedrate
        {
            get { return _Feedrate; }
            set { _Feedrate = value; OnPropertyChanged(); }
        }
        public double FeedrateLast
        {
            get { return _FeedrateLast; }
            set { _FeedrateLast = value; OnPropertyChanged(); }
        }
        public double FeedrateMax
        {
            get { return _FeedrateMax; }
            set { _FeedrateMax = value; OnPropertyChanged(); }
        }
        public double RPM
        {
            get { return _RPM; }
            set { _RPM = value; OnPropertyChanged(); }
        }
        public double CSSMaxRPM
        {
            get { return _CSSMaxRPM; }
            set { _CSSMaxRPM = value; OnPropertyChanged(); }
        }
        public bool CSS
        {
            get { return _CSS; }
            set { _CSS = value; OnPropertyChanged(); }
        }
        #endregion
    }

    public class LatheProfile
    {
        public ObservableCollection<ProfileData> profiles = new ObservableCollection<ProfileData>();

        private int id = 0;
        private string filename;

        public LatheProfile(string filename)
        {
            this.filename = Core.Resources.Path + "\\" + filename;
        }

        public ProfileData Add()
        {
            ProfileData data = new ProfileData();

            data.Name = "Default";
            data.Id = id++;

            profiles.Add(data);

            return data;
        }

        public void Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ProfileData>));

            FileStream fsout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            try
            {
                using (fsout)
                {
                    xs.Serialize(fsout, profiles);
                }
            }
            catch
            {
            }
        }

        public void Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ProfileData>));

            try
            {
                StreamReader reader = new StreamReader(filename);
                profiles = (ObservableCollection<ProfileData>)xs.Deserialize(reader);
                reader.Close();

                foreach (ProfileData profile in profiles)
                    profile.Id = id++;
            }
            catch
            {
            }
        }
    }

    public class WizardConfig
    {
        public LatheProfile profile;

        public WizardConfig(string name)
        {
            zdir = -1.0d;
            xmode = LatheMode.Radius;
            xmodelock = false;

            ProfileName = name;

            profile = new LatheProfile(name + "Profiles.xml");

            ActiveProfile = new ActiveProfile();

            using (new UIUtils.WaitCursor())
            {
                profile.Load();

                if (profile.profiles.Count() == 0)
                {
                    ActiveProfile.Profile = profile.Add();

                    profile.Save();
                }
                else
                    ActiveProfile.Profile = profile.profiles[0];

            }
        }

        public void ApplySettings(LatheConfig config)
        {
            zdir = config.ZDirFactor;
            xmode = config.XMode;
            xmodelock = xmode != LatheMode.Disabled;
        }

        public ActiveProfile ActiveProfile { get; private set; }

        public string ProfileName { get; private set; }
  
        public ObservableCollection<ProfileData> Profiles { get { return profile.profiles; } }

        public static double zdir { get; private set; } = -1d;
        public static LatheMode xmode { get; private set; } = LatheMode.Disabled;
        public static bool xmodelock { get; private set; } = false;

        public ProfileData Add()
        {
            return profile.Add();
        }

        public bool Update(ProfileData profile, double xclear, LatheMode xmode)
        {
            WizardConfig.xmode = xmode;

            if (profile != null)
            {
                ActiveProfile.Profile = profile;
                ActiveProfile.Profile.XClearance = xclear;
            }

            this.profile.Save();

            return true;
        }
    }

    public class PassCalc
    {
        private int _passes = 1;

        public PassCalc(double distance, double passdepth, double passdepth_last, int precision)
        {
            this.passdepth = passdepth;
            this.passdepth_last = passdepth_last;
            this.distance = Math.Round(Math.Abs(distance), precision);

            if (this.distance < this.passdepth_last)
                this.passdepth_last = this.distance;

            else if (this.distance < this.passdepth)
            {
                if (this.passdepth_last > 0.0d)
                    this._passes++;
                this.passdepth = this.distance - this.passdepth_last;
            }
            else
            {
                this.distance -= this.passdepth_last;
                this._passes = (int)Math.Floor(this.distance / this.passdepth);

                if (this.passdepth * (double)this._passes < this.distance)
                {
                    this._passes++;
                    this.passdepth = Math.Round(this.distance / (double)this._passes, precision);
                }
                this._passes++; // Add last pass
            }

            this.DOC = this.passdepth;
        }

        public int passes { get { return _passes + springpasses; } }
        public int springpasses { get; set; }
        public double passdepth { get; private set; }
        public double passdepth_last { get; private set; }
        public double distance { get; private set; }
        public bool IsLastPass { get; private set; }
        public bool dir { get; private set; }
        public double DOC { get; private set; }
        public double Distance { get; private set; }

        public double GetPassTarget(uint pass, double start, bool negative)
        {
            if (pass <= passes)
            {
                if ((IsLastPass = pass >= _passes))
                {
                    this.Distance = distance + passdepth_last;
                    this.DOC = pass > _passes ? 0.0d : passdepth_last;
                }
                else if (pass == _passes - 1)
                    this.Distance = distance;
                else
                    this.Distance = Math.Min(passdepth * (double)pass, distance);
            }

            return pass > passes ? double.NaN : (negative ? -this.Distance : this.Distance) + start;
        }
    }
}
