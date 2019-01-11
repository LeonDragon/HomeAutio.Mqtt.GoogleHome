﻿using System.Collections.Generic;

namespace HomeAutio.Mqtt.GoogleHome.Models
{
    /// <summary>
    /// Command to state key mapper.
    /// </summary>
    public static class CommandToStateKeyMapper
    {
        /// <summary>
        /// Maps a command parameter key to a state key.
        /// </summary>
        /// <param name="commandParameterKey">The command parameter key to map.</param>
        /// <returns>The state key.</returns>
        public static string Map(string commandParameterKey)
        {
            if (string.IsNullOrEmpty(commandParameterKey))
                return commandParameterKey;

            var replacements = new Dictionary<string, string>
            {
                { "color.spectrumRGB", "color.spectrumRgb" },
                { "color.spectrumHSV.hue", "color.spectrumHsv.hue" },
                { "color.spectrumHSV.saturation", "color.spectrumHsv.saturation" },
                { "color.spectrumHSV.value", "color.spectrumHsv.value" },
                { "color.temperature", "color.temperatureK" },
                { "fanSpeed", "currentFanSpeedSetting" },
                { "pause", "isPaused" },
                { "start", "isRunning" },
                { "updateModeSettings", "currentModeSettings" },
                { "updateToggleSettings", "currentToggleSettings" },
                { "volumeLevel", "currentVolume" }
            };

            foreach (var replacement in replacements)
            {
                if (commandParameterKey.StartsWith(replacement.Key))
                {
                    return replacement.Value + commandParameterKey.Substring(replacement.Key.Length);
                }
            }

            return commandParameterKey;
        }
    }
}
