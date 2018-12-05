using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusIotHubDeviceSubscription")]
    public class AddAzureIotHubDeviceSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Account name of IoT Hub, e.g, <account>.azure-devices.net", Mandatory = true)]
        public string Account;

        [Parameter(HelpMessage = "Device ID that will send messages to IoT Hub.", Mandatory = true)]
        public string DeviceId;

        [Parameter(HelpMessage = "(Optional) property name to use when sending to device, i.e., used with property value.", Mandatory = false)]
        public string PropertyName;

        [Parameter(HelpMessage = "(Optional) property value to use when sending to device, i.e., used with property name.", Mandatory = false)]
        public string PropertyValue;

        [Parameter(HelpMessage = "SAS token used for authentication.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Description of the subscription.", Mandatory = false)]
        public string Description;

        protected override void ProcessRecord()
        {
            string uriString = String.Format("iothub://{0}.azure-devices.net?deviceid={1}", Account, DeviceId);

            if (!String.IsNullOrEmpty(PropertyName))
            {
                uriString = String.Format("{0}&propname={1}&propvalue={1}", uriString, PropertyName, PropertyValue);
            }

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = Key,
                Description = this.Description
            };

            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);
        }
    }
}
