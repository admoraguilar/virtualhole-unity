﻿using System;

namespace EditorStatsReporting.Parameters.User
{
    public class ClientId : Parameter
    {
        public ClientId(Guid value): base(value)
        {
        }

        public ClientId(string value) : base(value)
        {
            
        }

        public override string Name
        {
            get { return "cid"; }
        }

        public override Type ValueType
        {
            get { return Value.GetType(); }
        }
    }
}
