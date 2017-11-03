using Android.App;
using Android.Content;
using Android.OS;

namespace nMAC
{
    internal class ProgressDialogFragment : DialogFragment
    {
        private static string _message, _title;
        private static ProgressDialog _progressDialog;
        private static Context _context;

        private bool _cancelable;

        internal void Initialize(Context context, string message, string title, bool cancelable = false)
        {
            _context = context;
            _message = message;
            _title = title;
            _cancelable = cancelable;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            _progressDialog = new ProgressDialog(_context);

            _progressDialog.SetTitle(_title);
            _progressDialog.SetMessage(_message);
            _progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            _progressDialog.SetCancelable(_cancelable);
            _progressDialog.SetCanceledOnTouchOutside(_cancelable);
            _progressDialog.Show();
            return _progressDialog;
        }

        public override void Dismiss()
        {
            _progressDialog.Dismiss();
        }

        internal void Show()
        {
            this.Show(((Activity) _context).FragmentManager, string.Empty);
        }
    }
}