/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization.Matching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    


    public class MatchExpressionDictionary : IDictionary<string, MatchExpression>
    {
        public MatchExpressionDictionary()
        {
            this.expressions = new Dictionary<string, MatchExpression>();
        }

        private Dictionary<string, MatchExpression> expressions;

        private static MatchExpressionDictionary defaultInstance;

        public static MatchExpressionDictionary Default
        {
            get
            {
                if(defaultInstance != null)
                {
                    return defaultInstance;
                }

                Action<Type, MatchExpressionDictionary> addOpAsType;
                Action<MatchExpression, MatchExpressionDictionary> addOpAsInstance;
                MatchExpressionDictionary dict = new MatchExpressionDictionary();
                addOpAsType = (typeRef, op) =>
                {
                    MatchExpression matchExpression = (MatchExpression)Activator.CreateInstance(Type.GetType(typeRef.FullName));
                    op.Add(matchExpression.Uri.ToString(), matchExpression);
                };

                addOpAsInstance = (instance, op) =>
                {
                    op.Add(instance.Uri.ToString(), instance);
                };

                addOpAsType(typeof(LiteralMatchExpression), dict);
                addOpAsType(typeof(PatternMatchExpression), dict);
                addOpAsType(typeof(ComplexTypeMatchExpression), dict);
                addOpAsType(typeof(UnaryMatchExpression), dict);

                defaultInstance = dict;
                return defaultInstance;
            }
        }


        #region IDictionary<string,MatchExpression> Members

        /// <summary>
        /// Adds a new match expression.
        /// </summary>
        /// <param name="key">The key that identifies the match expression.</param>
        /// <param name="value">The match expression instance.</param>
        public void Add(string key, MatchExpression value)
        {
            expressions.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return expressions.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return expressions.Keys; }
        }

        public bool Remove(string key)
        {
            return expressions.Remove(key);
        }

        public bool TryGetValue(string key, out MatchExpression value)
        {
            return expressions.TryGetValue(key, out value);
        }

        public ICollection<MatchExpression> Values
        {
            get { return expressions.Values; }
        }

        public MatchExpression this[string key]
        {
            get { return expressions[key]; }
            set { expressions[key] = value; }
        }

        #endregion

        #region ICollection<KeyValuePair<string,MatchExpression>> Members

        public void Add(KeyValuePair<string, MatchExpression> item)
        {
            ((ICollection<KeyValuePair<string, MatchExpression>>)expressions).Add(item);
        }

        public void Clear()
        {
            expressions.Clear();
        }

        public bool Contains(KeyValuePair<string, MatchExpression> item)
        {
            return ((ICollection<KeyValuePair<string, MatchExpression>>)expressions).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, MatchExpression>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, MatchExpression>>)expressions).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return expressions.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, MatchExpression> item)
        {
            return ((ICollection<KeyValuePair<string, MatchExpression>>)expressions).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,MatchExpression>> Members

        public IEnumerator<KeyValuePair<string, MatchExpression>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<string, MatchExpression>>)expressions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return expressions.GetEnumerator();
        }

        #endregion
    }
}
