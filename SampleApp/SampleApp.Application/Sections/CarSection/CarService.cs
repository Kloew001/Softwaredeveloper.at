namespace SampleApp.Application.Sections.CarSection;

public class CarService : EntityService<Car>
{
    public CarService(EntityServiceDependency<Car> entityServiceDependency)
        : base(entityServiceDependency)
    {
    }
}
