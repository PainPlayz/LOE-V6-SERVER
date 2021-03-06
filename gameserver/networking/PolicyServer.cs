﻿#region

using System;
using System.Net;
using System.Net.Sockets;
using LoESoft.Core;

#endregion

namespace LoESoft.GameServer.networking
{
    internal class PolicyServer
    {
        private readonly TcpListener listener;
        private bool started;

        public PolicyServer()
        {
            listener = new TcpListener(IPAddress.Any, 843);
        }

        private static void ServePolicyFile(IAsyncResult ar)
        {
            try
            {
                TcpClient cli = (ar.AsyncState as TcpListener).EndAcceptTcpClient(ar);
                (ar.AsyncState as TcpListener).BeginAcceptTcpClient(ServePolicyFile, ar.AsyncState);
                NetworkStream s = cli.GetStream();
                NReader rdr = new NReader(s);
                NWriter wtr = new NWriter(s);
                if (rdr.ReadNullTerminatedString() == "<policy-file-request/>")
                {
                    wtr.WriteNullTerminatedString(@"<cross-domain-policy>
     <allow-access-from domain=""*"" to-ports=""*"" />
</cross-domain-policy>");
                    wtr.Write((byte)'\r');
                    wtr.Write((byte)'\n');
                }
                cli.Close();
            }
            catch (ObjectDisposedException) { }
            catch (Exception) { }
        }

        public void Start()
        {
            try
            {
                listener.Start();
                listener.BeginAcceptTcpClient(ServePolicyFile, listener);
                started = true;
            }
            catch (ObjectDisposedException) { }
            catch (Exception)
            {
                started = false;
            }
        }

        public void Stop()
        {
            if (started)
                listener.Stop();
        }
    }
}