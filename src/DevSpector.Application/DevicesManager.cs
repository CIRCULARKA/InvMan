using System;
using System.Linq;
using System.Collections.Generic;
using DevSpector.Domain;
using DevSpector.Domain.Models;
using DevSpector.SDK.Models;

namespace DevSpector.Application
{
	public class DevicesManager : IDevicesManager
	{
		private readonly IRepository _repo;

		public DevicesManager(IRepository repo)
		{
			_repo = repo;
		}

		public void CreateDevice(DeviceInfo info)
		{
			ThrowIfDevice(EntityExistance.Exists, info.InventoryNumber);

			// Get N/A cabinet in N/A housing to put it as device's location
			var defaultCabinetID = _repo.GetSingle<Cabinet>(
				c => c.Name == "N/A"
			).ID;

			ThrowIfDeviceTypeNotExists(info.TypeID);

			var newDevice = FormDeviceFrom(info);

			_repo.Add<Device>(newDevice);
			_repo.Save();

			// Assign N/A cabinet to newly created device
			_repo.Add<DeviceCabinet>(
				new DeviceCabinet {
					DeviceID = newDevice.ID,
					CabinetID = defaultCabinetID
				}
			);
			_repo.Save();
		}

		public void UpdateDevice(string targetInventoryNumber, DeviceInfo info)
		{
			ThrowIfDevice(EntityExistance.DoesNotExist, targetInventoryNumber);

			var targetDevice = _repo.GetSingle<Device>(
				d => d.InventoryNumber == targetInventoryNumber);

			if (info.TypeID != Guid.Empty)
			{
				ThrowIfDeviceTypeNotExists(info.TypeID);
				targetDevice.TypeID = info.TypeID;
			}

			if (info.InventoryNumber != null) {
				// Check if there is already device with such inventory number
				var sameDevice = _repo.GetSingle<Device>(d => d.InventoryNumber == info.InventoryNumber);
				if (sameDevice != null)
					throw new ArgumentException("Can't update device - there is already device with inventory number specified");

				targetDevice.InventoryNumber = info.InventoryNumber;
			}

			if (info.NetworkName != null) {
				// Check if there is already device with such network name
				var sameDevice = _repo.GetSingle<Device>(d => d.NetworkName == info.NetworkName);
				if (sameDevice != null)
					throw new ArgumentException("Can't update device - there is already device with network name specified");
				targetDevice.NetworkName = info.NetworkName;
			}

			_repo.Update<Device>(targetDevice);
			_repo.Save();
		}

		public void DeleteDevice(string inventoryNumber)
		{
			ThrowIfDevice(EntityExistance.DoesNotExist, inventoryNumber);

			var targetDevice = _repo.GetSingle<Device>(d => d.InventoryNumber == inventoryNumber);

			_repo.Remove<Device>(targetDevice.ID);
			_repo.Save();
		}

		public void MoveDevice(string inventoryNumber, Guid cabinetID)
		{
			ThrowIfDevice(EntityExistance.DoesNotExist, inventoryNumber);

			var targetCabinet = _repo.GetByID<Cabinet>(cabinetID);
			if (targetCabinet == null)
				throw new ArgumentException("Could not find cabinet with specified ID");

			var targetDevice = _repo.GetSingle<Device>(d => d.InventoryNumber == inventoryNumber);

			var newLocation = new DeviceCabinet {
				DeviceID = targetDevice.ID,
				CabinetID = targetCabinet.ID
			};

			_repo.Add<DeviceCabinet>(newLocation);
			_repo.Save();
		}

		public IEnumerable<Device> GetDevices() =>
			_repo.Get<Device>(include: "Type");

		public Cabinet GetDeviceCabinet(string inventoryNumber) =>
			_repo.GetSingle<DeviceCabinet>(include: "Cabinet,Cabinet.Housing,Device",
				filter: dc => dc.Device.InventoryNumber == inventoryNumber).Cabinet;

		public IEnumerable<Appliance> GetAppliances()
		{
			return GetDevices().Select(d => {
				var deviceCabinet = GetDeviceCabinet(d.InventoryNumber);
				return new Appliance(
					d.ID,
					d.InventoryNumber,
					d.Type.Name,
					d.NetworkName,
					deviceCabinet.Housing.Name,
					deviceCabinet.Name,
					_repo.Get<IPAddress>(
						filter: ip => ip.DeviceID == d.ID
					).Select(ip => ip.Address).ToList(),
					null
				);
			});
		}

		public IEnumerable<DeviceType> GetDeviceTypes() =>
			_repo.Get<DeviceType>();

		private void ThrowIfDevice(EntityExistance existance, string inventoryNumber)
		{
			var existingDevice = _repo.GetSingle<Device>(d => d.InventoryNumber == inventoryNumber);

			if (existance == EntityExistance.Exists) {
				if (existingDevice != null)
					throw new ArgumentException("Device with specified inventory number already exists");
			}
			else {
				if (existingDevice == null)
					throw new ArgumentException("Device with specified inventory number does not exist");
			}
		}

		private void ThrowIfDeviceTypeNotExists(Guid typeID)
		{
			if (_repo.GetByID<DeviceType>(typeID) == null)
				throw new ArgumentException("Device type with specified ID doesn't exists");
		}

		private Device FormDeviceFrom(DeviceInfo info)
		{
			var newDevice = new Device();

			if (!string.IsNullOrWhiteSpace(info.InventoryNumber))
				newDevice.InventoryNumber = info.InventoryNumber;

			if (!string.IsNullOrWhiteSpace(info.NetworkName))
				newDevice.NetworkName = info.NetworkName;

			if (!string.IsNullOrWhiteSpace(info.ModelName))
				newDevice.ModelName = info.ModelName;

			if (!string.IsNullOrWhiteSpace(info.ModelName))
				newDevice.ModelName = info.ModelName;

			if (info.TypeID != Guid.Empty)
				newDevice.TypeID = info.TypeID;

			return newDevice;
		}
	}
}
