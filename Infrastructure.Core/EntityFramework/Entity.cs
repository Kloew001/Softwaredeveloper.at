using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IEntity
    {
        public Guid Id { get; set; }
    }

    public abstract class Entity : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }

        //[ConcurrencyCheck]
        //[Column("xmin", TypeName = "xid")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public long RowVersion { get; set; }

        //[ConcurrencyCheck]
        //[Timestamp]
        //public byte[] Timestamp { get; set; }

        public override string ToString()
        {
            return $"[{GetType().Name} {Id}]";
        }
    }
}
