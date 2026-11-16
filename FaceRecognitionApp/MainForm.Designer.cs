namespace FaceRecognitionApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Button btnStartCamera;
    private Button btnStopCamera;
    private CheckBox chkBlurBackground;
    private PictureBox picCamera;
    private Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        btnStartCamera = new Button();
        btnStopCamera = new Button();
        chkBlurBackground = new CheckBox();
        picCamera = new PictureBox();
        lblStatus = new Label();
        ((System.ComponentModel.ISupportInitialize)picCamera).BeginInit();
        SuspendLayout();

        // btnStartCamera
        btnStartCamera.Location = new Point(16, 14);
        btnStartCamera.Size = new Size(120, 32);
        btnStartCamera.Text = "Kamerayı Aç";
        btnStartCamera.UseVisualStyleBackColor = true;

        // btnStopCamera
        btnStopCamera.Location = new Point(144, 14);
        btnStopCamera.Size = new Size(120, 32);
        btnStopCamera.Text = "Kamerayı Kapat";
        btnStopCamera.UseVisualStyleBackColor = true;
        btnStopCamera.Enabled = false;

        // chkBlurBackground
        chkBlurBackground.AutoSize = true;
        chkBlurBackground.Checked = true;
        chkBlurBackground.CheckState = CheckState.Checked;
        chkBlurBackground.Location = new Point(280, 21);
        chkBlurBackground.Size = new Size(220, 19);
        chkBlurBackground.Text = "Arka Planı Bulanıklaştır (Portre Modu)";
        chkBlurBackground.UseVisualStyleBackColor = true;

        // picCamera
        picCamera.BackColor = Color.Black;
        picCamera.BorderStyle = BorderStyle.FixedSingle;
        picCamera.Location = new Point(16, 56);
        picCamera.Size = new Size(640, 480);
        picCamera.SizeMode = PictureBoxSizeMode.Zoom;
        picCamera.TabStop = false;

        // lblStatus
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(16, 546);
        lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblStatus.ForeColor = Color.DarkSlateGray;
        lblStatus.Text = "Durum: Kamera kapalı";

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(674, 576);
        Controls.Add(lblStatus);
        Controls.Add(picCamera);
        Controls.Add(chkBlurBackground);
        Controls.Add(btnStopCamera);
        Controls.Add(btnStartCamera);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Yüz Tanıma Uygulaması (Face Recognition)";
        ((System.ComponentModel.ISupportInitialize)picCamera).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
