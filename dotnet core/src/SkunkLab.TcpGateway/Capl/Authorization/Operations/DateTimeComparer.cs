/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization.Operations
{
    using System;
    using System.Collections;
    using System.Xml;

    /// <summary>
    /// Compares two DateTime types by UTC.
    /// </summary>
    public class DateTimeComparer : IComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeComparer"/> class.
        /// </summary>
        public DateTimeComparer()
        {
        }

        #region IComparer Members

        /// <summary>
        /// Compares two DateTime types by UTC.
        /// </summary>
        /// <param name="x">LHS datetime parameter to test.</param>
        /// <param name="y">RHS datatime parameter to test.</param>
        /// <returns>0 for equality; 1 for x greater than y; -1 for x less than y.</returns>
        public int Compare(object x, object y)
        {
            DateTime left = XmlConvert.ToDateTime((string)x, XmlDateTimeSerializationMode.Utc);
            DateTime right = XmlConvert.ToDateTime((string)y, XmlDateTimeSerializationMode.Utc);

            if (left == right)
            { 
                return 0; 
            }

            if (left < right) 
            { 
                return -1; 
            }

            return 1;
        }

        #endregion
    }
}
