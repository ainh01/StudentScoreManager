using System;
using System.Drawing;
using System.Windows.Forms;
using StudentScoreManager.Controllers;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class LoginForm : Form
    {
        private readonly AuthController _authController;

        private int _failedAttempts = 0;
        private const int MAX_ATTEMPTS = 5;

        public LoginForm()
        {
            InitializeComponent();

            _authController = new AuthController();

            this.Load += LoginForm_Load;

            this.AcceptButton = btnLogin;

            ConfigureFormAppearance();
        }

        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblError;
        private PictureBox picLogo;
        private Label lblVersion;
        private Panel pnlBackground;

        private Button btnTogglePassword;
        private CheckBox chkRememberMe;
        private Label lblCapsLock;
        private LinkLabel lnkForgotPassword;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (picLogo.Image != null)
                {
                    picLogo.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlBackground = new Panel();
            this.lblTitle = new Label();
            this.picLogo = new PictureBox();
            this.lblUsername = new Label();
            this.txtUsername = new TextBox();
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.lblError = new Label();
            this.lblVersion = new Label();

            this.btnTogglePassword = new Button();
            this.chkRememberMe = new CheckBox();
            this.lblCapsLock = new Label();
            this.lnkForgotPassword = new LinkLabel();

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();

            this.pnlBackground.BackColor = Color.White;
            this.pnlBackground.Dock = DockStyle.Fill;
            this.pnlBackground.Controls.Add(this.lblTitle);
            this.pnlBackground.Controls.Add(this.picLogo);
            this.pnlBackground.Controls.Add(this.lblUsername);
            this.pnlBackground.Controls.Add(this.txtUsername);
            this.pnlBackground.Controls.Add(this.lblPassword);
            this.pnlBackground.Controls.Add(this.txtPassword);
            this.pnlBackground.Controls.Add(this.btnLogin);
            this.pnlBackground.Controls.Add(this.lblError);
            this.pnlBackground.Controls.Add(this.lblVersion);
            this.pnlBackground.Controls.Add(this.btnTogglePassword);
            this.pnlBackground.Controls.Add(this.chkRememberMe);
            this.pnlBackground.Controls.Add(this.lblCapsLock);
            this.pnlBackground.Controls.Add(this.lnkForgotPassword);

            this.lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(41, 128, 185);
            this.lblTitle.Location = new Point(20, 20);
            this.lblTitle.Size = new Size(360, 30);
            this.lblTitle.Text = "PHẦN MỀM QUẢN LÝ ĐIỂM HỌC SINH";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            this.picLogo.Location = new Point(150, 60);
            this.picLogo.Size = new Size(100, 100);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;

            this.picLogo.BackColor = Color.FromArgb(240, 240, 240);

            this.lblUsername.Font = new Font("Segoe UI", 10F);
            this.lblUsername.Location = new Point(50, 180);
            this.lblUsername.Size = new Size(80, 25);
            this.lblUsername.Text = "Tên đăng nhập:";
            this.lblUsername.TextAlign = ContentAlignment.MiddleLeft;

            this.txtUsername.Font = new Font("Segoe UI", 10F);
            this.txtUsername.Location = new Point(140, 180);
            this.txtUsername.Size = new Size(210, 25);
            this.txtUsername.TabIndex = 0;
            this.txtUsername.MaxLength = 50;
            this.txtUsername.TextChanged += (s, e) => ClearErrorMessage();

            this.lblPassword.Font = new Font("Segoe UI", 10F);
            this.lblPassword.Location = new Point(50, 220);
            this.lblPassword.Size = new Size(80, 25);
            this.lblPassword.Text = "Mật khẩu:";
            this.lblPassword.TextAlign = ContentAlignment.MiddleLeft;

            this.txtPassword.Font = new Font("Segoe UI", 10F);
            this.txtPassword.Location = new Point(140, 220);
            this.txtPassword.Size = new Size(210, 25);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.MaxLength = 128;
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.UseSystemPasswordChar = false;
            this.txtPassword.TextChanged += (s, e) => ClearErrorMessage();

            this.txtPassword.KeyPress += TxtPassword_KeyPress;
            this.txtPassword.KeyDown += TxtPassword_KeyDown;

            this.btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Location = new Point(140, 340);
            this.btnLogin.Size = new Size(120, 35);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "ĐĂNG NHẬP";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Cursor = Cursors.Hand;

            this.btnLogin.Click += BtnLogin_Click;

            this.btnLogin.MouseEnter += (s, e) => {
                btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            };
            this.btnLogin.MouseLeave += (s, e) => {
                btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            };

            this.lblError.Font = new Font("Segoe UI", 9F);
            this.lblError.ForeColor = Color.FromArgb(231, 76, 60);
            this.lblError.Location = new Point(50, 385);
            this.lblError.Size = new Size(300, 40);
            this.lblError.Text = "";
            this.lblError.TextAlign = ContentAlignment.TopCenter;
            this.lblError.Visible = false;

            this.lblVersion.Font = new Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = Color.Gray;
            this.lblVersion.Location = new Point(50, 435);
            this.lblVersion.Size = new Size(300, 20);
            this.lblVersion.Text = "Version 1.2.999 | © 2025 Nhóm thuyết trình đầu";
            this.lblVersion.TextAlign = ContentAlignment.MiddleCenter;

            this.btnTogglePassword.Location = new Point(355, 220);
            this.btnTogglePassword.Size = new Size(30, 25);
            this.btnTogglePassword.Text = "👁";
            this.btnTogglePassword.Font = new Font("Segoe UI", 10F);
            this.btnTogglePassword.FlatStyle = FlatStyle.Flat;
            this.btnTogglePassword.FlatAppearance.BorderSize = 1;
            this.btnTogglePassword.Cursor = Cursors.Hand;
            this.btnTogglePassword.TabIndex = 3;
            this.btnTogglePassword.Click += BtnTogglePassword_Click;

            this.chkRememberMe.Font = new Font("Segoe UI", 9F);
            this.chkRememberMe.Location = new Point(140, 310);
            this.chkRememberMe.Size = new Size(120, 20);
            this.chkRememberMe.Text = "Lưu";
            this.chkRememberMe.TabIndex = 4;

            this.lblCapsLock.Font = new Font("Segoe UI", 8F);
            this.lblCapsLock.ForeColor = Color.Orange;
            this.lblCapsLock.Location = new Point(140, 248);
            this.lblCapsLock.Size = new Size(150, 15);
            this.lblCapsLock.Text = "⚠ Caps Lock đang BẬT";
            this.lblCapsLock.Visible = false;

            this.lnkForgotPassword.Font = new Font("Segoe UI", 9F);
            this.lnkForgotPassword.Location = new Point(265, 310);
            this.lnkForgotPassword.Size = new Size(110, 20);
            this.lnkForgotPassword.Text = "Quên mật khẩu?";
            this.lnkForgotPassword.LinkColor = Color.FromArgb(41, 128, 185);
            this.lnkForgotPassword.TabIndex = 5;
            this.lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;

            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 470);
            this.Controls.Add(this.pnlBackground);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập - Quản Lý Điểm";

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            this.ResumeLayout(false);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            LoadLogoImage();
            LoadRememberedUsername();
            CheckCapsLock();

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                txtUsername.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
            txtUsername.Select();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            ClearErrorMessage();

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Hãy nhập tên đăng nhập.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Hãy nhập mật khẩu.");
                txtPassword.Focus();
                return;
            }

            if (username.Length < 3 || username.Length > 50)
            {
                ShowError("Tên đăng nhập phải từ 3-50 ký tự.");
                txtUsername.Focus();
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";
            this.Cursor = Cursors.WaitCursor;

            bool shouldEnableButton = true;

            try
            {
                var loginResult = _authController.Login(username, password);

                if (loginResult != null)
                {
                    SaveRememberedUsername();

                    if (!SessionManager.IsAuthenticated)
                    {
                        ShowError("Lỗi không tạo Session được. Vui lòng thử lại.");
                        return;
                    }

                    Form mainForm = null;

                    if (SessionManager.IsStudent())
                    {
                        mainForm = new StudentMainForm();
                    }
                    else if (SessionManager.IsTeacher() || SessionManager.IsAdmin())
                    {
                        mainForm = new TeacherMainForm();
                    }
                    else
                    {
                        ShowError("Role không tồn tại. Hãy liên hệ Admin");
                        _authController.Logout();
                        return;
                    }

                    mainForm.Show();
                    this.Hide();

                    mainForm.FormClosed += (s, args) => {
                        _authController.Logout();
                        ResetForm();
                        this.Show();
                    };
                }
                else
                {
                    _failedAttempts++;
                    if (_failedAttempts >= MAX_ATTEMPTS)
                    {
                        ShowError($"Nhập sai quá nhiều. Bị khoá đăng nhập.");
                        shouldEnableButton = false;
                    }
                    else
                    {
                        ShowError($"Sai tên đăng nhập hoặc mật khẩu. Lần thứ {_failedAttempts} sai {MAX_ATTEMPTS} sẽ bị khoá.");
                        txtPassword.Clear();
                        txtUsername.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Lỗi gì đó đã phát sinh. Hãy thử lại.");
            }
            finally
            {
                btnLogin.Enabled = shouldEnableButton;
                btnLogin.Text = "ĐĂNG NHẬP";
                this.Cursor = Cursors.Default;
                CheckCapsLock();
            }
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnLogin_Click(sender, e);
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            CheckCapsLock();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;

            System.Windows.Forms.Timer flashTimer = new System.Windows.Forms.Timer { Interval = 100 };
            int flashCount = 0;
            flashTimer.Tick += (s, e) => {
                flashCount++;
                lblError.BackColor = (flashCount % 2 == 0) ? Color.Transparent : Color.FromArgb(255, 230, 230);
                if (flashCount >= 6)
                {
                    flashTimer.Stop();
                    flashTimer.Dispose();
                    lblError.BackColor = Color.Transparent;
                }
            };
            flashTimer.Start();
        }

        private void ClearErrorMessage()
        {
            if (lblError.Visible)
            {
                lblError.Visible = false;
                lblError.Text = "";
            }
        }

        private void ResetForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            ClearErrorMessage();
            _failedAttempts = 0;
            btnLogin.Enabled = true;
            LoadRememberedUsername();
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                txtUsername.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
            CheckCapsLock();
        }

        private void ConfigureFormAppearance()
        {
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void LoadLogoImage()
        {
            try
            {
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Static", "logo.png");

                if (System.IO.File.Exists(logoPath))
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
                else
                {
                    picLogo.BackColor = Color.FromArgb(240, 240, 240);
                }
            }
            catch (Exception ex)
            {
                picLogo.BackColor = Color.FromArgb(240, 240, 240);
            }
        }




        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '●')
            {
                txtPassword.PasswordChar = '\0';
                btnTogglePassword.Text = "👁";
            }
            else
            {
                txtPassword.PasswordChar = '●';
                btnTogglePassword.Text = "🔒";
            }
        }

        private void CheckCapsLock()
        {
            lblCapsLock.Visible = Control.IsKeyLocked(Keys.CapsLock);
        }

        private void LoadRememberedUsername()
        {
            try
            {
                string savedUsername = Properties.Settings.Default.RememberedUsername;
                bool rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.Checked = true;
                }
                else
                {
                    chkRememberMe.Checked = false;
                }
            }
            catch { }
        }

        private void SaveRememberedUsername()
        {
            try
            {
                if (chkRememberMe.Checked)
                {
                    Properties.Settings.Default.RememberedUsername = txtUsername.Text.Trim();
                    Properties.Settings.Default.RememberMe = true;
                }
                else
                {
                    Properties.Settings.Default.RememberedUsername = string.Empty;
                    Properties.Settings.Default.RememberMe = false;
                }
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void LnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "Hãy liên hệ Admin để thay đổi mật khẩu\n\n" +
                "Email: ainh@xain.click\n" +
                "Phone: 0327-240-273",
                "Lấy lại mật khẩu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}