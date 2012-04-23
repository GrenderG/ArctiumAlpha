﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.Logging;
using Common.Network;
using Common.Network.Packets;
using WorldServer.Packets;
using System.IO;

namespace WorldServer.Network
{
    public class WorldManager
    {
        public Socket socket;
        public static WorldSocket WorldSession;
        byte[] buffer = null;

        public void OnData()
        {
            PacketReader pkt = new PacketReader(buffer);

            if (Enum.IsDefined(typeof(Opcodes), pkt.Opcode))
                Log.Message(LogType.DUMP, "Recieved OPCODE: {0}, LENGTH: {1}", pkt.Opcode, pkt.Size);
            else
                Log.Message(LogType.DUMP, "UNKNOWN OPCODE: {0}, LENGTH: {1}", pkt.Opcode, pkt.Size);

            Log.Message();
            PacketManager.InvokeHandler(pkt, this, pkt.Opcode);
            Log.Message();
        }

        public void Recieve()
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_AUTH_CHALLENGE, 6, false);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            this.Send(writer);

            while (WorldSession.listenWorldSocket)
            {
                Thread.Sleep(200);
                if (socket.Available > 0)
                {
                    buffer = new byte[socket.Available];
                    socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    OnData();
                }
            }

            socket.Close();
        }

        public void Send(PacketWriter packet)
        {
            byte[] buffer = packet.ReadDataToSend();
            Log.Message();
            foreach (byte b in buffer)
            {
                Console.Write("{0:X} ", b);
            }
            Log.Message();
            try
            {
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(FinishSend), socket);

                Log.Message(LogType.DUMP, "Send {0}.", packet.Opcode);
                Log.Message();
            }
            catch (Exception ex)
            {
                Log.Message(LogType.ERROR, "{0}", ex.Message);
                Log.Message();
                socket.Close();
            }
        }

        public void FinishSend(IAsyncResult result)
        {
            socket.EndSend(result);
        }
    }
}
