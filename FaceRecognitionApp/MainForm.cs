using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceRecognitionApp;

public partial class MainForm : Form
{
    private VideoCapture? capture;
    private CascadeClassifier? faceCascade;
    private System.Windows.Forms.Timer? cameraTimer;
    private Mat? currentFrame;
    private Mat? grayFrame;
    private bool isCameraRunning = false;

    public MainForm()
    {
        InitializeComponent();

        btnStartCamera.Click += btnStartCamera_Click;
        btnStopCamera.Click += btnStopCamera_Click;
        FormClosing += MainForm_FormClosing;
    }

    private string GetCascadePath()
    {
        string path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
        if (File.Exists(path1))
        {
            return path1;
        }

        string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "haarcascade_frontalface_default.xml");
        if (File.Exists(path2))
        {
            return Path.GetFullPath(path2);
        }

        if (File.Exists("haarcascade_frontalface_default.xml"))
        {
            return Path.GetFullPath("haarcascade_frontalface_default.xml");
        }

        return string.Empty;
    }

    private void btnStartCamera_Click(object? sender, EventArgs e)
    {
        if (isCameraRunning)
        {
            return;
        }

        string cascadePath = GetCascadePath();
        if (string.IsNullOrEmpty(cascadePath))
        {
            MessageBox.Show("Yüz algılama dosyası (haarcascade_frontalface_default.xml) bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatus.Text = "Durum: Model dosyası bulunamadı.";
            return;
        }

        try
        {
            if (faceCascade == null)
            {
                faceCascade = new CascadeClassifier(cascadePath);
            }

            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("Kamera açılamadı veya bilgisayarda kamera bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblStatus.Text = "Durum: Kamera açılamadı.";
                capture.Dispose();
                capture = null;
                return;
            }

            currentFrame = new Mat();
            grayFrame = new Mat();

            if (cameraTimer == null)
            {
                cameraTimer = new System.Windows.Forms.Timer();
                cameraTimer.Interval = 33;
                cameraTimer.Tick += CameraTimer_Tick;
            }

            cameraTimer.Start();
            isCameraRunning = true;

            btnStartCamera.Enabled = false;
            btnStopCamera.Enabled = true;
            lblStatus.Text = "Durum: Kamera aktif, yüzler taranıyor...";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Kamera başlatılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            StopCamera();
        }
    }

    private void CameraTimer_Tick(object? sender, EventArgs e)
    {
        if (capture == null || !capture.IsOpened() || currentFrame == null)
        {
            return;
        }

        try
        {
            if (capture.Read(currentFrame) && !currentFrame.Empty())
            {
                if (faceCascade != null && grayFrame != null)
                {
                    Cv2.CvtColor(currentFrame, grayFrame, ColorConversionCodes.BGR2GRAY);
                    Rect[] faces = faceCascade.DetectMultiScale(grayFrame, 1.1, 4, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

                    foreach (Rect face in faces)
                    {
                        Cv2.Rectangle(currentFrame, face, Scalar.Green, 2);
                    }
                }

                Bitmap newBitmap = BitmapConverter.ToBitmap(currentFrame);
                var oldImage = picCamera.Image;
                picCamera.Image = newBitmap;
                oldImage?.Dispose();
            }
        }
        catch
        {
        }
    }

    private void btnStopCamera_Click(object? sender, EventArgs e)
    {
        StopCamera();
    }

    private void StopCamera()
    {
        if (cameraTimer != null)
        {
            cameraTimer.Stop();
        }

        if (capture != null)
        {
            try
            {
                capture.Release();
                capture.Dispose();
            }
            catch
            {
            }
            capture = null;
        }

        currentFrame?.Dispose();
        currentFrame = null;

        grayFrame?.Dispose();
        grayFrame = null;

        var oldImage = picCamera.Image;
        picCamera.Image = null;
        oldImage?.Dispose();

        isCameraRunning = false;
        btnStartCamera.Enabled = true;
        btnStopCamera.Enabled = false;
        lblStatus.Text = "Durum: Kamera durduruldu.";
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        StopCamera();
        faceCascade?.Dispose();
    }
}
