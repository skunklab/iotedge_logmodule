using Capl.Authorization;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplLogicalAnd")]
    public class CaplLogicalAnd : Cmdlet
    {
        [Parameter(HelpMessage = "Truthful evaluation of the logical AND", Mandatory = true)]
        public bool Evaluates;

        [Parameter(HelpMessage = "Array of Terms (Rules, Logical OR, Logical AND) or any combinations", Mandatory = true)]
        public Term[] Terms;

        protected override void ProcessRecord()
        {
            LogicalAndCollection lac = new LogicalAndCollection();
            lac.Evaluates = this.Evaluates;
            foreach (Term term in this.Terms)
            {
                lac.Add(term);
            }

            WriteObject(lac);
        }
    }
}
