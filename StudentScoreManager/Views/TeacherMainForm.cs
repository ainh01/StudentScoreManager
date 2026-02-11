using System;
using System.Drawing;
using System.Windows.Forms;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class TeacherMainForm : Form
    {
        private Panel pnlContent;
        private MenuStrip menuStrip;

        public TeacherMainForm()
        {
            InitializeComponents();
            this.Load += TeacherMainForm_Load;
        }

        private void InitializeComponents()
        {
            this.Text = "Giáo viên - PHẦN MỀM QUẢN LÝ ĐIỂM";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Padding = new Padding(10, 5, 0, 5)
            };

            ToolStripMenuItem mnuManageScores = new ToolStripMenuItem
            {
                Text = "📝 Quản Lý Điểm",
                ForeColor = Color.White
            };
            mnuManageScores.Click += MnuManageScores_Click;

            ToolStripMenuItem mnuAnalyzeScores = new ToolStripMenuItem
            {
                Text = "📊 Phân Tích Điểm",
                ForeColor = Color.White
            };
            mnuAnalyzeScores.Click += MnuAnalyzeScores_Click;

            ToolStripMenuItem mnuViewStudent = new ToolStripMenuItem
            {
                Text = "👨‍🎓 Xem Điểm Học Sinh",
                ForeColor = Color.White,
                Visible = SessionManager.IsAdmin()
            };
            mnuViewStudent.Click += MnuViewStudent_Click;

            ToolStripMenuItem mnuLogout = new ToolStripMenuItem
            {
                Text = "🚪 Đăng Xuất",
                ForeColor = Color.White,
                Alignment = ToolStripItemAlignment.Right
            };
            mnuLogout.Click += MnuLogout_Click;

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                mnuManageScores,
                mnuAnalyzeScores,
                mnuViewStudent,
                mnuLogout
            });

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

        private void TeacherMainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Giáo Viên - Xin Chào, {SessionManager.DisplayName} ({SessionManager.RoleName})";
            LoadManageScoresForm();
        }

        private void MnuManageScores_Click(object sender, EventArgs e)
        {
            LoadManageScoresForm();
        }

        private void LoadManageScoresForm()
        {
            pnlContent.Controls.Clear();

            ManageScoreForm manageForm = new ManageScoreForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            pnlContent.Controls.Add(manageForm);
            manageForm.Show();
        }

        private void MnuAnalyzeScores_Click(object sender, EventArgs e)
        {
            pnlContent.Controls.Clear();

            AnalyzeScoreForm analyzeForm = new AnalyzeScoreForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            pnlContent.Controls.Add(analyzeForm);
            analyzeForm.Show();
        }

        private void MnuViewStudent_Click(object sender, EventArgs e)
        {
            pnlContent.Controls.Clear();

            StudentScoreViewForm studentForm = new StudentScoreViewForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            pnlContent.Controls.Add(studentForm);
            studentForm.Show();
        }

        private void MnuLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có muốn Đăng xuất?",
                "Đăng xuất",
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
