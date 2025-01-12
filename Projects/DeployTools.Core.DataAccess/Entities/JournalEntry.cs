using System;

namespace DeployTools.Core.DataAccess.Entities
{
    public class JournalEntry : BaseEntity
    {
        public virtual string DeployId { get; set; }
        public virtual DateTimeOffset CommandStarted { get; set; }
        public DateTimeOffset CommandCompleted { get; set; }
        public string CommandExecuted { get; set; }
        public bool WasSuccessful { get; set; }
        public string Output { get; set; }
    }
}
