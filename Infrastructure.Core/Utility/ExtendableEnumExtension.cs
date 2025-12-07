using ExtendableEnums;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IExtendableEnumExtension
{

}
public abstract class ExtendableEnumExtension<TEnum> : IExtendableEnumExtension
    where TEnum : ExtendableEnum<TEnum>
{
}