using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Edge.Gateway
{
    public class RetryEventArgs : EventArgs
    {
        public RetryEventArgs(int attempts, int maxRetries, SigmoidType factor, TimeSpan sigmoidTime)
        {
            SigmoidTime = sigmoidTime;
            Attempts = attempts;
            MaxRetries = maxRetries;
            SigmoidFactor = factor;
        }

        public int Attempts { get; internal set; }

        public TimeSpan SigmoidTime { get; internal set; }

        public int MaxRetries { get; internal set; }

        public SigmoidType SigmoidFactor { get; internal set; }
    }
}
