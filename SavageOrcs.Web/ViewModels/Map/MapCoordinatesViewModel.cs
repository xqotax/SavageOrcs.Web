using SavageOrcs.Web.ViewModels.Mark;

namespace SavageOrcs.Web.ViewModels.Map
{
    public class MapCoordinatesViewModel: UnitedCatalogueViewModel
    {
        public int Id { get; set; }

        public string? Lat { get; set; }

        public string? Lng { get; set; }

        public string Zoom { get; set; } = string.Empty;

        public string? Name { get; set; }

        public MapMarkViewModel[] MapMarkViewModels { get; set; } = Array.Empty<MapMarkViewModel>();

        public MapClusterViewModel[] MapClusterViewModels { get; set; } = Array.Empty<MapClusterViewModel>();

        public string GoogleMapKey { get; set; } = string.Empty;
    }
}
