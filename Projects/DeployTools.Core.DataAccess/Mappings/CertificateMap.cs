using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Mappings.Custom;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class CertificateMap : BaseMap<Certificate>
    {
        public CertificateMap()
        {
            Table("certificates");
            MapBase();

            Map(x => x.Domain).Column("domain");
            Map(x => x.Arn).Column("arn");
            Map(x => x.CertificateId).Column("certificate_id");
            Map(x => x.ExpiresAt).CustomType<PostgresqlTimestamptz>().Column("expires_at");
            Map(x => x.ValidationInfo).Column("validation_info");
            Map(x => x.IsValidated).Column("is_validated");
            Map(x => x.IsCreated).Column("is_created");
            Map(x => x.IsMarkedForDeletion).Column("is_marked_for_deletion");
        }
    }
}