using System.Globalization;

namespace WeAreCars.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        ApplicationConfiguration.Initialize();
        using var instance = new Mutex(true, "WeAreCars.StaffRentalApplication", out var firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("WeAreCars is already running. Use the existing window.", "WeAreCars",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Application.Run(new LoginForm());
    }
}
