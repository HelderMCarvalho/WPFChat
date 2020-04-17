﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClassLibrary;

namespace WPFFrontendChatClient.Service
{
    public class ServerConnectService
    {
        private TcpClient _tcpClient;
        private IPEndPoint _ipEndPoint;
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public ServerConnectService()
        {
        }

        /// <summary>
        /// Inicia a conexão do Utilizador
        /// </summary>
        /// <param name="utilizador">Utilizador que vai iniciar conexão</param>
        /// <typeparam name="T">Tipo de Utilizador</typeparam>
        public void Start<T>(T utilizador)
        {
            _ipEndPoint = new IPEndPoint(Dns.GetHostEntry(IpAddress).AddressList[0], Port);
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_ipEndPoint);
            Response<T> response = new Response<T>(Response<T>.Operation.Login, utilizador);
            Helpers.sendSerializedMessage(_tcpClient, response);
        }


        public static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int received = 0; // how many bytes is already received
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex; // any serious error occurr
                }
            } while (received < size);
        }
    }
}