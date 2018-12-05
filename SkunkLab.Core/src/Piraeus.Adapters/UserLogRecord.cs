using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Adapters
{
    public class UserLogRecord : TableEntity
    {
        public UserLogRecord()
        {
        }

        public UserLogRecord(string channelId, string identity, string claimType, string channel, string protocol, string status, DateTime loginTime)
        {
            ChannelId = channelId;
            Identity = identity;
            ClaimType = claimType;
            Channel = channel;
            Protocol = protocol;
            Status = status;
            LoginTime = loginTime;
        }

        public UserLogRecord(DateTime logoutTime)
        {
            LogoutTime = logoutTime;
        }

        public string ChannelId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Identity
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Channel { get; set; }

        public string Protocol { get; set; }

        public string ClaimType { get; set; }

        public string Status { get; set; }

        public DateTime? LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }
    }
}
