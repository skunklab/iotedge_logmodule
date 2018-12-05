using Capl.Authorization.Operations;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplOperation")]
    [OutputType(typeof(Capl.Authorization.EvaluationOperation))]
    public class CaplOperationCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Value of Operation (optional)", Mandatory = true)]
        public string Value;

        [Parameter(HelpMessage = "Type of operation", Mandatory = true)]
        public OperationType Type;

        protected override void ProcessRecord()
        {
            Uri operationUri = null;
            if (this.Type == OperationType.BetweenDateTime)
            {
                operationUri = BetweenDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.Contains)
            {
                operationUri = ContainsOperation.OperationUri;
            }
            else if (this.Type == OperationType.Equal)
            {
                operationUri = EqualOperation.OperationUri;
            }
            else if (this.Type == OperationType.EqualDateTime)
            {
                operationUri = EqualDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.EqualNumeric)
            {
                operationUri = EqualNumericOperation.OperationUri;
            }
            else if (this.Type == OperationType.Exists)
            {
                operationUri = ExistsOperation.OperationUri;
            }
            else if (this.Type == OperationType.GreaterThan)
            {
                operationUri = GreaterThanOperation.OperationUri;
            }
            else if (this.Type == OperationType.GreaterThanDateTime)
            {
                operationUri = GreaterThanOrEqualDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.GreaterThanOrEqual)
            {
                operationUri = GreaterThanOrEqualOperation.OperationUri;
            }
            else if (this.Type == OperationType.GreaterThanOrEqualDateTime)
            {
                operationUri = GreaterThanOrEqualDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.LessThan)
            {
                operationUri = LessThanOperation.OperationUri;
            }
            else if (this.Type == OperationType.LessThanDateTime)
            {
                operationUri = LessThanOrEqualDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.LessThanOrEqual)
            {
                operationUri = LessThanOrEqualOperation.OperationUri;
            }
            else if (this.Type == OperationType.LessThanOrEqualDateTime)
            {
                operationUri = LessThanOrEqualDateTimeOperation.OperationUri;
            }
            else if (this.Type == OperationType.NotEqual)
            {
                operationUri = NotEqualOperation.OperationUri;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Type");
            }

            Capl.Authorization.EvaluationOperation operation = new Capl.Authorization.EvaluationOperation(operationUri, this.Value);

            WriteObject(operation);
        }
    }
}
