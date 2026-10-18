using System.Text.Json;
using WeAreCars.Core;

var checks = 0;
void Check(bool condition, string message) { checks++; if (!condition) throw new Exception(message); }
void Reject(Action action, string message)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or JsonException) { checks++; return; }
    throw new Exception(message);
}
var request = new BookingRequest("Alex", "Smith", "1 High Street", 21, true, 1, CarType.City, FuelType.Petrol, false, false);
Check(StaffLogin.Authenticate("sta001", "givemethekeys123"), "Valid login failed.");
Check(!StaffLogin.Authenticate("STA001", "givemethekeys123"), "Username must match.");
Check(!StaffLogin.Authenticate("sta001", "wrong"), "Invalid password accepted.");
Check(!StaffLogin.Authenticate("", ""), "Empty login accepted.");
var carCosts = new[] { 0m, 50m, 75m, 65m };
var fuelCosts = new[] { 0m, 0m, 30m, 50m };
foreach (var car in Enum.GetValues<CarType>())
foreach (var fuel in Enum.GetValues<FuelType>())
foreach (var days in new[] { 1, 14, 28 })
foreach (var mileage in new[] { false, true })
foreach (var cover in new[] { false, true })
{
    var quote = request with { Car = car, Fuel = fuel, Days = days, UnlimitedMileage = mileage, BreakdownCover = cover };
    Check(Pricing.Calculate(quote).Total == days * (25m + (mileage ? 10m : 0m) + (cover ? 2m : 0m)) + carCosts[(int)car] + fuelCosts[(int)fuel], "Price mismatch.");
}
foreach (var invalid in new[] { request with { FirstName = " " }, request with { Surname = "" }, request with { Address = "" },
    request with { FirstName = new string('a', 101) }, request with { Address = new string('a', 501) },
    request with { Age = 0 }, request with { Age = 121 }, request with { HasValidLicence = false },
    request with { Days = 0 }, request with { Days = 29 }, request with { Car = (CarType)99 }, request with { Fuel = (FuelType)(-1) } })
    Reject(() => Pricing.Calculate(invalid), "Invalid booking accepted.");
var directory = Path.Combine(Path.GetTempPath(), "WeAreCars-checks-" + Guid.NewGuid());
Directory.CreateDirectory(directory);
try
{
    var path = Path.Combine(directory, "rentals.json");
    var store = new RentalStore(path);
    Check(store.Load().Count == 0, "New store should be empty.");
    var rental = new Rental(Guid.NewGuid(), request, DateTimeOffset.Now, 25m);
    store.Save([rental]);
    Check(store.Load().Single() == rental, "Booking did not round trip.");
    Check(rental.DueAt == rental.BookedAt.AddDays(1), "Wrong due date.");
    var returned = rental with { ReturnedAt = DateTimeOffset.Now };
    store.Save([returned]);
    Check(store.Load().Single().ReturnedAt == returned.ReturnedAt, "Return did not persist.");
    Check(new RentalStore(path + ".bak").Load().Single() == rental, "Backup missing previous booking.");
    File.WriteAllText(path, "broken");
    Reject(() => store.Load(), "Corrupt file accepted.");
    File.WriteAllText(path, "null");
    Reject(() => store.Load(), "Null file accepted.");
    File.WriteAllText(path, "[null]");
    Reject(() => store.Load(), "Null entry accepted.");
    File.WriteAllText(path, JsonSerializer.Serialize(new[] { rental, rental }));
    Reject(() => store.Load(), "Duplicate rental accepted.");
    File.WriteAllText(path, JsonSerializer.Serialize(new[] { rental with { Total = 1m } }));
    Reject(() => store.Load(), "Invalid total accepted.");
}
finally { Directory.Delete(directory, true); }
Console.WriteLine($"Passed {checks} checks.");
