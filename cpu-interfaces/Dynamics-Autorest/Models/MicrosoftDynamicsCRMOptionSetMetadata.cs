// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Gov.Jag.VictimServices.Interfaces.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// OptionSetMetadata
    /// </summary>
    public partial class MicrosoftDynamicsCRMOptionSetMetadata
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MicrosoftDynamicsCRMOptionSetMetadata class.
        /// </summary>
        public MicrosoftDynamicsCRMOptionSetMetadata()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// MicrosoftDynamicsCRMOptionSetMetadata class.
        /// </summary>
        public MicrosoftDynamicsCRMOptionSetMetadata(IList<MicrosoftDynamicsCRMOptionMetadata> options = default(IList<MicrosoftDynamicsCRMOptionMetadata>))
        {
            Options = options;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Options")]
        public IList<MicrosoftDynamicsCRMOptionMetadata> Options { get; set; }

    }
}
