﻿/* ----------------------------------------------------------------------------
Audimat : an audio plugin host
Copyright (C) 2005-2020  George E Greaney

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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Kohoutech.VST;

namespace Audimat.UI
{
    class PluginInfoWnd : Form
    {
        VSTPanel panel;
        private Label lblPlugInfo;
        VSTPlugin plugin;

        public PluginInfoWnd(VSTPanel _panel)
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(0x3f, 0xff, 0x00);

            panel = _panel;
            plugin = panel.plugin;

            //flags
            String flags = "";
            if ((plugin.flags & (1 << 0)) != 0) flags += "  Has editor\n";
            if ((plugin.flags & (1 << 4)) != 0) flags += "  Supports replacing processing\n";
            if ((plugin.flags & (1 << 5)) != 0) flags += "  Program data is handled in chunks\n";
            if ((plugin.flags & (1 << 8)) != 0) flags += "  Is a synth\n";
            if ((plugin.flags & (1 << 9)) != 0) flags += "  Does not produce sound when input is silent\n";
            if ((plugin.flags & (1 << 12)) != 0) flags += "  Supports double precision processing\n";

            //unique ID
            byte[] idbytes = BitConverter.GetBytes(plugin.uniqueID);
            String idstr = System.Text.Encoding.ASCII.GetString(idbytes);
            char[] idchars = idstr.ToCharArray();
            Array.Reverse(idchars);
            idstr = new String(idchars);

            String infotext = plugin.name + "\n" +
                plugin.vendor + "\n\n" +
                //plugin.version + "\n" +
                plugin.numPrograms + " programs\n" +
                plugin.numParams + " parameters\n" +
                plugin.numInputs + " inputs\n" +
                plugin.numOutputs + " outputs\n\n" +
                "Flags : 0x" + plugin.flags.ToString("X4") + "\n" +
                flags +
                "Unique ID : " + idstr + "\n";
            lblPlugInfo.Text = infotext;
        }

        private void InitializeComponent()
        {
            this.lblPlugInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPlugInfo
            // 
            this.lblPlugInfo.AutoSize = true;
            this.lblPlugInfo.Location = new System.Drawing.Point(13, 13);
            this.lblPlugInfo.Name = "lblPlugInfo";
            this.lblPlugInfo.Size = new System.Drawing.Size(39, 13);
            this.lblPlugInfo.TabIndex = 0;
            this.lblPlugInfo.Text = "no info";
            // 
            // PluginInfoWnd
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.lblPlugInfo);
            this.Name = "PluginInfoWnd";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
