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
    private CascadeClassifier? handCascade;
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

    private string GetCascadePath(string fileName)
    {
        string path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        if (File.Exists(path1))
        {
            return path1;
        }

        string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", fileName);
        if (File.Exists(path2))
        {
            return Path.GetFullPath(path2);
        }

        if (File.Exists(fileName))
        {
            return Path.GetFullPath(fileName);
        }

        return string.Empty;
    }

    private void btnStartCamera_Click(object? sender, EventArgs e)
    {
        if (isCameraRunning)
        {
            return;
        }

        string facePath = GetCascadePath("haarcascade_frontalface_default.xml");
        if (string.IsNullOrEmpty(facePath))
        {
            MessageBox.Show("Yüz algılama dosyası (haarcascade_frontalface_default.xml) bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatus.Text = "Durum: Yüz modeli bulunamadı.";
            return;
        }

        try
        {
            if (faceCascade == null)
            {
                faceCascade = new CascadeClassifier(facePath);
            }

            string handPath = GetCascadePath("palm.xml");
            if (!string.IsNullOrEmpty(handPath) && handCascade == null)
            {
                handCascade = new CascadeClassifier(handPath);
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
            lblStatus.Text = "Durum: Kamera aktif (Aynalı görüntü, yüz/el algılama ve portre modu)";
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
                // 1. Görüntüyü yatay olarak aynala
                Cv2.Flip(currentFrame, currentFrame, FlipMode.Y);

                if (grayFrame != null)
                {
                    Cv2.CvtColor(currentFrame, grayFrame, ColorConversionCodes.BGR2GRAY);

                    Rect[] faces = Array.Empty<Rect>();
                    if (faceCascade != null)
                    {
                        faces = faceCascade.DetectMultiScale(grayFrame, 1.1, 4, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));
                    }

                    // 2. Arka Planı Bulanıklaştırma (Portre Modu)
                    if (chkBlurBackground.Checked && faces.Length > 0)
                    {
                        Rect mainFace = faces[0];

                        using Mat blurred = new Mat();
                        Cv2.GaussianBlur(currentFrame, blurred, new OpenCvSharp.Size(51, 51), 0);

                        using Mat mask = new Mat(currentFrame.Size(), MatType.CV_8UC1, Scalar.All(0));

                        // Baş bölgesi elipsi
                        OpenCvSharp.Point headCenter = new OpenCvSharp.Point(mainFace.X + mainFace.Width / 2, mainFace.Y + mainFace.Height / 2);
                        OpenCvSharp.Size headAxes = new OpenCvSharp.Size((int)(mainFace.Width * 0.75), (int)(mainFace.Height * 0.95));
                        Cv2.Ellipse(mask, headCenter, headAxes, 0, 0, 360, Scalar.All(255), -1);

                        // Omuz ve gövde bölgesi
                        OpenCvSharp.Point[] bodyPoly = new OpenCvSharp.Point[]
                        {
                            new OpenCvSharp.Point(Math.Max(0, mainFace.X - (int)(mainFace.Width * 0.9)), currentFrame.Height),
                            new OpenCvSharp.Point(mainFace.X, mainFace.Bottom),
                            new OpenCvSharp.Point(mainFace.Right, mainFace.Bottom),
                            new OpenCvSharp.Point(Math.Min(currentFrame.Width, mainFace.Right + (int)(mainFace.Width * 0.9)), currentFrame.Height)
                        };
                        Cv2.FillConvexPoly(mask, bodyPoly, Scalar.All(255));

                        // Maske kenarlarını yumuşat
                        Cv2.GaussianBlur(mask, mask, new OpenCvSharp.Size(35, 35), 0);

                        // Net kişiyi bulanık arka planın üzerine kopyala
                        currentFrame.CopyTo(blurred, mask);
                        blurred.CopyTo(currentFrame);
                    }

                    // 3. Yüz çerçevesi çizimi
                    foreach (Rect face in faces)
                    {
                        Cv2.Rectangle(currentFrame, face, Scalar.Green, 2);
                        Cv2.PutText(currentFrame, "Yuz", new OpenCvSharp.Point(face.X, Math.Max(20, face.Y - 6)), HersheyFonts.HersheySimplex, 0.6, Scalar.Green, 2);
                    }

                    // 4. El / Avuç Tespiti
                    if (handCascade != null)
                    {
                        Rect[] hands = handCascade.DetectMultiScale(grayFrame, 1.15, 3, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(40, 40));
                        foreach (Rect hand in hands)
                        {
                            bool overlapsFace = false;
                            foreach (Rect face in faces)
                            {
                                if (hand.IntersectsWith(face))
                                {
                                    overlapsFace = true;
                                    break;
                                }
                            }

                            if (!overlapsFace)
                            {
                                Cv2.Rectangle(currentFrame, hand, Scalar.Cyan, 2);
                                Cv2.PutText(currentFrame, "El", new OpenCvSharp.Point(hand.X, Math.Max(20, hand.Y - 6)), HersheyFonts.HersheySimplex, 0.6, Scalar.Cyan, 2);
                            }
                        }
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
        handCascade?.Dispose();
    }
}
