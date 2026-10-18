using System.Text.Json;

namespace WeAreCars.Core;

public sealed class RentalStore(string path)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public List<Rental> Load()
    {
        if (!File.Exists(path)) return [];
        var rentals = JsonSerializer.Deserialize<List<Rental>>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("The rentals file contains no rental list.");
        var ids = new HashSet<Guid>();
        foreach (var rental in rentals)
        {
            if (rental is null || rental.Customer is null) throw new InvalidDataException("Invalid rental entry.");
            rental.Customer.Validate();
            if (rental.Id == Guid.Empty || !ids.Add(rental.Id) || rental.Total != Pricing.Calculate(rental.Customer).Total
                || rental.BookedAt == default || rental.ReturnedAt < rental.BookedAt)
                throw new InvalidDataException("The rentals file contains an invalid booking.");
        }
        return rentals;
    }

    public void Save(IReadOnlyList<Rental> rentals)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);
        var temporary = path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(rentals, Options));
            if (File.Exists(path)) File.Copy(path, path + ".bak", true);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
