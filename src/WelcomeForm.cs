using System.Drawing;
using System.Windows.Forms;
using TransparentClock;

public class WelcomeForm : Form
{
    private readonly TextBox nameTextBox;
    private readonly ComboBox genderCombo;
    private readonly Button continueButton;

    public WelcomeForm()
    {
        Text = "Welcome";
        Size = new Size(420, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ControlBox = false;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var nameLabel = new Label
        {
            AutoSize = true,
            Text = "Name"
        };

        nameTextBox = new TextBox
        {
            Width = 220
        };

        var genderLabel = new Label
        {
            AutoSize = true,
            Text = "Gender"
        };

        genderCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 160
        };
        genderCombo.Items.AddRange(new object[] { "male", "female", "other" });
        genderCombo.SelectedIndex = 2;

        continueButton = new Button
        {
            Text = "Continue",
            Width = 120,
            Enabled = false
        };

        nameTextBox.TextChanged += (_, __) =>
        {
            continueButton.Enabled = !string.IsNullOrWhiteSpace(nameTextBox.Text);
        };

        continueButton.Click += (_, __) =>
        {
            Program.CurrentState.UserName = nameTextBox.Text.Trim();
            Program.CurrentState.Gender = genderCombo.SelectedItem?.ToString() ?? "other";
            Program.CurrentState.IsFirstRun = false;
            AppStateStorage.Save(Program.CurrentState);
            DialogResult = DialogResult.OK;
            Close();
        };

        layout.Controls.Add(nameLabel, 0, 0);
        layout.Controls.Add(nameTextBox, 1, 0);
        layout.Controls.Add(genderLabel, 0, 1);
        layout.Controls.Add(genderCombo, 1, 1);
        layout.Controls.Add(continueButton, 1, 2);

        Controls.Add(layout);
    }
}
