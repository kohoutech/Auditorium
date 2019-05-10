﻿/* ----------------------------------------------------------------------------
Audimat : an audio plugin host
Copyright (C) 2005-2019  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Audimat.UI;
using Transonic.VST;
using Transonic.MIDI.System;
using Transonic.Widget;

namespace Audimat
{
    public partial class AudimatWindow : Form
    {
        public ControlPanel controlPanel;
        public VSTRack rack;

        //child windows
        public PatchWindow patchWin;
        public KeyboardWnd keyboardWnd;

        //backend & i/o
        public Vashti vashti;
        public WaveDevices waveDevices;
        public MidiSystem midiDevices;

        public bool isRunning;

        //for saving keyboard window settings
        public Point keyWindowPos;
        public int keyWindowSize;
        public VSTPlugin keyWindowPlugin;

        String curpluginPath;
        int vstnum;         //for debugging

        public AudimatWindow()
        {
            InitializeComponent();

            //control panel goes just below menubar
            controlPanel = new ControlPanel(this);
            this.Controls.Add(controlPanel);
            controlPanel.Location = new Point(this.ClientRectangle.Left, AudimatMenu.Bottom);
            //controlPanel.Anchor = AnchorStyles.Top;
            this.Controls.Add(controlPanel);

            //rack control fills up entire client area between control panel & status bar
            rack = new VSTRack(this);
            rack.Location = new Point(this.ClientRectangle.Left, controlPanel.Bottom);
            this.Controls.Add(rack);
            controlPanel.Width = rack.Width;

            int minHeight = this.AudimatMenu.Height + controlPanel.Height + this.AudimatStatus.Height;
            this.ClientSize = new System.Drawing.Size(rack.Size.Width, rack.Size.Height + minHeight);
            this.MinimumSize = new System.Drawing.Size(this.Size.Width, this.Size.Height - VSTPanel.PANELHEIGHT);
            this.MaximumSize = new System.Drawing.Size(this.Size.Width, Int32.MaxValue);

            patchWin = new PatchWindow(this);

            vashti = new Vashti();
            waveDevices = new WaveDevices();
            midiDevices = new MidiSystem();

            isRunning = false;

            keyWindowPos = new Point(0, 0);
            keyWindowSize = -1;
            keyWindowPlugin = null;

            curpluginPath = "";
            vstnum = 0;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (rack != null)
            {
                rack.Size = new Size(this.ClientSize.Width, AudimatStatus.Top - controlPanel.Bottom);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            stopHost();
            rack.shutdown();            //unload all plugins in rack
            midiDevices.shutdown();     //close midi devices
            vashti.shutDown();          //shut down back end
        }

        //callback when rack's contents have increased or decreased
        public void rackChanged()
        {
            if (keyboardWnd != null)
            {
                keyboardWnd.updatePluginList();
            }
        }

        //- file menu -----------------------------------------------------------------

        private void exitFileMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //- host menu -----------------------------------------------------------------

        public void startHost()
        {
            host.startEngine();
            isRunning = true;
            lblAudimatStatus.Text = "Engine is running";
        }

        public void stopHost()
        {
            host.stopEngine();
            isRunning = false;
            lblAudimatStatus.Text = "Engine is stopped";
        }

        private void StartHost_Click(object sender, EventArgs e)
        {
            startHost();
        }

        private void StopHost_Click(object sender, EventArgs e)
        {
            stopHost();
        }

        public void enableKeyboardBarMenuItem(bool enable)
        {
            keyboardBarHostMenuItem.Enabled = enable;
            controlPanel.btnKeys.Enabled = enable;
        }

        public void showKeyboardWindow()
        {
            keyboardWnd = new KeyboardWnd(this);
            keyboardWnd.Icon = this.Icon;
            keyboardWnd.FormClosing += new FormClosingEventHandler(keyboardWindow_FormClosing);
            //restore previous keyboard vals, if set
            if (keyWindowSize != -1)
            {
                keyboardWnd.setSize(keyWindowSize);
            }
            keyboardWnd.setSelectedPlugin(keyWindowPlugin);
            keyboardWnd.Show();

            if (keyWindowPos.X != 0 && keyWindowPos.Y != 0)
            {
                keyboardWnd.Location = keyWindowPos;
            }

            enableKeyboardBarMenuItem(false);
        }

        private void keysButton_Click(object sender, EventArgs e)
        {
            showKeyboardWindow();
        }

        private void keyboardWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            keyWindowPos = keyboardWnd.Location;
            keyWindowSize = keyboardWnd.cbxKeySize.SelectedIndex;
            keyWindowPlugin = keyboardWnd.currentPlugin;
            enableKeyboardBarMenuItem(true);
        }

        private void panicButton_Click(object sender, EventArgs e)
        {
            //not implemented yet
        }

        //- plugin menu -------------------------------------------------------

        public void loadPlugin()
        {
            String pluginPath = "";

#if (DEBUG)
            //useful for testing, don't have to go through the FileOPen dialog over & over
            string[] vstlist = File.ReadAllLines("vst.lst");
            pluginPath = vstlist[vstnum++];
            if (vstnum >= vstlist.Length) vstnum = 0;       //wrap it around

#else
            loadPluginDialog.Title = "load a VST";
            loadPluginDialog.InitialDirectory = curpluginPath;
            loadPluginDialog.Filter =  "VST plugins (*.dll)|*.dll|All files (*.*)|*.*";
            loadPluginDialog.ShowDialog();            
            pluginPath = loadPluginDialog.FileName;
            if (pluginPath.Length == 0) return;
            curpluginPath = Path.GetDirectoryName(pluginPath);
#endif
            bool result = rack.addPanel(pluginPath);

            if (!result)
            {
                MessageBox.Show("failed to load the plugin file: " + pluginPath, "Plugin Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadPlugin_Click(object sender, EventArgs e)
        {
            loadPlugin();
        }

        //- help menu -----------------------------------------------------------------

        private void aboutHelpMenuItem_Click(object sender, EventArgs e)
        {
            String msg = "Audimat\nversion 1.2.0\n" + "\xA9 Transonic Software 2007-2019\n" + "http://transonic.kohoutech.com";
            MessageBox.Show(msg, "About");
        }

        //- i/o connections ---------------------------------------------------

        public void connectMidiInput(int idx, PluginMidiIn pluginMidiIn)
        {
            InputDevice indev = midiDevices.inputDevices[idx];
            try
            {
                indev.open();
                indev.connectUnit(pluginMidiIn);
                indev.start();                
            }
            catch
            {
                //Console.WriteLine("error connecting midi input");
            }
        }

        public void disconnectMidiInput(int idx, PluginMidiIn pluginMidiIn)
        {
            InputDevice indev = midiDevices.inputDevices[idx];
            indev.disconnectUnit(pluginMidiIn);
        }

        public void connectMidiOutput(int idx, PluginMidiIn pluginMidiIn)
        {
        }
    }
}

//  Console.WriteLine(" there's no sun in the shadow of the wizard");
