using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Mappings.Custom;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class JournalEntryMap : BaseMap<JournalEntry>
    {
        public JournalEntryMap()
        {
            Table("journal_entries");
            MapBase();

            Map(x => x.CommandCompleted).CustomType<PostgresqlTimestamptz>().Column("command_completed");
            Map(x => x.CommandStarted).CustomType<PostgresqlTimestamptz>().Column("command_started");
            Map(x => x.CommandExecuted).Column("command_executed");
            Map(x => x.DeployId).Column("deploy_id");
            Map(x => x.Output).Column("output");
            Map(x => x.WasSuccessful).Column("was_successful");
        }
    }
}
