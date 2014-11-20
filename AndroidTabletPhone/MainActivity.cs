using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using TwilioClient.Android;
using System.Net.Http;
using Android.Util;
using System.Collections.Generic;

namespace AndroidTabletPhone
{
	[Activity (Label = "AndroidTabletPhone", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, Twilio.IInitListener
	{
		private Device _device;
		private IConnection _connection;
		private const string TAG = "AndroidTabletPhone";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			Twilio.Initialize (this.ApplicationContext, this);

			// Get our button from the layout resource,
			// and attach an event to it
			Button call = FindViewById<Button> (Resource.Id.callButton);
			EditText phoneEditText = FindViewById<EditText> (Resource.Id.phoneNumberEditText);
			call.Click += delegate {
				var parameters = new Dictionary<string, string> () {
					{ "Target", phoneEditText.Text },
					{ "Source", "+12152407664" }          
				};

				_connection = _device.Connect (parameters, null);
			};

			Button anyKey = FindViewById<Button> (Resource.Id.anyKeyButton);
			anyKey.Click += delegate {
				if (_connection != null && _connection.State == ConnectionState.Connected)
				{
					_connection.SendDigits("1");
				}
			};

Button hangup = FindViewById<Button> (Resource.Id.hangupButton);
hangup.Click += delegate {
	_connection.Disconnect();
};
		}

		public void OnError (Java.Lang.Exception p0)
		{
		}

		public async void OnInitialized ()
		{
			try {
				var client = new HttpClient ();
				var token = await client.GetStringAsync ("http://twilioclientazure.azurewebsites.net/Client/Token?clientName=xamarin");

				_device = Twilio.CreateDevice(token, null);

				var intent  = new Intent(this.ApplicationContext, typeof(MainActivity));
				var pendingIntent = PendingIntent.GetActivity(this.ApplicationContext, 0, intent, PendingIntentFlags.UpdateCurrent);

				_device.SetIncomingIntent(pendingIntent);

			} catch (Exception ex) {
				Log.Info(TAG, "Error: " + ex.Message);
			}
		}

		protected override void OnNewIntent (Intent intent)
		{
			base.OnNewIntent (intent);
			this.Intent = intent;
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			var intent = this.Intent;
			var device = intent.GetParcelableExtra(Device.ExtraDevice) as Device;
			var connection = intent.GetParcelableExtra(Device.ExtraConnection).JavaCast<IConnection>();

			if (device != null && connection != null) {
				intent.RemoveExtra(Device.ExtraDevice);
				intent.RemoveExtra(Device.ExtraConnection);
				HandleIncomingConnection(device, connection);
			}
		}

		void HandleIncomingConnection (Device device, IConnection connection)
		{
			if (_connection != null)
				_connection.Disconnect();
			_connection = connection;
			_connection.Accept();
		}
	}
}


