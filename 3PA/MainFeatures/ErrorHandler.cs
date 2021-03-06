﻿#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ErrorHandler.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamuiFramework.Themes;
using _3PA.Lib;

// ReSharper disable LocalizableElement

namespace _3PA.MainFeatures {

    internal static class ErrorHandler {

        /// <summary>
        /// Allows to keep track of the messages already displayed to the user
        /// </summary>
        private static HashSet<string> _catchedErrors = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Shows a Messagebox informing the user that something went wrong with a file,
        /// renames said file with the suffix "_errors"
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <param name="fileName"></param>
        public static void ShowErrors(Exception e, string message, string fileName) {
            Log(e.ToString());
            MessageBox.Show("Attention user! An error has occurred while loading the following file :" + "\n\n"
                + fileName +
                "\n\n" + "The file has been suffixed with '_errors' to avoid further problems.", AssemblyInfo.AssemblyProduct + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (File.Exists(fileName + "_errors"))
                File.Delete(fileName + "_errors");
            File.Move(fileName, fileName + "_errors");
        }

        /// <summary>
        /// Shows an error to the user
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        public static void ShowErrors(Exception e, string message) {
            // log the error into a file
            if (Log(message + "\r\n" + e)) {
                if (UserCommunication.Ready) {
                    // show it to the user
                    UserCommunication.Notify("The last action you started has triggered an error and has been cancelled.<br><br>1. If you didn't ask anything from 3P then you can probably ignore this message.<br>2. Otherwise, you might want to check out the error log below :" + (File.Exists(Config.FileErrorLog) ? "<br>" + Config.FileErrorLog.ToHtmlLink("Link to the error log") : "no .log found!") + "<br>Consider opening an issue on GitHub :<br>" + Config.IssueUrl.ToHtmlLink() + "<br><br><b>As a last resort, try restart Notepad++ and see if things are better!</b>",
                        MessageImg.MsgPoison, "Unexpected error", message,
                        args => {
                            if (args.Link.EndsWith(".log")) {
                                Npp.Goto(args.Link);
                                args.Handled = true;
                            }
                        },
                        0, 500);
                } else {
                    // show an old school message
                    MessageBox.Show("An error has occurred and we couldn't display a notification.\n\nThis very likely happened during the plugin loading; hence there is a hugh probability that it will cause the plugin to not operate normally.\n\nCheck the log at the following location to learn more about this error : " + Config.FileErrorLog.ProgressQuoter() + "\n\nTry to restart Notepad++, consider opening an issue on : " + Config.IssueUrl + " if the problem persists.", AssemblyInfo.AssemblyProduct + " error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Log a piece of information
        /// returns false if the error already occured during the session, true otherwise
        /// </summary>
        public static bool Log(string message, bool offlineLogOnly = false) {

            // don't show/store the same error twice in a session
            if (_catchedErrors.Contains(message))
                return false;
            _catchedErrors.Add(message);

            var toAppend = new StringBuilder("***************************\r\n");

            try {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var callingClass = method.DeclaringType;
                var callingMethod = method.Name;

                toAppend.AppendLine("**" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "**");
                if (method.DeclaringType != null && !method.DeclaringType.Name.Equals("ErrorHandler"))
                    toAppend.AppendLine("*From " + callingClass + "." + callingMethod + "()*");
                toAppend.AppendLine("```");
                toAppend.AppendLine(message);
                toAppend.AppendLine("```\r\n");

                File.AppendAllText(Config.FileErrorLog, toAppend.ToString());
            } catch (Exception) {
                // nothing to do
            }

            if (!offlineLogOnly) {
                try {
                    File.AppendAllText(Config.FileErrorToSend, toAppend.ToString());

                    // send to github
                    Task.Factory.StartNew(() => {
                        if (Config.Instance.GlobalDontAutoPostLog || User.SendIssue(File.ReadAllText(Config.FileErrorToSend), Config.SendLogApi)) {
                            Utils.DeleteFile(Config.FileErrorToSend);
                        }
                    });
                } catch (Exception) {
                    // nothing to do
                }
            }

            return true;
        }

        /// <summary>
        /// Log a piece of information
        /// returns false if the error already occured during the session, true otherwise
        /// </summary>
        public static bool LogError(Exception e) {

            return true;
        }

        public static string GetHtmlLogLink {
            get {
                return Config.Instance.UserGetsPreReleases ? "<br>More details can be found in log file below :" + (File.Exists(Config.FileErrorLog) ? "<br>" + Config.FileErrorLog.ToHtmlLink("Link to the error log") : "no .log found!") : "";
            }
        } 

        public static void UnhandledErrorHandler(object sender, UnhandledExceptionEventArgs args) {
            ShowErrors((Exception)args.ExceptionObject, "Unhandled error");
        }

        public static void ThreadErrorHandler(object sender, ThreadExceptionEventArgs e) {
            ShowErrors(e.Exception, "Thread error");
        }

        public static void UnobservedErrorHandler(object sender, UnobservedTaskExceptionEventArgs e) {
            ShowErrors(e.Exception, "Unobserved task error");
        }
    }
}
