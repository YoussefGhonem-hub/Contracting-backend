using Contracting.Domain.Entities.master;
using Contracting.Shared.MasterDtos.BranchDto;
using Mapster;

namespace Contracting.Application.Common.Mappings;

public static class MappingConfig
{
    private const string FallbackImage =
        "https://images.pexels.com/photos/90946/pexels-photo-90946.jpeg?auto=compress&cs=tinysrgb&dpr=1&w=500";

    public static void Register(TypeAdapterConfig config)
    {
        // Add global mapping configurations here if needed in the future
        config.NewConfig<Branch, GetBranchDto>();


    }
}