using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views.InputMethods;
using EU.Chainfire.Libsuperuser;
using Java.Lang;

namespace nMAC.Utils
{
    internal static class General
    {
        internal static Logger Logger;

        internal static async Task ToggleAirplaneMode(Context context, bool state)
        {
            IList<string> commands = new List<string>
            {
                $"settings put global airplane_mode_on {Convert.ToInt32(state)}",
                $"am broadcast -a android.intent.action.AIRPLANE_MODE --ez state {Convert.ToInt32(state)}"
            };

            await Task.Run(() => Shell.SU.Run(commands));
        }

        internal static bool IsAirplaneModeEnabled(Context context)
        {
            return Convert.ToBoolean(Settings.System.GetInt(context.ContentResolver, Settings.Global.AirplaneModeOn));
        }

        internal static void ShowCriticalError(Context context, string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle("Error");
            builder.SetMessage(message);
            builder.SetPositiveButton("Exit", (sender, args) => JavaSystem.Exit(0));
            builder.SetCancelable(false);
            builder.Show();
        }

        internal static void ShowMessage(Context context, string message, string title = "Error")
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (sender, args) => { });
            builder.SetCancelable(true);
            builder.Show();
        }

        internal static void InitializeLogger(Context context)
        {
            Logger = new Logger(context);
        }

        internal static void Log()
        {
            Logger.Log(string.Empty);
        }

        internal static void Log(string text)
        {
            Logger.Log(text);
        }

        internal static async Task<bool> CheckSUStrict(Context context)
        {
            bool isSU = false;
            await Task.Run(() => isSU = Shell.SU.Available());

            if (!isSU)
                ShowCriticalError(context, "This application requires Super User (root) access in order to work!");

            return isSU;
        }

        internal static void ToggleKeyboard(Context context, bool state)
        {
            InputMethodManager inputManager = (InputMethodManager)
                context.GetSystemService(Context.InputMethodService);

            inputManager.HideSoftInputFromWindow(((Activity) context).CurrentFocus?.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private static ProgressDialogFragment _progressDialog = null;

        internal static bool ShowLoading(Context context, string message)
        {
            ((Activity) context).RunOnUiThread(() =>
            {
                _progressDialog = new ProgressDialogFragment();
                _progressDialog.Initialize(context, message, string.Empty);
                _progressDialog.Show();
            });

            return true;
        }

        internal static bool DismissLoading()
        {
            if (_progressDialog == null)
                return false;

            _progressDialog.Dismiss();
            _progressDialog = null;
            return true;
        }
    }
}