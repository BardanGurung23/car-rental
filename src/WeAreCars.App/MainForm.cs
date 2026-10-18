using System.Text.Json;
using WeAreCars.Core;

namespace WeAreCars.App;

internal sealed class MainForm : Form
{
    private readonly TextBox firstName = new() { MaxLength = 100 };
    private readonly TextBox surname = new() { MaxLength = 100 };
    private readonly TextBox address = new() { Multiline = true, Height = 65, MaxLength = 500, ScrollBars = ScrollBars.Vertical };
    private readonly NumericUpDown age = new() { Minimum = 1, Maximum = 120, Value = 18 };
    private readonly ComboBox licence = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown days = new() { Minimum = 1, Maximum = 28, Value = 1 };
    private readonly ComboBox car = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox fuel = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox mileage = new() { Text = "Unlimited mileage (£10/day)", AutoSize = true };
    private readonly CheckBox cover = new() { Text = "Breakdown cover (£2/day)", AutoSize = true };
    private readonly Label price = new() { AutoSize = true, MaximumSize = new Size(450, 0) };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.DarkGreen };
    private readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly ToolTip tips = new();
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, RowHeadersVisible = false };
    private readonly CheckBox history = new() { Text = "Include returned rentals", AutoSize = true };
    private readonly Button book = new() { Text = "Review booking", AutoSize = true };
    private readonly Button returnCar = new() { Text = "Mark selected car as returned", AutoSize = true };
    private readonly RentalStore store = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WeAreCars", "rentals.json"));
    private List<Rental> rentals = [];
    private bool storageAvailable;

    public MainForm()
    {
        Text = "WeAreCars — Rental management";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 780);
        MinimumSize = new Size(850, 700);
        Font = new Font("Segoe UI", 10);
        errors.ContainerControl = this;
        licence.Items.AddRange(["Yes", "No"]);
        car.Items.AddRange(["City car (+£0)", "Family car (+£50)", "Sports car (+£75)", "SUV (+£65)"]);
        fuel.Items.AddRange(["Petrol (+£0)", "Diesel (+£0)", "Hybrid (+£30)", "Full electric (+£50)"]);
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(16) };
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var header = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        header.Controls.Add(new Label { Text = "WeAreCars   |   Staff rental management", AutoSize = true, Padding = new Padding(0, 8, 25, 8), Font = new Font(Font, FontStyle.Bold) });
        var help = new Button { Text = "Help", AutoSize = true };
        help.Click += (_, _) => MessageBox.Show(this,
            "All starred fields are required. A valid driving licence is required. Rentals last 1–28 days.\n\nCar and fuel supplements apply once per booking; extras apply each day. Review the total before confirming.\n\nBookings start immediately. Use Current rentals to record a return. Overdue rentals remain active until returned.\n\nBookings are saved on this computer under Local AppData/WeAreCars. Protect this folder because it contains customer details.",
            "Booking help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        var logout = new Button { Text = "Log out", AutoSize = true };
        logout.Click += (_, _) => Close();
        header.Controls.Add(help);
        header.Controls.Add(logout);
        shell.Controls.Add(header);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var bookingTab = new TabPage("New booking") { AutoScroll = true };
        var rentalsTab = new TabPage("Current rentals");
        tabs.TabPages.AddRange([bookingTab, rentalsTab]);
        var form = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(20) };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddField(form, "First name *", firstName, "Enter the customer's first name.");
        AddField(form, "Surname *", surname, "Enter the customer's surname.");
        AddField(form, "Address *", address, "Enter the customer's full address.");
        AddField(form, "Age *", age, "Record the customer's age. The brief does not specify a minimum rental age.");
        AddField(form, "Valid driving licence? *", licence, "Select Yes only after checking the customer's licence.");
        AddField(form, "Rental days *", days, "Between 1 and 28 days at £25 per day.");
        AddField(form, "Car type *", car, "A one-off supplement applies to the selected category.");
        AddField(form, "Fuel type *", fuel, "A one-off supplement applies to hybrid and electric cars.");
        AddField(form, "Optional extras", mileage, "Charged for every rental day.");
        AddField(form, "", cover, "Charged for every rental day.");
        AddField(form, "Price breakdown", price, "The total includes all selected options.");
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var clear = new Button { Text = "Clear form", AutoSize = true };
        clear.Click += (_, _) => { if (MessageBox.Show(this, "Clear the booking form?", "Clear form", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) ResetForm(); };
        actions.Controls.Add(book);
        actions.Controls.Add(clear);
        AddField(form, "", actions, "");
        AddField(form, "", status, "");
        bookingTab.Controls.Add(form);
        var rentalLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(10) };
        rentalLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rentalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var rentalActions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        rentalActions.Controls.Add(history);
        rentalActions.Controls.Add(returnCar);
        rentalLayout.Controls.Add(rentalActions);
        rentalLayout.Controls.Add(grid);
        rentalsTab.Controls.Add(rentalLayout);
        shell.Controls.Add(tabs);
        Controls.Add(shell);
        book.Click += (_, _) => CreateBooking();
        returnCar.Click += (_, _) => ReturnSelected();
        history.CheckedChanged += (_, _) => RefreshRentals();
        foreach (var choice in new[] { car, fuel, licence }) choice.SelectedIndexChanged += (_, _) => UpdatePrice();
        days.ValueChanged += (_, _) => UpdatePrice();
        mileage.CheckedChanged += (_, _) => UpdatePrice();
        cover.CheckedChanged += (_, _) => UpdatePrice();
        ResetForm();
        Shown += (_, _) => LoadRentals();
    }

    private void AddField(TableLayoutPanel panel, string label, Control control, string tip)
    {
        var row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, 0, row);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(3, 4, 24, 4);
        control.AccessibleName = string.IsNullOrEmpty(label) ? control.Text : label.TrimEnd(' ', '*');
        panel.Controls.Add(control, 1, row);
        tips.SetToolTip(control, tip);
    }

    private BookingRequest Request() => new(firstName.Text.Trim(), surname.Text.Trim(), address.Text.Trim(),
        (int)age.Value, licence.SelectedIndex == 0, (int)days.Value, (CarType)car.SelectedIndex,
        (FuelType)fuel.SelectedIndex, mileage.Checked, cover.Checked);

    private void UpdatePrice()
    {
        if (car.SelectedIndex < 0 || fuel.SelectedIndex < 0) { price.Text = "Select a car and fuel type to see the total."; return; }
        var quote = Pricing.Calculate(Request() with { FirstName = "Quote", Surname = "Quote", Address = "Quote", HasValidLicence = true });
        price.Text = $"Rental: {quote.Rental:C2}   |   Car: {quote.Car:C2}   |   Fuel: {quote.Fuel:C2}\nMileage: {quote.Mileage:C2}   |   Breakdown cover: {quote.Cover:C2}\nTotal: {quote.Total:C2}";
    }

    private bool ValidateFields()
    {
        errors.Clear();
        Control? firstInvalid = null;
        void Check(Control control, bool invalid, string message)
        {
            if (!invalid) return;
            errors.SetError(control, message);
            firstInvalid ??= control;
        }
        Check(firstName, string.IsNullOrWhiteSpace(firstName.Text), "Enter the customer's first name.");
        Check(surname, string.IsNullOrWhiteSpace(surname.Text), "Enter the customer's surname.");
        Check(address, string.IsNullOrWhiteSpace(address.Text), "Enter the customer's address.");
        Check(licence, licence.SelectedIndex != 0, "A valid driving licence is required.");
        Check(car, car.SelectedIndex < 0, "Select a car type.");
        Check(fuel, fuel.SelectedIndex < 0, "Select a fuel type.");
        firstInvalid?.Focus();
        return firstInvalid is null;
    }

    private void CreateBooking()
    {
        status.Text = "";
        if (!storageAvailable || !ValidateFields()) return;
        var request = Request();
        var total = Pricing.Calculate(request).Total;
        if (MessageBox.Show(this,
            $"Customer: {request.FirstName} {request.Surname}\nAddress: {request.Address}\nAge: {request.Age}\nValid licence: Yes\nCar: {car.Text}\nFuel: {fuel.Text}\nDays: {request.Days}\nUnlimited mileage: {(request.UnlimitedMileage ? "Yes" : "No")}\nBreakdown cover: {(request.BreakdownCover ? "Yes" : "No")}\n\n{price.Text}\n\nConfirm this booking?",
            "Review booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        var rental = new Rental(Guid.NewGuid(), request, DateTimeOffset.Now, total);
        var updated = new List<Rental>(rentals) { rental };
        if (!Save(updated)) return;
        ResetForm();
        status.Text = $"Booking saved. Reference: {rental.Id.ToString()[..8]}. Total: {total:C2}.";
        RefreshRentals();
    }

    private void ReturnSelected()
    {
        if (!storageAvailable || grid.CurrentRow?.Cells["Reference"].Value is not Guid id) return;
        var rental = rentals.Single(r => r.Id == id);
        if (rental.ReturnedAt is not null) { MessageBox.Show(this, "This car has already been returned."); return; }
        if (MessageBox.Show(this, $"Record the return for {rental.Customer.FirstName} {rental.Customer.Surname}?", "Confirm return",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        var updated = rentals.Select(r => r.Id == id ? r with { ReturnedAt = DateTimeOffset.Now } : r).ToList();
        if (Save(updated)) RefreshRentals();
    }

    private bool Save(List<Rental> updated)
    {
        try { store.Save(updated); rentals = updated; return true; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"The change was not saved. Your existing bookings are unchanged.\n\n{exception.Message}", "Unable to save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void LoadRentals()
    {
        try { rentals = store.Load(); storageAvailable = true; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            MessageBox.Show(this, $"Bookings could not be loaded. Booking and return actions are disabled to protect existing data. Close the app and check the rentals.json file or its .bak backup in Local AppData/WeAreCars.\n\n{exception.Message}", "Unable to load rentals", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        book.Enabled = storageAvailable;
        returnCar.Enabled = storageAvailable;
        RefreshRentals();
    }

    private void RefreshRentals()
    {
        grid.DataSource = rentals.Where(r => history.Checked || r.ReturnedAt is null).OrderByDescending(r => r.BookedAt)
            .Select(r => new { Reference = r.Id, Customer = $"{r.Customer.FirstName} {r.Customer.Surname}",
                Car = r.Customer.Car, Fuel = r.Customer.Fuel, Days = r.Customer.Days,
                Booked = r.BookedAt.LocalDateTime, Due = r.DueAt.LocalDateTime, Total = r.Total,
                Status = r.ReturnedAt is not null ? "Returned" : r.DueAt < DateTimeOffset.Now ? "Overdue" : "Rented",
                Returned = r.ReturnedAt?.LocalDateTime }).ToList();
        if (grid.Columns["Total"] is { } totalColumn) totalColumn.DefaultCellStyle.Format = "C2";
        if (grid.Columns["Reference"] is { } referenceColumn) referenceColumn.Visible = false;
        foreach (var name in new[] { "Booked", "Due", "Returned" })
            if (grid.Columns[name] is { } column) column.DefaultCellStyle.Format = "g";
    }

    private void ResetForm()
    {
        firstName.Clear(); surname.Clear(); address.Clear(); age.Value = 18; days.Value = 1;
        licence.SelectedIndex = -1; car.SelectedIndex = -1; fuel.SelectedIndex = -1;
        mileage.Checked = false; cover.Checked = false; errors.Clear(); status.Text = "";
        UpdatePrice(); firstName.Focus();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { errors.Dispose(); tips.Dispose(); }
        base.Dispose(disposing);
    }
}
