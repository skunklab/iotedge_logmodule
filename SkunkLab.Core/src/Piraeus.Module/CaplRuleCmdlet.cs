using Capl.Authorization;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplRule")]
    [OutputType(typeof(Capl.Authorization.Rule))]
    public class CaplRuleCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Truthful evaluation of the rule", Mandatory = true)]
        public bool Evaluates;

        [Parameter(HelpMessage = "Name of issuer (optional)", Mandatory = false)]
        public string Issuer;        

        [Parameter(HelpMessage = "CAPL Operation", Mandatory = true)]
        public EvaluationOperation Operation;

        [Parameter(HelpMessage = "CAPL Match Expression", Mandatory = true)]
        public Match MatchExpression;

        protected override void ProcessRecord()
        {
            //Rule rule = new Rule(this.MatchExpression, this.Operation, this.Evaluates);
           

            Rule rule = new Rule();
            rule.Evaluates = this.Evaluates;
            rule.Operation = this.Operation;
            rule.MatchExpression = this.MatchExpression;

            if (!string.IsNullOrEmpty(this.Issuer))
            {
                rule.Issuer = this.Issuer;
            }

            WriteObject(rule);
        }
    }
}
