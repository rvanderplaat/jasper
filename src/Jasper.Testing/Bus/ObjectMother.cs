﻿using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;
using NSubstitute;

namespace Jasper.Testing.Bus
{
    public static class ObjectMother
    {
        public static Envelope Envelope()
        {
            return new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                Callback = Substitute.For<IMessageCallback>(),
                Headers = new Dictionary<string, string>{{Jasper.Bus.Runtime.Envelope.MessageTypeKey, "Something"}},
                CorrelationId = Guid.NewGuid().ToString()
            };
        }
    }
}