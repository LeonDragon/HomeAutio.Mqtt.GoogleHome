﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HomeAutio.Mqtt.GoogleHome.Models.State;
using HomeAutio.Mqtt.GoogleHome.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HomeAutio.Mqtt.GoogleHome
{
    /// <summary>
    /// Google device repository.
    /// </summary>
    public class GoogleDeviceRepository
    {
        private readonly ILogger<GoogleDeviceRepository> _logger;
        private readonly string _deviceConfigFile;

        private ConcurrentDictionary<string, Device> _devices;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDeviceRepository"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="path">Device config file path.</param>
        public GoogleDeviceRepository(ILogger<GoogleDeviceRepository> logger, string path)
        {
            _logger = logger;
            _deviceConfigFile = path;

            Refresh();
        }

        /// <summary>
        /// Adds a device.
        /// </summary>
        /// <param name="device">Device to add.</param>
        public void Add(Device device)
        {
            _devices.TryAdd(device.Id, device);
        }

        /// <summary>
        /// Determines if the device is contained in the repository.
        /// </summary>
        /// <param name="deviceId">Device id.</param>
        /// <returns><c>true</c> if present, else <c>false</c>.</returns>
        public bool Contains(string deviceId)
        {
            return _devices.ContainsKey(deviceId);
        }

        /// <summary>
        /// Deletes a device.
        /// </summary>
        /// <param name="deviceId">Device Id to delete.</param>
        public void Delete(string deviceId)
        {
            _devices.TryRemove(deviceId, out Device _);
        }

        /// <summary>
        /// Gets a device.
        /// </summary>
        /// <param name="deviceId">Device Id to delete.</param>
        /// <returns>The <see cref="Device"/>.</returns>
        public Device Get(string deviceId)
        {
            if (_devices.TryGetValue(deviceId, out Device value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>A list of <see cref="Device"/>.</returns>
        public IList<Device> GetAll()
        {
            return _devices.Values.ToList();
        }

        /// <summary>
        /// Changes the device ID for a device.
        /// </summary>
        /// <param name="originalId">Original ID.</param>
        /// <param name="newId">New ID.</param>
        public void ChangeDeviceId(string originalId, string newId)
        {
            var device = Get(originalId);
            if (device != null)
            {
                // Ensure internal ID changed and matches.
                if (device.Id != newId)
                {
                    device.Id = newId;
                }

                Add(device);
                Delete(originalId);
            }
        }

        /// <summary>
        /// Persists current device confiuguration.
        /// </summary>
        public void Persist()
        {
            var deviceString = JsonConvert.SerializeObject(_devices, Formatting.Indented);
            File.WriteAllText(_deviceConfigFile, deviceString);
            _logger.LogInformation($"Persisting device configuration changes to {_deviceConfigFile}");
        }

        /// <summary>
        /// Refreshes deice configuration from file.
        /// </summary>
        public void Refresh()
        {
            if (File.Exists(_deviceConfigFile))
            {
                var deviceConfigurationString = File.ReadAllText(_deviceConfigFile);
                _devices = new ConcurrentDictionary<string, Device>(JsonConvert.DeserializeObject<Dictionary<string, Device>>(deviceConfigurationString));

                // Validate the config
                foreach (var device in _devices)
                {
                    var errors = DeviceValidator.Validate(device.Value);
                    if (errors.Count() > 0)
                    {
                        _logger.LogWarning("GoogleDevices.json issues detected for device {Device}: {DeviceConfigIssues}", device.Key, errors);
                    }
                }

                _logger.LogInformation($"Loaded device configuration from {_deviceConfigFile}");
            }
            else
            {
                _devices = new ConcurrentDictionary<string, Device>();
                _logger.LogInformation($"Device configuration file {_deviceConfigFile} not found, starting with empty set");
            }
        }
    }
}
