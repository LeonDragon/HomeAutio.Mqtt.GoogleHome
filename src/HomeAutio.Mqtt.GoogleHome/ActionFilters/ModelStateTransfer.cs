﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace HomeAutio.Mqtt.GoogleHome.ActionFilters
{
    /// <summary>
    /// Model state transfer.
    /// </summary>
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        /// <summary>
        /// Temp data key.
        /// </summary>
        protected const string Key = nameof(ModelStateTransfer);

        /// <summary>
        /// Serializes model state.
        /// </summary>
        /// <param name="modelState">The model state dictionary.</param>
        /// <returns>A serialized string.</returns>
        protected static string SerialiseModelState(ModelStateDictionary modelState)
        {
            var errorList = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                });

            return JsonConvert.SerializeObject(errorList);
        }

        /// <summary>
        /// Deserializes model state.
        /// </summary>
        /// <param name="serialisedErrorList">Serialized error list.</param>
        /// <returns>A <see cref="ModelStateDictionary"/>.</returns>
        protected static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
        {
            var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList);
            var modelState = new ModelStateDictionary();

            foreach (var item in errorList)
            {
                modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }

            return modelState;
        }

        /// <summary>
        /// Model state transfer value class.
        /// </summary>
        public class ModelStateTransferValue
        {
            /// <summary>
            /// Key.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Attempted value.
            /// </summary>
            public string AttemptedValue { get; set; }

            /// <summary>
            /// Raw value.
            /// </summary>
            public object RawValue { get; set; }

            /// <summary>
            /// Error messages.
            /// </summary>
            public ICollection<string> ErrorMessages { get; set; } = new List<string>();
        }
    }
}
