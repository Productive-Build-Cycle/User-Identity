using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Identity.Core.Domain.Entities
{
    public class BaseEntity : IdentityRole<Guid>
    {
        public BaseEntity()
        {
            this.CreateDate = DateTime.UtcNow;
        }
        [Key]
        public Int64 ID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
