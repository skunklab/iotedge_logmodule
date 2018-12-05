/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Issuance
{
    public static class IssueConstants
    {
        public static class Namespaces
        {
            public const string Xmlns = "http://schemas.authz.org/cipl";
        }

        public static class Elements
        {
            public const string IssuePolicy = "IssuePolicy";
        }

        public static class Attributes
        {
            public const string PolicyId = "PolicyID";
            public const string Mode = "Mode";
        }

        public static class IssueModes
        {
            public const string Unique = "http://schemas.authz.org/cipl/mode#Unique";
            public const string Aggregate = "http://schemas.authz.org/cipl/mode#Aggregate";
        }
    }
}
