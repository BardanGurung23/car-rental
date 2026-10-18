using WeAreCars.Core;

namespace WeAreCars.App;

internal sealed class LoginForm : Form
{
    private readonly TextBox username = new() { MaxLength = 100, Dock = DockStyle.Fill };
    private readonly TextBox password = new() { UseSystemPasswordChar = true, MaxLength = 100, Dock = DockStyle.Fill };
    private readonly Label error = new() { AutoSize = true, ForeColor = Color.Firebrick };

    public LoginForm()
    {
        Text = "WeAreCars — Staff login";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 300);
        MinimumSize = new Size(440, 340);
        Font = new Font("Segoe UI", 10);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28), ColumnCount = 1, RowCount = 7 };
        layout.Controls.Add(new Label { Text = "WeAreCars staff login", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        layout.Controls.Add(new Label { Text = "Username", AutoSize = true });
        layout.Controls.Add(username);
        layout.Controls.Add(new Label { Text = "Password", AutoSize = true });
        layout.Controls.Add(password);
        layout.Controls.Add(error);
        var login = new Button { Text = "Log in", AutoSize = true };
        login.Click += (_, _) => LogIn();
        layout.Controls.Add(login);
        Controls.Add(layout);
        AcceptButton = login;
    }

    private void LogIn()
    {
        if (!StaffLogin.Authenticate(username.Text, password.Text))
        {
            error.Text = "Username or password is incorrect.";
            password.Clear();
            password.Focus();
            return;
        }
        password.Clear();
        Hide();
        using var main = new MainForm();
        main.ShowDialog(this);
        username.Clear();
        error.Text = "";
        Show();
        username.Focus();
    }
}
