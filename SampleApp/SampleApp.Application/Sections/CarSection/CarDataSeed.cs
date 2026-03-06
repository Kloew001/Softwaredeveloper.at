using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SampleApp.Application.Sections.CarSection;

public class CarDataSeed : IDataSeed
{
    public decimal Priority => 15m;
    public bool AutoExecute => true;

    private readonly IDbContext _context;

    public CarDataSeed(IDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (_context.Set<Car>().Any())
            return;

        var random = new Random();

        var manufacturers = new[] { "Volkswagen", "BMW", "Audi", "Mercedes-Benz", "Tesla", "Toyota", "Porsche" };
        var combustionModels = new[] { "Golf GTI", "320i", "A4", "C200", "Supra", "911 Carrera" };
        var electricModels = new[] { "Model 3", "Model Y", "ID.3", "i3", "EQE", "Leaf" };
        var hybridModels = new[] { "Prius", "Passat GTE", "330e", "C300e" };
        var classicModels = new[] { "Beetle", "Mustang", "911 Classic", "E-Type" };
        var fuelTypes = new[] { "Petrol", "Diesel" };
        var hybridTypes = new[] { "HEV", "MHEV", "PHEV" };
        var seatOptions = new[] { 2, 4, 5, 7 };

        var cars = new List<Car>();

        // Combustion cars
        for (var i = 0; i < 80; i++)
        {
            var year = random.Next(1990, DateTime.UtcNow.Year + 1);

            var car = new CombustionCar
            {
                Manufacturer = manufacturers[random.Next(manufacturers.Length)],
                Model = combustionModels[random.Next(combustionModels.Length)],
                Year = year,
                Vin = Guid.NewGuid().ToString("N"),
                NumberOfSeats = seatOptions[random.Next(seatOptions.Length)],
                IsRegistered = random.NextDouble() > 0.2,

                HorsePower = random.Next(75, 450),
                TopSpeedKmh = random.Next(160, 300),
                FuelType = fuelTypes[random.Next(fuelTypes.Length)],
                EngineDisplacementLiters = Math.Round(random.NextDouble() * 3.0 + 1.0, 1), // 1.0L - 4.0L
                HasTurbo = random.NextDouble() > 0.5
            };

            cars.Add(car);
        }

        // Electric cars
        for (var i = 0; i < 60; i++)
        {
            var year = random.Next(2015, DateTime.UtcNow.Year + 1);

            var car = new ElectricCar
            {
                Manufacturer = manufacturers[random.Next(manufacturers.Length)],
                Model = electricModels[random.Next(electricModels.Length)],
                Year = year,
                Vin = Guid.NewGuid().ToString("N"),
                NumberOfSeats = seatOptions[random.Next(seatOptions.Length)],
                IsRegistered = random.NextDouble() > 0.1,

                BatteryCapacityKwh = random.Next(40, 110),
                RangeKm = random.Next(200, 700),
                ChargeTimeMinutes = random.Next(20, 90),
                SupportsFastCharging = random.NextDouble() > 0.4
            };

            cars.Add(car);
        }

        // Hybrid cars
        for (var i = 0; i < 40; i++)
        {
            var year = random.Next(2005, DateTime.UtcNow.Year + 1);

            var car = new HybridCar
            {
                Manufacturer = manufacturers[random.Next(manufacturers.Length)],
                Model = hybridModels[random.Next(hybridModels.Length)],
                Year = year,
                Vin = Guid.NewGuid().ToString("N"),
                NumberOfSeats = seatOptions[random.Next(seatOptions.Length)],
                IsRegistered = random.NextDouble() > 0.1,

                BatteryCapacityKwh = random.Next(8, 30),
                RangeKm = random.Next(600, 1000),
                ChargeTimeMinutes = random.Next(30, 120),
                SupportsFastCharging = random.NextDouble() > 0.5,

                CombustionEngineDisplacementLiters = Math.Round(random.NextDouble() * 2.0 + 1.2, 1),
                HybridType = hybridTypes[random.Next(hybridTypes.Length)]
            };

            cars.Add(car);
        }

        // Classic cars
        for (var i = 0; i < 30; i++)
        {
            var year = random.Next(1960, 1990);

            var car = new ClassicCar
            {
                Manufacturer = manufacturers[random.Next(manufacturers.Length)],
                Model = classicModels[random.Next(classicModels.Length)],
                Year = year,
                Vin = Guid.NewGuid().ToString("N"),
                NumberOfSeats = seatOptions[random.Next(seatOptions.Length)],
                IsRegistered = random.NextDouble() > 0.3,

                HorsePower = random.Next(60, 350),
                TopSpeedKmh = random.Next(140, 260),
                FuelType = fuelTypes[random.Next(fuelTypes.Length)],
                EngineDisplacementLiters = Math.Round(random.NextDouble() * 3.5 + 1.3, 1),
                HasTurbo = random.NextDouble() > 0.7,

                IsHistoricRegistration = random.NextDouble() > 0.5,
                LastRestorationDate = random.NextDouble() > 0.3
                    ? DateTime.UtcNow.AddYears(-random.Next(1, 15))
                    : null
            };

            cars.Add(car);
        }

        _context.AddRange(cars);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
