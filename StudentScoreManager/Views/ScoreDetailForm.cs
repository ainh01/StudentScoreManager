using System;
using System.Drawing;
using System.Windows.Forms;
using StudentScoreManager.Controllers;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class ScoreDetailForm : Form
    {
        private readonly ScoreController _scoreController;
        private readonly int _studentId;
        private readonly int _subjectId;
        private readonly string _schoolYear;
        private readonly int _semester;
        private readonly string _studentName;

        private TextBox txtQtScore;
        private TextBox txtGkScore;
        private TextBox txtCkScore;
        private TextBox txtFnScore;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;
        private ToolTip toolTip;

        public ScoreDetailForm(int studentId, int subjectId, string schoolYear,
            int semester, string studentName)
        {
            _scoreController = new ScoreController();
            _studentId = studentId;
            _subjectId = subjectId;
            _schoolYear = schoolYear;
            _semester = semester;
            _studentName = studentName;

            InitializeComponent();
            LoadScoreData();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 400);
            this.Text = "Nhập Điểm Học Sinh";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.Icon = SystemIcons.Error;

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;

            Label lblTitle = new Label
            {
                Text = $"Tên Học Sinh: {_studentName}",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };

            GroupBox grpScores = new GroupBox
            {
                Text = "Điểm (0.00 - 10.00)",
                Location = new Point(20, 60),
                Size = new Size(400, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            int yPos = 35;
            int labelX = 30;
            int textBoxX = 200;
            int spacing = 45;

            CreateScoreField(grpScores, "Điểm QT (20%):", ref txtQtScore, labelX, textBoxX, yPos,
                "Nhập điểm Quá Trình (từ 0.00 đến 10.00)");
            yPos += spacing;

            CreateScoreField(grpScores, "Điểm GK (30%):", ref txtGkScore, labelX, textBoxX, yPos,
                "Nhập điểm Giữa Kỳ (từ 0.00 đến 10.00)");
            yPos += spacing;

            CreateScoreField(grpScores, "Điểm CK (50%):", ref txtCkScore, labelX, textBoxX, yPos,
                "Nhập điểm Cuối Kỳ (từ 0.00 đến 10.00)");
            yPos += spacing;

            Label lblFn = new Label
            {
                Text = "Điểm Tổng:",
                Location = new Point(labelX, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            grpScores.Controls.Add(lblFn);

            txtFnScore = new TextBox
            {
                Location = new Point(textBoxX, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ReadOnly = true,
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.FromArgb(41, 128, 185),
                TextAlign = HorizontalAlignment.Center
            };
            grpScores.Controls.Add(txtFnScore);
            toolTip.SetToolTip(txtFnScore, "Tự động tính: QT×20% + GK×30% + CK×50%");

            btnSave = new Button
            {
                Text = "💾 Nhập Điểm",
                Location = new Point(120, 280),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(260, 280),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblTitle, grpScores, btnSave, btnCancel });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void CreateScoreField(GroupBox parent, string labelText, ref TextBox textBox,
            int labelX, int textBoxX, int yPos, string tooltipText)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(labelX, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lbl);

            textBox = new TextBox
            {
                Location = new Point(textBoxX, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F),
                TextAlign = HorizontalAlignment.Center
            };
            textBox.TextChanged += ScoreTextBox_TextChanged;
            textBox.KeyPress += ScoreTextBox_KeyPress;
            parent.Controls.Add(textBox);

            toolTip.SetToolTip(textBox, tooltipText);
        }

        private void LoadScoreData()
        {
            try
            {
                var score = _scoreController.GetScoreDetail(_studentId, _subjectId, _schoolYear, _semester);

                if (score != null)
                {
                    txtQtScore.Text = score.QtScore.ToString();
                    txtGkScore.Text = score.GkScore.ToString();
                    txtCkScore.Text = score.CkScore.ToString();
                    if (score.FnScore.HasValue)
                    {
                        txtFnScore.Text = score.FnScore.Value.ToString("F2");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được điểm: {ex.Message}",
                    "Lỗi Tải Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScoreTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            errorProvider.SetError(textBox, "");

            if (ValidateAllScores())
            {
                CalculateFinalScore();
            }
        }

        private void ScoreTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            TextBox textBox = sender as TextBox;
            if (e.KeyChar == '.' && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private bool ValidateAllScores()
        {
            bool isValid = true;

            isValid &= ValidateScore(txtQtScore, "Điểm QT");
            isValid &= ValidateScore(txtGkScore, "Điểm GK");
            isValid &= ValidateScore(txtCkScore, "Điểm CK");

            return isValid;
        }

        private bool ValidateScore(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider.SetError(textBox, $"{fieldName} là bắt buộc");
                textBox.BackColor = Color.FromArgb(255, 230, 230);
                return false;
            }

            if (!decimal.TryParse(textBox.Text, out decimal score))
            {
                errorProvider.SetError(textBox, $"{fieldName} phải là số hợp lệ");
                textBox.BackColor = Color.FromArgb(255, 230, 230);
                return false;
            }

            if (score < 0.00m || score > 10.00m)
            {
                errorProvider.SetError(textBox, $"{fieldName} phải từ 0.00 đến 10.00");
                textBox.BackColor = Color.FromArgb(255, 230, 230);
                return false;
            }

            textBox.BackColor = Color.White;
            errorProvider.SetError(textBox, "");
            return true;
        }

        private void CalculateFinalScore()
        {
            if (decimal.TryParse(txtQtScore.Text, out decimal qt) &&
                decimal.TryParse(txtGkScore.Text, out decimal gk) &&
                decimal.TryParse(txtCkScore.Text, out decimal ck))
            {
                decimal finalScore = (qt * 0.2m) + (gk * 0.3m) + (ck * 0.5m);
                txtFnScore.Text = finalScore.ToString("F2");

                txtFnScore.ForeColor = GetColorForScore(finalScore);
            }
            else
            {
                txtFnScore.Text = "";
            }
        }

        private Color GetColorForScore(decimal score)
        {
            if (score >= 9.0m) return Color.FromArgb(39, 174, 96);
            if (score >= 8.0m) return Color.FromArgb(52, 152, 219);
            if (score >= 7.0m) return Color.FromArgb(26, 188, 156);
            if (score >= 6.0m) return Color.FromArgb(243, 156, 18);
            if (score >= 5.0m) return Color.FromArgb(230, 126, 34);
            return Color.FromArgb(231, 76, 60);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateAllScores())
            {
                MessageBox.Show("Vui lòng sửa các lỗi kiểm tra trước khi nhập.",
                    "Lỗi Xác Thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnSave.Enabled = false;
                btnSave.Text = "Đang Nhập...";

                decimal qtScore = decimal.Parse(txtQtScore.Text);
                decimal gkScore = decimal.Parse(txtGkScore.Text);
                decimal ckScore = decimal.Parse(txtCkScore.Text);
                decimal fnScore = decimal.Parse(txtFnScore.Text);

                (bool success, string message) = _scoreController.SaveScore(
                    _studentId,
                    _subjectId,
                    _schoolYear,
                    _semester,
                    qtScore,
                    gkScore,
                    ckScore
                );

                if (success)
                {
                    MessageBox.Show("Đã nhập điểm thành công!",
                        "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không nhập được điểm. Vui lòng thử lại.",
                        "Lỗi Nhập Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không nhập được điểm: {ex.Message}",
                    "Lỗi Nhập Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSave.Enabled = true;
                btnSave.Text = "💾 Nhập Điểm";
            }
        }
    }
}
