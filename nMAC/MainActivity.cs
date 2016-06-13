using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using static nMAC.Helpers;
using static nMAC.MACFunctions;

namespace nMAC
{
    [Activity(Label = "Nil MAC Changer", MainLauncher = true, Icon = "@mipmap/ic_launcher", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private readonly Random _random = new Random();

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(Resource.Layout.Main);
            ToggleViews(false);

            Button btnRandomize = this.FindViewById<Button>(Resource.Id.btnRandomize);
            btnRandomize.Click += BtnRandomize_Click;
            Button btnChange = this.FindViewById<Button>(Resource.Id.btnChange);
            btnChange.Click += BtnChange_Click;
            Button btnRestore = this.FindViewById<Button>(Resource.Id.btnRestore);
            btnRestore.Click += BtnRestore_Click;

            InitializeLogger(this);
            Log("Checking SU availability...");

            if (!await CheckSUStrict(this))
            {
                Log("Error confirming SU access!");
                return;
            }

            await AssignPaths(this);

            if (MACFile == null) // device not supported
            {
                Log("Device not supported!");
                return;
            }

            Log("Done");

            CheckBackup();
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            ToggleViews(false);
            ToggleKeyboard(this, false);
            Log("Restoring MAC Address...");
            ShowLoading(this, "Please wait...");

            bool disableAirplaneMode = !IsAirplaneModeEnabled(this);

            await ToggleAirplaneMode(this, true);

            await ReplaceMACFile(this, BackupMACFile);

            if (disableAirplaneMode)
                await ToggleAirplaneMode(this, false);

            DismissLoading();
            Log("Done");
            LoadMAC();
            ToggleViews(true);
        }

        private async void BtnChange_Click(object sender, EventArgs e)
        { 
            string newMAC = GetMACFromViews();

            if (newMAC.Length != 12)
            {
                ShowMessage(this, "Invalid MAC address entered!");
                return;
            }

            ToggleViews(false);
            ToggleKeyboard(this, false);
            ShowLoading(this, "Please wait...");

            Log("Changing MAC address...");

            bool disableAirplaneMode = !IsAirplaneModeEnabled(this);
            await ToggleAirplaneMode(this, true);

            byte[] content = File.ReadAllBytes(LocalMACFile);

            WriteMAC(ref content, newMAC);
            File.WriteAllBytes(LocalMACFile, content);

            await ReplaceMACFile(this, LocalMACFile);

            if (disableAirplaneMode)
                await ToggleAirplaneMode(this, false);

            DismissLoading();
            Log("Done");
            Log();
            ToggleViews(true);
        }

        private void BtnRandomize_Click(object sender, EventArgs e)
        {    
            int MAC1 = _random.Next(0x111111, 0xffffff); // 6/12
            int MAC2 = _random.Next(0x111111, 0xffffff); // 12/12
            SetMACViews(MAC1.ToString("X") + MAC2.ToString("X"), true);
        }

        private void ToggleViews(bool state)
        {
            RelativeLayout layoutMAC = this.FindViewById<RelativeLayout>(Resource.Id.layoutMAC);

            for (int i = 0; i < layoutMAC.ChildCount; i++)
                layoutMAC.GetChildAt(i).Enabled = state;

            RelativeLayout layoutButtons = this.FindViewById<RelativeLayout>(Resource.Id.layoutButtons);

            for (int i = 0; i < layoutButtons.ChildCount; i++)
                layoutButtons.GetChildAt(i).Enabled = state;
        }    

        private void CheckBackup()
        {
            Log("Checking for backup file...");

            if (File.Exists(BackupMACFile))
            {
                Log("Done");
                LoadMAC();
                return;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Welcome");
            builder.SetMessage(@"It seems like this is the first time you are using nMAC.

To be able to revert anything you do here, a backup of your current MAC binary file will be created. You can find it in /.nMAC/ even after you uninstall this application.");
            builder.SetPositiveButton("Create Backup", btnBackupOkHandler);
            builder.SetCancelable(false);
            builder.Show();
        }

        private async void btnBackupOkHandler(object sender, DialogClickEventArgs dialogClickEventArgs)
        {
            await BackupMACBinary();

            if (!File.Exists(BackupMACFile))
            {
                ShowCriticalError(this, "Error creating backup file!");
                return;
            }

            Log("Backup file created");
            LoadMAC();
        }

        private async void LoadMAC()
        {
            Log("Loading current MAC address...");

            await GetMACFile(this);
            string MAC = ReadMAC(this);

            if (MAC == null) // device not supported
            {
                Log("Device not supported!");
                return;
            }

            SetMACViews(MAC);

            Log("Done");
            Log();
            ToggleViews(true);
        }

        private void SetMACViews(string MAC, bool excludeFirst = false)
        {
            string[] MACArray = new string[6];

            string @byte = string.Empty;

            for (int i = 0, u = 0; i < MAC.Length; i++)
            {
                @byte += MAC[i];
                if (@byte.Length < 2)
                    continue;
                else
                {
                    MACArray[u++] = @byte;
                    @byte = string.Empty;
                }
            }

            RelativeLayout layoutMAC = this.FindViewById<RelativeLayout>(Resource.Id.layoutMAC);

            for (int i = excludeFirst ? 3 : 0; i < layoutMAC.ChildCount; i++)
            {
                EditText editMAC = (EditText)layoutMAC.GetChildAt(i);
                editMAC.Text = MACArray[i];
                editMAC.Hint = MACArray[i];
            }
        }

        private string GetMACFromViews()
        {
            RelativeLayout layoutMAC = this.FindViewById<RelativeLayout>(Resource.Id.layoutMAC);
            StringWriter stringWriter = new StringWriter();

            for (int i = 0; i < layoutMAC.ChildCount; i++)
            {
                EditText editMAC = (EditText) layoutMAC.GetChildAt(i);
                if (editMAC.Text == string.Empty)
                    editMAC.Text = editMAC.Hint;

                stringWriter.Write(editMAC.Text);
            }

            return stringWriter.ToString();
        }
    }
}

