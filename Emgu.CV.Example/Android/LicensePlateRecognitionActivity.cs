//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Preferences;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using LicensePlateRecognition;
using System.IO;

namespace AndroidExamples
{
    [Activity(Label = "License Plate Recognition")]
    public class LicensePlateRecognitionActivity : ButtonMessageImageActivity
    {
        public LicensePlateRecognitionActivity()
           : base("Detect License Plate")
        {
        }
        /*
        private static void TesseractDownloadLangFile(String folder, String lang)
        {
            String subfolderName = "tessdata";
            String folderName = System.IO.Path.Combine(folder, subfolderName);
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
            }
            String dest = System.IO.Path.Combine(folderName, String.Format("{0}.traineddata", lang));
            if (!System.IO.File.Exists(dest))
                using (System.Net.WebClient webclient = new System.Net.WebClient())
                {
                    String source = Emgu.CV.OCR.Tesseract.GetLangFileUrl(lang);
                        
                    Console.WriteLine(String.Format("Downloading file from '{0}' to '{1}'", source, dest));
                    webclient.DownloadFile(source, dest);
                    Console.WriteLine(String.Format("Download completed"));
                }
        }*/

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            OnImagePicked += (sender, image) =>
            {
                if (image == null)
                    return;

                String path = System.IO.Path.Combine(this.FilesDir.AbsolutePath, "tessdata");

                try
                {
                    SetProgressMessage("Checking Tesseract Lang files...");
                    LicensePlateDetector.TesseractDownloadLangFile(path, "eng");
                    LicensePlateDetector.TesseractDownloadLangFile(path, "osd");
                    SetProgressMessage("Please wait ...");
                }
                catch (System.Net.WebException e)
                {
                    SetMessage("Unable to download tesseract language file from Internet, please check your Internet connection.");
                    Console.WriteLine(e);
                    return;
                }
                catch (Exception e)
                {
                    SetMessage(e.ToString());
                    Console.WriteLine(e);
                    return;
                }


                LicensePlateDetector detector = new LicensePlateDetector(path + System.IO.Path.DirectorySeparatorChar);


                Stopwatch watch = Stopwatch.StartNew(); // time the detection process

                List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
                List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
                List<RotatedRect> licenseBoxList = new List<RotatedRect>();
                List<string> words = detector.DetectLicensePlate(
                image,
                licensePlateImagesList,
                filteredLicensePlateImagesList,
                licenseBoxList);

                watch.Stop(); //stop the timer

                StringBuilder builder = new StringBuilder();
                builder.Append(String.Format("{0} milli-seconds. ", watch.Elapsed.TotalMilliseconds));
                foreach (String w in words)
                    builder.AppendFormat("{0} ", w);
                SetMessage(builder.ToString());

                foreach (RotatedRect box in licenseBoxList)
                {
                    Rectangle rect = box.MinAreaRect();
                    CvInvoke.Rectangle(image, rect, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                }

                SetImageBitmap(image.ToBitmap());
                image.Dispose();
            };


            OnButtonClick += delegate
            {
                PickImage("license-plate.jpg");

            };
        }
    }
}