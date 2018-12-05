using System;
using System.Management.Automation;
using Capl.Authorization;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplMatch")]
    [OutputType(typeof(Capl.Authorization.Match))]
    public class CaplMatchCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Claim type to match", Mandatory = true)]
        public string ClaimType;

        [Parameter(HelpMessage = "Determines whether the claim type is required to match.", Mandatory = true)]
        public bool Required;

        [Parameter(HelpMessage = "Type", Mandatory = true)]
        public MatchType Type;

        [Parameter(HelpMessage = "Value of match expression (optional)", Mandatory = false)]
        public string Value;

        protected override void ProcessRecord()
        {
            Uri matchUri = null;
            if (this.Type == MatchType.Literal)
            {
                matchUri = Capl.Authorization.Matching.LiteralMatchExpression.MatchUri;
            }
            else if (this.Type == MatchType.Pattern)
            {
                matchUri = Capl.Authorization.Matching.PatternMatchExpression.MatchUri;
            }
            else if (this.Type == MatchType.ComplexType)
            {
                matchUri = Capl.Authorization.Matching.ComplexTypeMatchExpression.MatchUri;
            }
            else if (this.Type == MatchType.Unary)
            {
                matchUri = Capl.Authorization.Matching.UnaryMatchExpression.MatchUri;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Type");
            }
            
            WriteObject(new Match() { ClaimType = this.ClaimType, Required = this.Required, Type = matchUri });
        }
    }
}
