/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Security.Claims;
    using System.Runtime.Serialization;

    /// <summary>
    /// An abstract logical connective.
    /// </summary>
    [Serializable]
    public abstract class LogicalConnectiveCollection : Term, IList<Term>
    {
        /// <summary>
        /// A list of terms.
        /// </summary>
        private List<Term> list;

        /// <summary>
        /// Truthful evaluation of the logical connective.
        /// </summary>
        private bool _evaluates;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalConnectiveCollection"/> class.
        /// </summary>
        protected LogicalConnectiveCollection()
        {
            this.list = new List<Term>();
            this._evaluates = true;    
        }

        /// <summary>
        /// Gets or sets a value indicating whether the truthful evaluation of the logical connective is true or false.
        /// </summary>
        public override bool Evaluates
        {
            get { return this._evaluates; }
            set { this._evaluates = value; }
        }
        
        /// <summary>
        /// Gets or sets and option id for the term.
        /// </summary>
        public override Uri TermId { get; set; }

        /// <summary>
        /// Gets the number of element actually contained in the Capl.Authorization.LogicalConectiveCollection.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the terms to be evaluated by the logical connective.
        /// </summary>
        /// <param name="index">Index of the term.</param>
        /// <returns>A term of the logical connective.</returns>
        public Term this[int index]
        {
            get { return this.list[index]; }
            set { this.list[index] = value; }
        }

        ///// <summary>
        ///// Evaluates a set of claims.
        ///// </summary>
        ///// <param name="claimSet">The set of claims to evaluate.</param>
        ///// <returns>True, if the evaluation is true; otherwise false.</returns>
        //public abstract bool Evaluate(ClaimCollection claims);

        
        /// <summary>
        /// Index of a term.
        /// </summary>
        /// <param name="item">Term to return for the index.</param>
        /// <returns>Index of the term.</returns>
        public int IndexOf(Term item)
        {
            return this.list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an elmenent into the Capl.Authorization.LogicalConnectiveCollection at the specified location.
        /// </summary>
        /// <param name="index">Index to insert.</param>
        /// <param name="item">Term to insert.</param>
        public void Insert(int index, Term item)
        {
            this.list.Insert(index, item);
        }

        /// <summary>
        /// Removes the element at a specified index from the Capl.Authorization.LogicalConectiveCollection
        /// </summary>
        /// <param name="index">Index of the term to remove.</param>
        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        /// <summary>
        /// Adds an object to the end of the Capl.Authorization.LogicalConnectiveCollection.
        /// </summary>
        /// <param name="item">Term to add to the collection.</param>
        public void Add(Term item)
        {
            this.list.Add(item);
        }

        /// <summary>
        /// Removes all element from the Capl.Authorization.LogicalConnectiveCollection.
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
        }

        /// <summary>
        /// Determines whether an element in the Capl.Authorization.LogicalConnectiveCollection
        /// </summary>
        /// <param name="item">Term to test for existence.</param>
        /// <returns>True, if the term exists; otherwise false.</returns>
        public bool Contains(Term item)
        {
            return this.list.Contains(item);
        }

        /// <summary>
        /// Copies the entire Capl.Authorization.LogicalConnectiveCollection to a compatible one-dimensional array System.Array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">Array to copy terms.</param>
        /// <param name="arrayIndex">Index to begin the copy to the array.</param>
        public void CopyTo(Term[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remmoves the first occurrence of a specifed object from the Capl.Authorization.LogicalConectiveCollection
        /// </summary>
        /// <param name="item">Term to remove.</param>
        /// <returns>True, if the term is removed; otherwise false.</returns>
        public bool Remove(Term item)
        {
            return this.list.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Capl.Authorization.LogicalAndCollection
        /// </summary>
        /// <returns>Returns an IEnumerator</returns>
        public IEnumerator<Term> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator for the collection.
        /// </summary>
        /// <returns>Returns an IEnumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }        
    }
}
