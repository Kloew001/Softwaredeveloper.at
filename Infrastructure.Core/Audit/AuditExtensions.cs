namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit
{
    public static class AuditExtensions
    {
        public static Type GetEntityAuditType(this IAuditableEntity entity)
        {
            var interfac = entity
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(i => 
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IAuditableEntity<>));

            var entityAuditType = interfac.GenericTypeArguments.Single();

            return entityAuditType;
        }
    }
}
