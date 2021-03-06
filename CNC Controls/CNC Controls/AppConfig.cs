﻿/*
 * AppConfig.cs - part of Grbl Code Sender
 *
 * v0.03 / 2019-12-03 / Io Engineering (Terje Io)
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
using System.IO;
using System.Xml.Serialization;
using CNC.Core;
using System.Windows;
using CNC.GCode;

namespace CNC.Controls
{

    [Serializable]
    public class LatheConfig : ViewModelBase
    {
        private bool _isEnabled = false;
        private LatheMode _latheMode = LatheMode.Disabled;

        [XmlIgnore]
        public double ZDirFactor { get { return ZDirection == ZDirection.Negative? -1d : 1d;} }

        [XmlIgnore]
        public LatheMode[] LatheModes { get { return (LatheMode[])Enum.GetValues(typeof(LatheMode)); } }

        [XmlIgnore]
        public ZDirection[] ZDirections { get { return (ZDirection[])Enum.GetValues(typeof(ZDirection)); } }

        [XmlIgnore]
        public bool IsEnabled { get { return _isEnabled; }  set { _isEnabled = value; OnPropertyChanged(); } }


        public LatheMode XMode { get { return _latheMode; } set { _latheMode = value; IsEnabled = value != LatheMode.Disabled; } }
        public ZDirection ZDirection { get; set; } = ZDirection.Negative;
        public double PassDepthLast { get; set; } = 0.02d;
        public double FeedRate { get; set; } = 300d;
    }

    [Serializable]
    public class CameraConfig : ViewModelBase
    {
        double _xoffset = 0d, _yoffset = 0d;

        [XmlIgnore]
        public CameraMoveMode[] MoveModes { get { return (CameraMoveMode[])Enum.GetValues(typeof(CameraMoveMode)); } }

        public double XOffset { get { return _xoffset; } set { _xoffset = value; OnPropertyChanged(); } }
        public double YOffset { get { return _yoffset; } set { _yoffset = value; OnPropertyChanged(); } }
        public CameraMoveMode MoveMode { get; set; } = CameraMoveMode.BothAxes;
    }

    [Serializable]
    public class GCodeViewerConfig : ViewModelBase
    {
        private bool _isEnabled = false;

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; OnPropertyChanged(); } }
        public int ArcResolution { get; set; } = 10;
        public double MinDistance { get; set; } = 0.05d;
        public bool ShowGrid { get; set; } = true;
        public bool ShowAxes { get; set; } = true;
        public bool ShowBoundingBox { get; set; } = false;
        public bool ShowViewCube { get; set; } = true;
    }

    [Serializable]

    public class Config : ViewModelBase
    {
        public int PollInterval { get; set; } = 200; // ms
        public string PortParams { get; set; } = "COM1:115200,N,8,1";
        public LatheConfig Lathe { get; set; } = new LatheConfig();
        public CameraConfig Camera { get; set; } = new CameraConfig();
        public GCodeViewerConfig GCodeViewer { get; set; } = new GCodeViewerConfig();
    }

    public class AppConfig
    {
        public Config Config = null;

        private string configfile = null;

        public string FileName { get; private set; }

        public bool Save (string filename)
        {
            bool ok = false;

            if(Config == null)
                Config = new Config();

            XmlSerializer xs = new XmlSerializer(typeof(Config));

            try
            {
                FileStream fsout = new FileStream(configfile, FileMode.Create, FileAccess.Write, FileShare.None);
                using (fsout)
                {
                    xs.Serialize(fsout, Config);
                    this.configfile = filename;
                    ok = true;
                }
            }
            catch
            {
            }

            return ok;
        }

        public bool Save()
        {
            return configfile != null && Save(configfile);
        }

        public bool Load(string filename)
        {
            bool ok = false;
            XmlSerializer xs = new XmlSerializer(typeof(Config));

            try
            {
                StreamReader reader = new StreamReader(filename);
                Config = (Config)xs.Deserialize(reader);
                reader.Close();
                configfile = filename;
                ok = true;
            }
            catch
            {
            }

            return ok;
        }

        private void setPort (string port)
        {
            Config.PortParams = port;
            if(!(Config.PortParams.ToLower().StartsWith("ws://") || char.IsDigit(Config.PortParams[0])) && Config.PortParams.IndexOf(':') == -1)
                 Config.PortParams += ":115200,N,8,1";
        }

        public int SetupAndOpen(string appname, GrblViewModel model, System.Windows.Threading.Dispatcher dispatcher)
        {
            int status = 0;
            bool selectPort = false;
            string port = string.Empty;

            CNC.Core.Resources.Path = AppDomain.CurrentDomain.BaseDirectory;

            string[] args = Environment.GetCommandLineArgs();

            int p = 0;
            while (p < args.GetLength(0)) switch (args[p++])
            {
                case "-inifile":
                    CNC.Core.Resources.IniName = GetArg(args, p++);
                    break;

                case "-configmapping":
                    CNC.Core.Resources.ConfigName = GetArg(args, p++);
                    break;

                case "-language":
                    CNC.Core.Resources.Language = GetArg(args, p++);
                    break;

                case "-port":
                    port = GetArg(args, p++);
                    break;

                case "-selectport":
                    selectPort = true;
                    break;

                default:
                    if(File.Exists(args[p - 1]))
                        FileName = args[p - 1];
                    break;
            }

            if (!Load(CNC.Core.Resources.IniFile))
            {
                if (MessageBox.Show("Config file not found or invalid, create new?", appname, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (!Save(CNC.Core.Resources.IniFile))
                    {
                        MessageBox.Show("Could not save config file.", appname);
                        status = 1;
                    }
                }
                else
                    status = 1;
            }

            if (!string.IsNullOrEmpty(port))
                selectPort = false;

            if (!selectPort)
            {
                if (!string.IsNullOrEmpty(port))
                    setPort(port);
#if USEWEBSOCKET
                if (Config.PortParams.ToLower().StartsWith("ws://"))
                    new WebsocketStream(Config.PortParams, dispatcher);
                else
#endif
                if (char.IsDigit(Config.PortParams[0])) // We have an IP address
                    new TelnetStream(Config.PortParams, dispatcher);
                else
                    new SerialStream(Config.PortParams, Comms.ResetMode.None, dispatcher);
            }

            if ((Comms.com == null || !Comms.com.IsOpen) && string.IsNullOrEmpty(port))
            {
                PortDialog portsel = new PortDialog();

                port = portsel.ShowDialog(Config.PortParams);
                if (string.IsNullOrEmpty(port))
                    status = 2;

                else
                {
                    setPort(port);
#if USEWEBSOCKET
                    if (port.ToLower().StartsWith("ws://"))
                        new WebsocketStream(Config.PortParams, dispatcher);
                    else
#endif
                    if (char.IsDigit(port[0])) // We have an IP address
                        new TelnetStream(Config.PortParams, dispatcher);
                    else
                        new SerialStream(Config.PortParams, Comms.ResetMode.None, dispatcher);

                    Save(CNC.Core.Resources.IniFile);
                }
            }

            if (Comms.com != null && Comms.com.IsOpen)
            {
                int delay = 40; // 400 ms

                // Wait to see if a MPG is polling Grbl...
                while (delay-- != 0 && Comms.com.Reply == "")
                    System.Threading.Thread.Sleep(10);

                // ...if so show dialog for wait for it to stop polling and relinquish control.
                if (!(Comms.com.Reply == "" || Comms.com.Reply.StartsWith("Grbl") || Comms.com.Reply.StartsWith("[MSG:")))
                {
                    MPGPending await = new MPGPending(model);
                    await.ShowDialog();
                    if (await.Cancelled)
                    {
                        Comms.com.Close();
                        status = 2;
                    }
                }

                model.PollInterval = Config.PollInterval;
            }
            else if (status != 2)
            {
                MessageBox.Show(string.Format("Unable to open connection ({0})", Config.PortParams), appname, MessageBoxButton.OK, MessageBoxImage.Error);
                status = 2;
            }

            return status;
        }

        private string GetArg(string[] args, int i)
        {
            return i < args.GetLength(0) ? args[i] : null;
        }
    }
}
