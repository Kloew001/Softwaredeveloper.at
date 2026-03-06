namespace SampleApp.Application.Sections.CarSection;

public class CarOverviewDto : Dto
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int? Year { get; set; }
    public string Vin { get; set; }
    public string DisplayName { get; set; }
}

public class CarOverviewDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : DefaultDtoFactory<CarOverviewDto, Car>(memoryCache, factoryResolver)
{
    public override CarOverviewDto ConvertToDto(Car entity, CarOverviewDto dto)
    {
        dto = base.ConvertToDto(entity, dto);
        dto.DisplayName = ($"{entity.Manufacturer} {entity.Model}").Trim();
        return dto;
    }
}

public class XCarDto : Dto
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int? Year { get; set; }
    public string Vin { get; set; }
    public string DisplayName { get; set; }
}

public abstract class CarDto : Dto
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int? Year { get; set; }
    public string Vin { get; set; }

    public int NumberOfSeats { get; set; }
    public bool IsRegistered { get; set; }

    public string DisplayName { get; set; }
}

public class CombustionCarDto : CarDto
{
    public int HorsePower { get; set; }
    public int TopSpeedKmh { get; set; }

    public string FuelType { get; set; }
    public double EngineDisplacementLiters { get; set; }
    public bool HasTurbo { get; set; }

	public string EngineSummary { get; set; }

	public bool IsHighPerformance { get; set; }
}

public class ElectricCarDto : CarDto
{
	public int BatteryCapacityKwh { get; set; }
	public int RangeKm { get; set; }

	public int ChargeTimeMinutes { get; set; }
	public bool SupportsFastCharging { get; set; }

	public double RangePerKwh { get; set; }
}

public class HybridCarDto : ElectricCarDto
{
	public double CombustionEngineDisplacementLiters { get; set; }
	public string HybridType { get; set; }
    public bool IsPlugInHybrid { get; set; }
}

public class ClassicCarDto : CombustionCarDto
{
	public bool IsHistoricRegistration { get; set; }
	public DateTime? LastRestorationDate { get; set; }
	public bool IsOldTimer { get; set; }
}

public abstract class CarDtoFactoryBase<TDto, TEntity>(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : DefaultDtoFactory<TDto, TEntity>(memoryCache, factoryResolver)
    where TDto : CarDto
    where TEntity : Car
{
    public override TDto ConvertToDto(TEntity entity, TDto dto)
    {
        base.ConvertToDto(entity, dto);

        var baseName = ($"{entity.Manufacturer} {entity.Model}").Trim();
        if (entity.Year.HasValue)
        {
            baseName = $"{baseName} ({entity.Year})";
        }

        dto.DisplayName = baseName;
        return dto;
    }
}

public abstract class CombustionCarDtoFactoryBase<TDto, TEntity>(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : CarDtoFactoryBase<TDto, TEntity>(memoryCache, factoryResolver)
    where TDto : CombustionCarDto
    where TEntity : CombustionCar
{
    public override TDto ConvertToDto(TEntity entity, TDto dto)
    {
        base.ConvertToDto(entity, dto);

        var engineText = $"{entity.EngineDisplacementLiters:0.0}L";
        if (string.IsNullOrWhiteSpace(entity.FuelType) == false)
            engineText += $" {entity.FuelType}";
        if (entity.HasTurbo)
            engineText += " Turbo";

        dto.EngineSummary = engineText;
        dto.IsHighPerformance = entity.HorsePower >= 250;

        return dto;
    }
}

public class CombustionCarDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : CombustionCarDtoFactoryBase<CombustionCarDto, CombustionCar>(memoryCache, factoryResolver)
{
}

public abstract class ElectricCarDtoFactoryBase<TDto, TEntity>(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : CarDtoFactoryBase<TDto, TEntity>(memoryCache, factoryResolver)
    where TDto : ElectricCarDto
    where TEntity : ElectricCar
{
    public override TDto ConvertToDto(TEntity entity, TDto dto)
    {
        base.ConvertToDto(entity, dto);

        dto.RangePerKwh = entity.BatteryCapacityKwh > 0
            ? (double)entity.RangeKm / entity.BatteryCapacityKwh
            : 0;

        return dto;
    }
}

public class ElectricCarDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : ElectricCarDtoFactoryBase<ElectricCarDto, ElectricCar>(memoryCache, factoryResolver)
{
}

public class HybridCarDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : ElectricCarDtoFactoryBase<HybridCarDto, HybridCar>(memoryCache, factoryResolver)
{
    public override HybridCarDto ConvertToDto(HybridCar entity, HybridCarDto dto)
    {
        base.ConvertToDto(entity, dto);

        dto.IsPlugInHybrid = string.Equals(entity.HybridType, "PHEV", StringComparison.OrdinalIgnoreCase);
        return dto;
    }
}

public class ClassicCarDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver)
    : CombustionCarDtoFactoryBase<ClassicCarDto, ClassicCar>(memoryCache, factoryResolver)
{
    public override ClassicCarDto ConvertToDto(ClassicCar entity, ClassicCarDto dto)
    {
        base.ConvertToDto(entity, dto);

        var isOldByAge = entity.Year.HasValue && entity.Year <= DateTime.UtcNow.Year - 30;
        dto.IsOldTimer = entity.IsHistoricRegistration || isOldByAge;

        return dto;
    }
}

