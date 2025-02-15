using System.Threading;
using System.Threading.Tasks;

namespace DeployTools.Core.Services.Background.Interfaces
{
    public interface ICertificateAdminJob
    {
        Task ProcessAsync(CancellationToken cancellationToken);
    }
}
