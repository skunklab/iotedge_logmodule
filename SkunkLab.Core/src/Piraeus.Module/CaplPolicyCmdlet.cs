using System;
using System.Management.Automation;
using Capl.Authorization;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplPolicy")]
    public class CaplPolicyCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Uniquely identifies the policy as a URI", Mandatory = true)]
        public string PolicyID;

        [Parameter(HelpMessage = "(Optional) Determines if the policy should use delegation.", Mandatory = false)]
        public bool Delegation;

        [Parameter(HelpMessage = "Evaluation expression (Rule, LogicalAnd, LogicalOr)", Mandatory = true)]
        public Term EvaluationExpression;

        [Parameter(HelpMessage = "(Optional) transforms", Mandatory = false)]
        public Transform[] Transforms;
        protected override void ProcessRecord()
        {
            AuthorizationPolicy policy = new AuthorizationPolicy(this.EvaluationExpression, new Uri(this.PolicyID), this.Delegation);

            if(this.Transforms != null && this.Transforms.Length > 0)
            {
                foreach(Transform transform in this.Transforms)
                {
                    policy.Transforms.Add(transform);
                }
            }

            WriteObject(policy);
        }
    }


    //[Cmdlet(VerbsCommon.Add, "CaplPolicy")]
    //public class CaplPolicyAdd : Cmdlet
    //{
    //    [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
    //    public string ServiceUrl;

    //    [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
    //    public string SecurityToken;

    //    [Parameter(HelpMessage = "CAPL Policy to load.", Mandatory = true)]
    //    public AuthorizationPolicy Policy;


    //    protected override void ProcessRecord()
    //    {
    //        RestRequestBuilder builder = new RestRequestBuilder("POST", String.Format("{0}/{1}",this.ServiceUrl,"api2/accesscontrol"), "application/xml", false, this.SecurityToken);
    //        RestRequest request = new RestRequest(builder);
    //        request.Post<AuthorizationPolicy>(this.Policy);
    //    }
    //}

    //[Cmdlet(VerbsCommon.Remove, "CaplPolicy")]
    //public class CaplPolicyRemove : Cmdlet
    //{
    //    [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
    //    public string Url;

    //    [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
    //    public string SecurityToken;

    //    [Parameter(HelpMessage = "Policy ID used to return the CAPL policy.", Mandatory = true)]
    //    public string PolicyID;

    //    protected override void ProcessRecord()
    //    {
    //        string url = String.Format("{0}/api/policy?policyId={1}", this.Url, this.PolicyID);
    //        RestRequestBuilder builder = new RestRequestBuilder("DELETE", url, "application/json", true, this.SecurityToken);
    //        RestRequest request = new RestRequest(builder);
    //        request.Delete();

    //    }
    //}

    //[Cmdlet(VerbsCommon.Get, "CaplPolicy")]
    //public class CaplPolicyGet : Cmdlet
    //{
    //    [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
    //    public string Url;

    //    [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
    //    public string SecurityToken;

    //    [Parameter(HelpMessage = "Policy ID used to return the CAPL policy.", Mandatory = true)]
    //    public string PolicyID;

    //    protected override void ProcessRecord()
    //    {
    //        string url = String.Format("{0}/api/policy?PolicyId={1}", this.Url, this.PolicyID);
    //        RestRequestBuilder builder = new RestRequestBuilder("GET", url, "application/xml", true, this.SecurityToken);
    //        RestRequest request = new RestRequest(builder);
    //        WriteObject(request.Get<AuthorizationPolicy>());
    //    }
    //}

    //[Cmdlet(VerbsData.Update, "CaplPolicy")]
    //public class CaplPolicyUpdate : Cmdlet
    //{
    //    [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
    //    public string Url;

    //    [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
    //    public string SecurityToken;

    //    [Parameter(HelpMessage = "CAPL Policy to update.", Mandatory = true)]
    //    public AuthorizationPolicy Policy;

    //    protected override void ProcessRecord()
    //    {
    //        RestRequestBuilder builder = new RestRequestBuilder("PUT", this.Url, "application/xml", false, this.SecurityToken);
    //        RestRequest request = new RestRequest(builder);
    //        request.Put<AuthorizationPolicy>(this.Policy);
    //    }
    //}

    //[Cmdlet(VerbsCommon.Find, "CaplPolicy")]
    //public class CaplPolicyFind : Cmdlet
    //{
    //    [Parameter(HelpMessage = "Url of the CAPL REST service.", Mandatory = true)]
    //    public string Url;

    //    [Parameter(HelpMessage = "Security token used to access the CAPL REST service.", Mandatory = true)]
    //    public string SecurityToken;

    //    protected override void ProcessRecord()
    //    {
    //        RestRequestBuilder builder = new RestRequestBuilder("GET", this.Url, "application/json", true, this.SecurityToken);
    //        RestRequest request = new RestRequest(builder);
    //        WriteObject(request.Get<string[]>());
    //    }
    //}
}
