namespace WeAreCars.Core;

public enum CarType { City, Family, Sports, SUV }
public enum FuelType { Petrol, Diesel, Hybrid, Electric }

public sealed record BookingRequest(string FirstName, string Surname, string Address, int Age,
    bool HasValidLicence, int Days, CarType Car, FuelType Fuel, bool UnlimitedMileage, bool BreakdownCover)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || FirstName.Trim().Length > 100)
            throw new ArgumentException("Enter a first name (up to 100 characters).");
        if (string.IsNullOrWhiteSpace(Surname) || Surname.Trim().Length > 100)
            throw new ArgumentException("Enter a surname (up to 100 characters).");
        if (string.IsNullOrWhiteSpace(Address) || Address.Trim().Length > 500)
            throw new ArgumentException("Enter an address (up to 500 characters).");
        if (Age is < 1 or > 120) throw new ArgumentException("Enter an age between 1 and 120.");
        if (!HasValidLicence) throw new ArgumentException("A valid driving licence is required to book a car.");
        if (Days is < 1 or > 28) throw new ArgumentException("Rentals must be between 1 and 28 days.");
        if (!Enum.IsDefined(Car) || !Enum.IsDefined(Fuel)) throw new ArgumentException("Select a valid car and fuel type.");
    }
}

public sealed record PriceBreakdown(decimal Rental, decimal Car, decimal Fuel, decimal Mileage, decimal Cover)
{
    public decimal Total => Rental + Car + Fuel + Mileage + Cover;
}

public static class Pricing
{
    public static PriceBreakdown Calculate(BookingRequest request)
    {
        request.Validate();
        return new(request.Days * 25m,
            request.Car switch { CarType.City => 0m, CarType.Family => 50m, CarType.Sports => 75m, CarType.SUV => 65m, _ => throw new ArgumentException("Invalid car.") },
            request.Fuel switch { FuelType.Petrol or FuelType.Diesel => 0m, FuelType.Hybrid => 30m, FuelType.Electric => 50m, _ => throw new ArgumentException("Invalid fuel.") },
            request.UnlimitedMileage ? request.Days * 10m : 0m,
            request.BreakdownCover ? request.Days * 2m : 0m);
    }
}

public sealed record Rental(Guid Id, BookingRequest Customer, DateTimeOffset BookedAt,
    decimal Total, DateTimeOffset? ReturnedAt = null)
{
    public DateTimeOffset DueAt => BookedAt.AddDays(Customer.Days);
}

public static class StaffLogin
{
    // Fixed demonstration credentials are required by the assignment.
    public static bool Authenticate(string username, string password) =>
        username == "sta001" && password == "givemethekeys123";
}
