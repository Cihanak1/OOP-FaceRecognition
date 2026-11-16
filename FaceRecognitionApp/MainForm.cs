using System;
using System.Drawing;
using System.Windows.Forms;

namespace FaceRecognitionApp;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        btnStartCamera.Click += btnStartCamera_Click;
        btnStopCamera.Click += btnStopCamera_Click;
        FormClosing += MainForm_FormClosing;
    }

    private void btnStartCamera_Click(object? sender, EventArgs e)
    {
        lblStatus.Text = "Durum: Kamera başlatılıyor...";
    }

    private void btnStopCamera_Click(object? sender, EventArgs e)
    {
        lblStatus.Text = "Durum: Kamera durduruldu.";
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
    }
}
