using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Mappings.Custom;
using FluentNHibernate.Mapping;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class BaseMap<T> : ClassMap<T> where T: BaseEntity
    {
        public void MapBase()
        {
            Id(x => x.Id).GeneratedBy.UuidHex("N").Column("id");
            Map(x => x.CreatedAt).CustomType<PostgresqlTimestamptz>().Column("created_at");
            Map(x => x.UpdatedAt).CustomType<PostgresqlTimestamptz>().Column("updated_at");
        }
    }
}
