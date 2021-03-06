﻿#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (YamuiPage.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Windows.Forms;
using YamuiFramework.Themes;

namespace YamuiFramework.Controls {

    public class YamuiPage : UserControl {

        #region constructor

        public YamuiPage() {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Method called by YamuiTab when the page changes to this page
        /// </summary>
        public virtual void OnShow() { }

        /// <summary>
        /// Method called by YamuiTab when the page changes from this one and when the form closes
        /// </summary>
        public virtual void OnHide() { }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e) {
            // paint background
            e.Graphics.Clear(YamuiThemeManager.Current.FormBack);
        }

        #endregion

    }
}
