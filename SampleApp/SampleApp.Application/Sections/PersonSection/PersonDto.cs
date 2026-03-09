
namespace SampleApp.Application.Sections.PersonSection;

public class PersonDto : Dto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public IEnumerable<AddressDto> Addresses { get; set; }
}

public class AddressDto : Dto
{
    public bool IsMain { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}