using System;
using System.Drawing;
using System.Windows.Forms;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class StudentMainForm : Form
    {
        private Panel pnlContent;
        private MenuStrip menuStrip;

        public StudentMainForm()
        {
            InitializeComponent();
            this.Load += StudentMainForm_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Cổng Học Sinh - Hệ Thống Quản Lý Điểm";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(10, 5, 0, 5)
            };

            ToolStripMenuItem mnuScores = new ToolStripMenuItem
            {
                Text = "📊 Điểm Của Tôi",
                ForeColor = Color.White
            };
            mnuScores.Click += MnuScores_Click;

            ToolStripMenuItem mnuProfile = new ToolStripMenuItem
            {
                Text = "👤 Hồ Sơ",
                ForeColor = Color.White
            };
            mnuProfile.Click += MnuProfile_Click;

            ToolStripMenuItem mnuLogout = new ToolStripMenuItem
            {
                Text = "🚪 Đăng Xuất",
                ForeColor = Color.White,
                Alignment = ToolStripItemAlignment.Right
            };
            mnuLogout.Click += MnuLogout_Click;

            menuStrip.Items.AddRange(new ToolStripItem[] { mnuScores, mnuProfile, mnuLogout });

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void StudentMainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Cổng Học Sinh - Xin Chào, {SessionManager.DisplayName}";
            LoadScoresForm();
        }

        private void MnuScores_Click(object sender, EventArgs e)
        {
            LoadScoresForm();
        }

        private void LoadScoresForm()
        {
            pnlContent.Controls.Clear();

            StudentScoreViewForm scoreForm = new StudentScoreViewForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            pnlContent.Controls.Add(scoreForm);
            scoreForm.Show();
        }

        private void MnuProfile_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"Thông Tin Học Sinh\n\n" +
                $"Tên: {SessionManager.DisplayName}\n" +
                $"Tên Đăng Nhập: {SessionManager.Username}\n" +
                $"Quyền: {SessionManager.RoleName}\n" +
                $"ID Học Sinh: {SessionManager.GetStudentId()}",
                "Thông Tin Học Sinh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void MnuLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có muốn Đăng xuất?",
                "Đăng Xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
