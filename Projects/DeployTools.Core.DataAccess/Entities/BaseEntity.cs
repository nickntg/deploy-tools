using System;

namespace DeployTools.Core.DataAccess.Entities
{
    public class BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual DateTimeOffset CreatedAt { get; set; }
        public virtual DateTimeOffset? UpdatedAt { get; set; }
    }
}
