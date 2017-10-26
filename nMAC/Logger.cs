using Android.App;
using Android.Content;
using Android.Text.Method;
using Android.Widget;
using Environment = System.Environment;

namespace nMAC
{
    internal class Logger
    {
        private readonly TextView _txtLog;

        internal Logger(Context context)
        {
            _txtLog = ((Activity) context).FindViewById<TextView>(Resource.Id.txtLog);
            ClearLog();
            _txtLog.MovementMethod = new ScrollingMovementMethod();
        }

        internal void Log(string text)
        {
            _txtLog.Text += text + Environment.NewLine;
        }

        internal void ClearLog()
        {
            _txtLog.Text = string.Empty;
        }
    }
}