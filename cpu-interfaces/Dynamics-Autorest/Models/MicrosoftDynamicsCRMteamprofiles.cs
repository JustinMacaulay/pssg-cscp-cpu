// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Gov.Jag.VictimServices.Interfaces.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// teamprofiles
    /// </summary>
    public partial class MicrosoftDynamicsCRMteamprofiles
    {
        /// <summary>
        /// Initializes a new instance of the MicrosoftDynamicsCRMteamprofiles
        /// class.
        /// </summary>
        public MicrosoftDynamicsCRMteamprofiles()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftDynamicsCRMteamprofiles
        /// class.
        /// </summary>
        public MicrosoftDynamicsCRMteamprofiles(string teamprofileid = default(string), long? versionnumber = default(long?), string teamid = default(string), string fieldsecurityprofileid = default(string))
        {
            Teamprofileid = teamprofileid;
            Versionnumber = versionnumber;
            Teamid = teamid;
            Fieldsecurityprofileid = fieldsecurityprofileid;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "teamprofileid")]
        public string Teamprofileid { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "versionnumber")]
        public long? Versionnumber { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "teamid")]
        public string Teamid { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "fieldsecurityprofileid")]
        public string Fieldsecurityprofileid { get; set; }

    }
}
