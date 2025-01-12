using System;

namespace DeployTools.Core.DataAccess.Entities
{
    public class JournalEntry : BaseEntity
    {
        public virtual string DeployId { get; set; }
        public virtual DateTimeOffset CommandStarted { get; set; }
        public virtual DateTimeOffset CommandCompleted { get; set; }
        public virtual string CommandExecuted { get; set; }
        public virtual bool WasSuccessful { get; set; }
        public virtual string Output { get; set; }
    }
}
