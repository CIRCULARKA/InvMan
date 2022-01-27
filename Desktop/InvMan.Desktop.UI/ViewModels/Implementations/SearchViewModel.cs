using System.Linq;
using System.Reactive;
using System.Collections.Generic;
using ReactiveUI;
using InvMan.Desktop.Service;
using System.Text;
using ReactiveUI;

namespace InvMan.Desktop.UI.ViewModels
{
    public class SearchViewModel : ViewModelBase, ISearchViewModel
    {
        private string _searchQuery;

        private IApplicationEvents _events;

        private IDevicesListViewModel _devicesListViewModel;

        public SearchViewModel(
            IApplicationEvents events,
            IDevicesListViewModel devicesListViewModel
        )
        {
            _events = events;
            _devicesListViewModel = devicesListViewModel;

            FilterDevicesCommand = ReactiveCommand.Create(
                () => events.RaiseSearchExecuted(
                    FilterDevices(devicesListViewModel.Appliances)
                )
            );
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ReactiveCommand<Unit, Unit> FilterDevicesCommand { get; }

        private List<Appliance> FilterDevices(IEnumerable<Appliance> devices)
        {
            var result = new List<Appliance>(devices.Count());

            result.AddRange(devices.Where(d => d.NetworkName.Contains(SearchQuery)));
            result.AddRange(devices.Where(d => d.InventoryNumber.Contains(SearchQuery)));
            result.AddRange(devices.Where(d => d.Housing.Contains(SearchQuery)));
            result.AddRange(devices.Where(d => d.Cabinet.Contains(SearchQuery)));

            return result;
        }
    }
}
