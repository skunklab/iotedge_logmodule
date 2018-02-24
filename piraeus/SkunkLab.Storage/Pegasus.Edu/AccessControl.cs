using Capl.Authorization;
using Capl.Authorization.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegasus.Edu
{
    public class AccessControl
    {

        



        public static AuthorizationPolicy GetPolicy(string policyUriString, string claimType, string claimValue)
        {            

            //operation
            EvaluationOperation operation = new EvaluationOperation(new Uri(AuthorizationConstants.OperationUris.Equal), claimValue);

            //a claim to match for the operation
            Match match = new Match() { ClaimType = claimType, Required = true, Type = LiteralMatchExpression.MatchUri };

            //a rule that encapsulates the match expression and operation with a truthful evaluation
            Rule rule = new Rule() { MatchExpression = match, Operation = operation, Evaluates = true };

            //Policy for the rule
            AuthorizationPolicy policy = new AuthorizationPolicy();
            policy.PolicyId = new Uri(policyUriString);
            policy.Expression = rule;

            return policy;
        }
    }
}
