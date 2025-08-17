using BackEnd.Tcp;
using System.Collections.Generic;
using UnityEngine;

namespace BackEnd
{
    public enum ProtocolType : sbyte
    {
        None,
    }

    public class Message
    {
        public ProtocolType ProtocolType { get; set; }
    }
}
