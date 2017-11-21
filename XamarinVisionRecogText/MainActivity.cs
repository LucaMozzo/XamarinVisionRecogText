using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Com.Microsoft.Projectoxford.Vision;
using Android.Graphics;
using System.IO;
using System;
using Com.Microsoft.Projectoxford.Vision.Contract;
using GoogleGson;
using XamarinVisionRecogText.Model;
using Newtonsoft.Json;
using Java.Lang;

namespace XamarinVisionRecogText
{
    [Activity(Label = "XamarinVisionRecogText", MainLauncher = true, Icon = "@drawable/icon",Theme ="@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        public VisionServiceRestClient visionServiceClient = new VisionServiceRestClient("ac585835001b490a941d07984f938e77");

        private Bitmap mBitmap;
        private ImageView imageView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            mBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.abc);
            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            imageView.SetImageBitmap(mBitmap);

            Button btnProcess = FindViewById<Button>(Resource.Id.btnProcess);

            //Convert bitmap to stream
            byte[] data;
            using (var stream = new MemoryStream())
            {
                mBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                data = stream.ToArray();
            }
            Stream inputStream = new MemoryStream(data);

            btnProcess.Click += delegate {

                new RecognizeTextTask(this).Execute(inputStream);
            };


        }
    }

    internal class RecognizeTextTask:AsyncTask<Stream,string,string>
    {
        private MainActivity mainActivity;
        ProgressDialog mDialog = new ProgressDialog(Application.Context);

        public RecognizeTextTask(MainActivity mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        protected override string RunInBackground(params Stream[] @params)
        {
            try
            {
                PublishProgress("Recognizing...");
                OCR ocr = mainActivity.visionServiceClient.RecognizeText(@params[0], LanguageCodes.English, true);

                return new Gson().ToJson(ocr);
            }
            catch(Java.Lang.Exception ex)
            {
                return null;
            }
        }

        protected override void OnPreExecute()
        {
            mDialog.Window.SetType(Android.Views.WindowManagerTypes.SystemAlert);
            mDialog.Show();
        }

        protected override void OnProgressUpdate(params string[] values)
        {
            mDialog.SetMessage(values[0]);
        }

        protected override void OnPostExecute(string result)
        {
            mDialog.Dismiss();

            OCRModel ocrModel = JsonConvert.DeserializeObject<OCRModel>(result);

            TextView txtDes = mainActivity.FindViewById<TextView>(Resource.Id.txtDescription);
            StringBuilder strBuilder = new StringBuilder();

            foreach(var region in ocrModel.regions)
            {
                foreach(var line in region.lines)
                {
                    foreach (var word in line.words)
                        strBuilder.Append(word.text + " ");
                    strBuilder.Append("\n");
                }
                strBuilder.Append("\n\n");
            }
            txtDes.Text = strBuilder.ToString();
        }
    }
}

