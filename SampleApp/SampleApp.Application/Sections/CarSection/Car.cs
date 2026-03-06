using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleApp.Application.Sections.CarSection;

[Table(nameof(Car))]
public abstract class Car : Entity
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int? Year { get; set; }
    public string Vin { get; set; }

    public int NumberOfSeats { get; set; }
    public bool IsRegistered { get; set; }
}

public class CombustionCar : Car
{
    public int HorsePower { get; set; }
    public int TopSpeedKmh { get; set; }

    public string FuelType { get; set; }
    public double EngineDisplacementLiters { get; set; }
    public bool HasTurbo { get; set; }
}

public class ElectricCar : Car
{
    public int BatteryCapacityKwh { get; set; }
    public int RangeKm { get; set; }

    public int ChargeTimeMinutes { get; set; }
    public bool SupportsFastCharging { get; set; }
}

public class HybridCar : ElectricCar
{
    public double CombustionEngineDisplacementLiters { get; set; }
    public string HybridType { get; set; }
}

public class ClassicCar : CombustionCar
{
    public bool IsHistoricRegistration { get; set; }
    public DateTime? LastRestorationDate { get; set; }
}

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder
            .HasDiscriminator<string>("CarType")
            .HasValue<CombustionCar>("CombustionCar")
            .HasValue<ElectricCar>("ElectricCar")
            .HasValue<HybridCar>("HybridCar")
            .HasValue<ClassicCar>("ClassicCar");
    }
}
