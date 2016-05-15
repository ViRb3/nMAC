using Android.App;
using Android.Content;
using Android.Text.Method;
using Android.Widget;
using Environment = System.Environment;

namespace nMAC
{
    public class Logger
    {
        private TextView _txtLog;

        public Logger(Context context)
        {
            _txtLog = ((Activity) context).FindViewById<TextView>(Resource.Id.txtLog);
            ClearLog();
            _txtLog.MovementMethod = new ScrollingMovementMethod();
        }
        public void Log(string text)
        {
            _txtLog.Text += text + Environment.NewLine;
        }

        public void ClearLog()
        {
            _txtLog.Text = string.Empty;
        }
    }
}