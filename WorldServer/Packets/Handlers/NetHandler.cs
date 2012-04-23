﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Constans;
using Common.Network.Packets;
using WorldServer.Network;

namespace WorldServer.Packets.Handlers
{
    public class NetHandler
    {
        public static void HandlePing(ref PacketReader packet, ref WorldManager manager)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_PONG, 4);
            writer.WriteUInt32(packet.ReadUInt32());

            manager.Send(writer);
        }
    }
}
