using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jai_FactoryDotNET;

//using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace ImageSave_GLv1._0
{
    public partial class mainForm : Form
    {
        CFactory myFactory = new CFactory();
        CCamera myCamera;
        string selectedPath;
        // Bitmap image;
        String timestmp_folder = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
        CNode myNode;
        Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;
        int counter = 0;
        int timeCounter = 0;
        string pastepath;
        string sourceF;
        string destF;
    

        public mainForm()
        {
            InitializeComponent();
        }
        // Exposure Mode
        // Exposure Time (us)
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            labelInfo.Text = "Searching for camera...";
            try
            {
                error = myFactory.Open("");
                cameraSearch();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Camera not found, please make sure the connection is working.");
                Application.Exit();
            }

            textBoxWidth.Text = "1000";
            textBoxHeight.Text = "800";
            textBoxOffsetX.Text = "600";
            textBoxOffsetY.Text = "500";
            buttonBrowse.Enabled = true;
            buttonSearch.Enabled = false;

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath;
            }
            else
            {
                selectedPath = "C:\\Users\\plc-user\\Documents\\Levente\\Krist";
                pastepath = "C:\\Users\\plc-user\\Documents\\Levente\\Krist\\Images";
            }
          
            labelInfo.Text = "The pictures will be saved to: ";
            labelPath.Text = selectedPath;
            


            labelPath.Visible = true;
            buttonStart.Enabled = true;


                textBoxExpTime.Text = Convert.ToString(myCamera.GetNode("ExposureTime").Value);
            

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            cameraStart();

            buttonTrigger.Enabled = true;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            cameraStop();

            buttonTrigger.Enabled = false;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }


        private void cameraStart()
        {
            if (myCamera != null)
            {
                CNode nodeAcquisitionMode = myCamera.GetNode("AcquisitionMode");
                if (null != nodeAcquisitionMode)
                {
                    nodeAcquisitionMode.Value = "Continuous";

                    myCamera.AcquisitionCount = UInt32.MaxValue;
                }



                myCamera.GetNode("Width").Value = Convert.ToInt32(textBoxWidth.Text);
                myCamera.GetNode("Height").Value = Convert.ToInt32(textBoxHeight.Text);
                myCamera.GetNode("OffsetX").Value = Convert.ToInt32(textBoxOffsetX.Text);
                myCamera.GetNode("OffsetY").Value = Convert.ToInt32(textBoxOffsetY.Text);

                labelAddInfo.Visible = true;

                if (radioButtonExpModeTimed.Checked)
                {
                    myCamera.GetNode("ExposureMode").Value = "Timed";
                    myCamera.GetNode("ExposureTime").Value = Convert.ToDouble(textBoxExpTime.Text);
                }

                myCamera.StartImageAcquisition(true, 5);
               // myCamera.NewImageDelegate += new Jai_FactoryWrapper.ImageCallBack(imageSaver);
                timer1.Start();

            }
        }

        private void cameraStop()
        {
            myCamera.StopImageAcquisition();
            // myCamera.NewImageDelegate -= new Jai_FactoryWrapper.ImageCallBack(imageSaver);
            timer1.Stop();
        }

        private void cameraSearch()
        {
            if (null != myCamera)
            {
                if (myCamera.IsOpen)
                {
                    myCamera.Close();
                }

                myCamera = null;
            }

            myFactory.UpdateCameraList(Jai_FactoryDotNET.CFactory.EDriverType.FilterDriver);
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myCamera = myFactory.CameraList[i];
                if (Jai_FactoryWrapper.EFactoryError.Success == myCamera.Open())
                {
                    break;
                }
            }

            if (null != myCamera && myCamera.IsOpen)
            {
                textBox.Text = myCamera.CameraID;
                labelInfo.Text = "Camera found!";

               // int currentValue = 0;
            }
            else
            {
                labelInfo.Text = "No camera found!";
            }

        }

        unsafe
        void imageSaver(ref Jai_FactoryWrapper.ImageInfo ImageInfo)
        {
            counter += 1;
            
            Bitmap image = GetBitmap((int)ImageInfo.SizeX, (int)ImageInfo.SizeY, 8, (byte*)ImageInfo.ImageBuffer);
            String timestmp = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");

            image.Save(selectedPath + "\\" + timestmp + "_L.jpg", ImageFormat.Jpeg);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (IsDirectoryEmpty(pastepath)) {
                String timestmp = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
                myCamera.SaveLastFrame(selectedPath + "\\" + timestmp + "_L.jpg", Jai_FactoryWrapper.ESaveFileFormat.Jpeg, 100);
                labelAddInfo.Text = "Saving picture: " + timestmp + "_L.jpg";
                sourceF = selectedPath + "\\" + timestmp + "_L.jpg";
                destF = pastepath + "\\" + timestmp + "_L.jpg";
                this.Move(sourceF, destF);
            }
        }

        unsafe
        public Bitmap GetBitmap(int nWidth, int nHeight, int nBpp, byte* DataColor)
        {
            Bitmap BitMapImage = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);
            BitmapData srcBmpData = BitMapImage.LockBits(new Rectangle(0, 0, BitMapImage.Width, BitMapImage.Height),
            ImageLockMode.ReadWrite, BitMapImage.PixelFormat);

            switch (BitMapImage.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    unsafe
                    {
                        byte* psrcBuffer = (byte*)srcBmpData.Scan0.ToPointer();

                        int nCount = srcBmpData.Width * srcBmpData.Height;
                        int nIndex = 0;

                        for (int y = 0; y < nCount; y++)
                        {
                            psrcBuffer[nIndex++] = DataColor[y];
                            psrcBuffer[nIndex++] = DataColor[y];
                            psrcBuffer[nIndex++] = DataColor[y];
                        }
                    }
                    break;
            }

            BitMapImage.UnlockBits(srcBmpData);

            return BitMapImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            String timestmp = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
            myCamera.SaveLastFrame(selectedPath + "\\" + timestmp + "_L.jpg", Jai_FactoryWrapper.ESaveFileFormat.Jpeg, 100);
        //    this.Copy(selectedPath, pastepath);
            
        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        void Copy(string sourceFile, string destinationFile)
        {
                File.Copy(sourceFile,destinationFile);
        }

        void Move(string sourceFile, string destinationFile)
        {
            System.IO.File.Move(sourceFile, destinationFile);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(1);
        }


    }
}
