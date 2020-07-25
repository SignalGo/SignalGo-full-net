using SignalGo.Publisher.Models;
using SignalGo.Publisher.ViewModels;
using SignalGo.Publisher.Views.Extra;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace SignalGo.Publisher.Engines.Security
{
    public class AccessControl : AccessControlBase
    {
        public AccessControl() : base()
        {

        }
        private static int retryAttemp = 0;
        static bool locked = false;


        private static void ValidateAccessControlState()
        {
            if (ProjectManagerWindowViewModel.This.IsAccessControlUnlocked)
            {
                ProjectManagerWindowViewModel.This.IsAccessControlUnlocked = false;
                locked = true;
            }
        }
        public static void LockAccessControl()
        {
            ProjectManagerWindowViewModel.This.IsAccessControlUnlocked = false;
        }
        public static bool UnlockAccessControl()
        {
            InputDialogWindow inputDialog = new InputDialogWindow($"Please enter your Master Password", "Access Control");
            if (inputDialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(UserSettingInfo.Current.UserSettings.ApplicationMasterPassword))
                {
                    MessageBox.Show("First set a master password");
                }
                if (AccessControlBase.CheckMasterPassword(inputDialog.Answer))
                {
                    Debug.WriteLine("Application Access Granted Using Master Password!");
                }
                else
                    return false;
            }
            else return false;
            return true;
        }
        /// <summary>
        /// Interactive Authorization on the specified server
        /// </summary>
        /// <param name="serverInfo"></param>
        /// <returns></returns>
        public static bool AuthorizeServer(ServerInfo serverInfo)
        {
            // if access control it unlock grant access auto
            if (ProjectManagerWindowViewModel.This.IsAccessControlUnlocked)
                return true;
            // if server authorized already
            var key = serverInfo.ServerKey;
            if (ServerInfo.Servers.Any(x => x.ServerKey == key))
                return true;

            ValidateAccessControlState();
            try
            {
            GetThePass:
                if (retryAttemp > 2)
                {
                    retryAttemp = 0;
                    return false;
                }
                InputDialogWindow inputDialog = new InputDialogWindow($"Please enter your secret for Server", "Access Control", serverInfo.ServerName);
                if (inputDialog.ShowDialog() == true)
                {
                    if (!AccessControlBase.AuthorizeServer(inputDialog.Answer, ref serverInfo))
                    {
                        if (System.Windows.Forms.MessageBox.Show("password does't match!", "Access Denied", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        {
                            retryAttemp++;
                            goto GetThePass;
                        }
                        else
                        {
                            serverInfo.IsChecked = false;
                            serverInfo.ServerLastUpdate = "Access Denied!";
                        }
                    }
                }
                // if input dialog canceled
                else return false;
            }
            catch { }
            finally
            {
                if (locked)
                    ProjectManagerWindowViewModel.This.IsAccessControlUnlocked = true;
            }
            return true;
        }

    }
}
