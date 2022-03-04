using System;
using System.Collections.Generic;
using DevSpector.SDK.Models;
using DevSpector.Domain.Models;

namespace DevSpector.Application
{
	public interface IDevicesManager
	{
		void CreateDevice(DeviceInfo info);

		void UpdateDevice(string targetDeviceInventoryNumber, DeviceInfo info);

		void DeleteDevice(string inventoryNumber);

		void MoveDevice(string inventoryNumber, Guid cabinetID);

		Device GetDeviceByInventoryNumber(string inventoryNumber);

		Cabinet GetDeviceCabinet(string inventoryNumber);

		IEnumerable<Device> GetDevices();

		IEnumerable<Appliance> GetAppliances();

		IEnumerable<DeviceType> GetDeviceTypes();
	}
}
