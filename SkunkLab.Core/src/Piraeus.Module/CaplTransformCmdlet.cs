using Capl.Authorization;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{

    [Cmdlet(VerbsCommon.New, "CaplTransform")]
    public class CaplTransformCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Type of transform", Mandatory = true)]
        public TransformType Type;

        [Parameter(HelpMessage = "Match expression.", Mandatory = true)]
        public Match MatchExpression;

        [Parameter(HelpMessage = "Required claim for 'add' and 'replace' transforms. Not used for 'remove' transform.", Mandatory = false)]
        public LiteralClaim TargetClaim;

        [Parameter(HelpMessage = "An evaluation expression that determines if the transform is applied (optional).", Mandatory = false)]
        public Term EvaluationExpression;

        protected override void ProcessRecord()
        {
            Uri uri = null;

            if(this.Type == TransformType.Add)
            {
                uri = new Uri(AuthorizationConstants.TransformUris.Add);
            }
            else if(this.Type == TransformType.Remove)
            {
                uri = new Uri(AuthorizationConstants.TransformUris.Remove);
            }
            else if(this.Type == TransformType.Replace)
            {
                uri = new Uri(AuthorizationConstants.TransformUris.Replace);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Type");
            }

            ClaimTransform transform = new ClaimTransform(uri, this.MatchExpression, this.TargetClaim);
            transform.Expression = this.EvaluationExpression;

            WriteObject(transform);
        }

    }
}
