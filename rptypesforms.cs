#region Copyright
/*
 *  Report Manager:  Database Reporting tool for .Net and Mono
 *
 *     The contents of this file are subject to the MPL License
 *     with optional use of GPL or LGPL licenses.
 *     You may not use this file except in compliance with the
 *     Licenses. You may obtain copies of the Licenses at:
 *     http://reportman.sourceforge.net/license
 *
 *     Software is distributed on an "AS IS" basis,
 *     WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations.
 *
 *  Copyright (c) 1994 - 2008 Toni Martir (toni@reportman.es)
 *  All Rights Reserved.
*/
#endregion

using System;
using System.Windows.Forms;

namespace Reportman.Drawing.Forms
{
	/// <summary>
	/// Provide utitilies about handling Key codes
	/// </summary>
	public class KeyUtil
	{
        /// <summary>
        /// Convert numpad digits to normal digits
        /// </summary>
        /// <param name="keycode"></param>
        /// <returns></returns>
        public static Keys ConvertKeyPad(Keys keycode)
        {
            Keys aresult = keycode;
            switch (keycode)
            {
                case Keys.NumPad0:
                    aresult = Keys.D0;
                    break;
                case Keys.NumPad1:
                    aresult = Keys.D1;
                    break;
                case Keys.NumPad2:
                    aresult = Keys.D2;
                    break;
                case Keys.NumPad3:
                    aresult = Keys.D3;
                    break;
                case Keys.NumPad4:
                    aresult = Keys.D4;
                    break;
                case Keys.NumPad5:
                    aresult = Keys.D5;
                    break;
                case Keys.NumPad6:
                    aresult = Keys.D6;
                    break;
                case Keys.NumPad7:
                    aresult = Keys.D7;
                    break;
                case Keys.NumPad8:
                    aresult = Keys.D8;
                    break;
                case Keys.NumPad9:
                    aresult = Keys.D9;
                    break;
            }
            return aresult;
        }
        /// <summary>
        /// Returns true if the key is a control key code like arrows, home, end...
        /// </summary>
        public static bool IsSpecialKeyCode(Keys keycode)
        {
            bool aresult = false;
            switch (keycode)
            {
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Tab:
                case Keys.Escape:
                case Keys.Home:
                case Keys.End:
                case Keys.Delete:
                case Keys.Insert:
                case Keys.PageDown:
                case Keys.PageUp:
                case Keys.Back:
                case Keys.Alt:
                case Keys.Add:
                case Keys.Capital:
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.Clear:
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.F13:
                case Keys.F14:
                case Keys.F15:
                case Keys.F16:
                case Keys.F17:
                case Keys.F18:
                case Keys.F19:
                case Keys.F20:
                case Keys.F21:
                case Keys.F22:
                case Keys.F23:
                case Keys.F24:
                case Keys.PrintScreen:
                case Keys.Scroll:
                case Keys.Pause:
                case Keys.Print:
                case Keys.Zoom:
                case Keys.XButton1:
                case Keys.XButton2:
                case Keys.VolumeUp:
                case Keys.VolumeDown:
                case Keys.VolumeMute:
                case Keys.Sleep:
                case Keys.ShiftKey:
                case Keys.Shift:
                case Keys.Separator:
                case Keys.SelectMedia:
                case Keys.Select:
                case Keys.RWin:
                case Keys.RShiftKey:
                case Keys.RMenu:
                case Keys.Return:
                case Keys.RControlKey:
                case Keys.RButton:
                case Keys.ProcessKey:
                case Keys.Play:
                case Keys.NumLock:
                case Keys.MediaStop:
                case Keys.MediaPreviousTrack:
                case Keys.MediaPlayPause:
                case Keys.MediaNextTrack:
                case Keys.MButton:
                case Keys.LWin:
                case Keys.LShiftKey:
                case Keys.LMenu:
                case Keys.LineFeed:
                case Keys.LControlKey:
                case Keys.LButton:
                case Keys.LaunchMail:
                case Keys.LaunchApplication2:
                case Keys.LaunchApplication1:
                case Keys.KanjiMode:
                case Keys.KanaMode:
                case Keys.KeyCode:
                case Keys.JunjaMode:
                case Keys.IMENonconvert:
                case Keys.IMEModeChange:
                case Keys.IMEConvert:
                case Keys.IMEAceept:
                case Keys.Help:
                case Keys.FinalMode:
                case Keys.Exsel:
                case Keys.Execute:
                case Keys.EraseEof:
                case Keys.Crsel:
                case Keys.BrowserStop:
                case Keys.BrowserSearch:
                case Keys.BrowserRefresh:
                case Keys.BrowserHome:
                case Keys.BrowserForward:
                case Keys.BrowserFavorites:
                case Keys.BrowserBack:
                case Keys.Attn:
                case Keys.Apps:
                    aresult = true;
                    break;
            }
            return aresult;
        }
	}
}

