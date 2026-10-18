# WeAreCars

A standalone C# Windows Forms application implementing the rental requirements in `CET131_Assignment_1.md`.

## Run on Windows

Install the **.NET 10 SDK** and Git. Clone this repository, then open `WeAreCars.sln` in Rider or Visual Studio with .NET 10 support. Set `WeAreCars.App` as the startup project and run it.

From a terminal in the repository:

```sh
dotnet build WeAreCars.sln -c Release
dotnet run --project src/WeAreCars.App
```

Log in with username **sta001** and password **givemethekeys123** (case sensitive).

## Features

- Required customer details and explicit driving licence confirmation.
- Rentals of 1–28 days at £25 per day.
- One-off car supplements: City £0, Family £50, Sports £75, SUV £65.
- One-off fuel supplements: Petrol/Diesel £0, Hybrid £30, Full electric £50.
- Unlimited mileage £10/day and breakdown cover £2/day.
- Live price breakdown, review before saving, inline validation, tooltips and help.
- Active and overdue rentals, return recording, and optional returned rental history.
- Local JSON storage with the previous saved version retained as a `.bak` file.
- Logout clears the login credentials and booking form by creating a fresh session.

## Behaviour and assumptions

Bookings start when confirmed; the due time is that time plus the selected days. An overdue rental stays active until staff record a return. Early returns do not change the original price. The brief provides categories rather than a vehicle inventory, so bookings track the car category and fuel type, without assigning a registration number or enforcing fleet availability.

Age is recorded as a whole number from 1 to 120. There is no minimum rental-age policy in the brief; eligibility is controlled by the required valid driving licence confirmation. Text fields accept international names and addresses. Car and fuel selections and licence confirmation start unselected.

The specified credentials are demonstration credentials embedded in the application, not production authentication. Customer records are stored as plain JSON in `%LOCALAPPDATA%\WeAreCars\rentals.json`; use Windows account/file protections and avoid committing real customer data. The app allows one running instance per Windows session. Storage is per Windows user and is not designed for a shared network folder.

If data cannot be read, the app disables booking and return actions and displays the error. Close the app before investigating. Preserve the damaged file, then restore `rentals.json.bak` as `rentals.json` if appropriate; the backup represents the previous successful save. Failed saves leave the in-memory rental list unchanged.

## Checks

The dependency-free checks run on macOS and Windows:

```sh
dotnet run --project tests/WeAreCars.Checks -c Release
```

They cover credentials, all car/fuel/extra combinations at multiple durations, invalid inputs, JSON round trips, returns, backups, and invalid stored data. Windows GitHub Actions builds the complete solution, runs the checks, and uploads a self-contained Windows x64 app. Download and extract the complete artifact folder, then run `WeAreCars.App.exe`.

To publish manually on Windows:

```sh
dotnet publish src/WeAreCars.App -c Release -r win-x64 --self-contained true -o artifacts/WeAreCars
```

Windows Forms requires Windows to run and visually verify. macOS can run the core checks and cross-compile the app when Windows targeting references are available.

## Windows acceptance checks

1. Reject blank or incorrect credentials; accept the specified credentials. Confirm the password is masked.
2. Reject blank customer details, missing selections and a No licence answer; focus the invalid field.
3. Confirm a one-day City/Petrol booking costs £25; a 28-day Sports/Electric booking with both extras costs £1,161.
4. Cancel a booking review and confirm nothing is saved. Confirm a booking and find it in Current rentals.
5. Restart the application and confirm the booking persists.
6. Mark it returned; confirm it disappears from active rentals and appears with Include returned rentals selected. Restart to verify the return persisted.
7. Resize the window, navigate with Tab, and verify tooltips, help and readable controls at your Windows display scale.
8. Log out and verify a new login is required.

Only the software implementation is included; assignment report and modelling diagrams are outside scope.
