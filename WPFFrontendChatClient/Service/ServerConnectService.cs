﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Models;

namespace WPFFrontendChatClient.Service
{
    public class ServerConnectService
    {
        private readonly TcpClient _tcpClient;
        private string IpAddress { get; set; }
        private int Port { get; set; }
        public Utilizador UtilizadorLigado { get; set; }

        public delegate void AddAlunoAction(Utilizador utilizador);

        public event AddAlunoAction AddAlunoEvent;

        public delegate void AddMensagemRecebidaActionScs(Mensagem mensagem);

        public event AddMensagemRecebidaActionScs AddMensagemRecebidaEventScs;

        public delegate void AddUnidadeCurricularAction(UnidadeCurricular unidadeCurricular);

        public event AddUnidadeCurricularAction AddUnidadeCurricularEvent;

        /// <summary>
        /// Construtor do ServerConnectService
        /// <para>Estabelece ligação com o servidor.</para>
        /// </summary>
        public ServerConnectService()
        {
            IpAddress = "tp1cd.ddns.net";
            // IpAddress = "192.168.1.65";
            Port = int.Parse("1000");

            // IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
            IPEndPoint ipEndPoint = new IPEndPoint(Dns.GetHostEntry(IpAddress).AddressList[0], Port);

            _tcpClient = new TcpClient();
            _tcpClient.Connect(ipEndPoint);
        }

        /// <summary>
        /// Inicia a conexão do Utilizador
        /// </summary>
        /// <param name="utilizador">Utilizador que vai iniciar conexão</param>
        public void Start(Utilizador utilizador)
        {
            Response resLogin = new Response(Response.Operation.Login, utilizador);
            Helpers.SendSerializedMessage(_tcpClient, resLogin);
            // Espera pela mensagem do servidor com os dados do user.
            Boolean flagHaveUser = false;
            while (!flagHaveUser)
            {
                Response resGetUserInfo = Helpers.ReceiveSerializedMessage(_tcpClient);
                UtilizadorLigado = resGetUserInfo.Utilizador;
                flagHaveUser = true;
                /*
                 *     Adiciona as Unidades Curriculares do Curso e Extras à lista de Aulas para podermos abrir
                 * separadores de chat.
                 *     Para isso invoca um evento que é capturado no "MainViewModel",
                 */
                UtilizadorLigado.Curso?.UnidadesCurriculares?.ForEach(unidadeCurricular =>
                    AddUnidadeCurricularEvent?.Invoke(unidadeCurricular));
                UtilizadorLigado.UnidadesCurriculares?.ForEach(unidadeCurricular =>
                    AddUnidadeCurricularEvent?.Invoke(unidadeCurricular));
            }

            MessageHandler();
        }

        /// <summary>
        /// Trata as mensagens recebidas
        /// </summary>
        private void MessageHandler()
        {
            Thread messageHandlerThread = new Thread(() =>
            {
                while (true)
                {
                    Response response = Helpers.ReceiveSerializedMessage(_tcpClient);
                    switch (response.Operacao)
                    {
                        case Response.Operation.EntrarChat:
                        {
                            break;
                        }
                        case Response.Operation.SendMessage:
                        {
                            Application.Current.Dispatcher?.Invoke(delegate
                            {
                                AddMensagemRecebidaEventScs?.Invoke(response.Mensagem);
                            });
                            break;
                        }
                        case Response.Operation.LeaveChat:
                        {
                            break;
                        }
                        case Response.Operation.Login:
                        {
                            break;
                        }
                        case Response.Operation.GetUserInfo:
                        {
                            break;
                        }
                        case Response.Operation.NewUserOnline:
                        {
                            Application.Current.Dispatcher?.Invoke(delegate
                            {
                                AddAlunoEvent?.Invoke(response.Utilizador);
                            });
                            break;
                        }
                        case Response.Operation.BlockLogin:
                        {
                            break;
                        }
                    }
                }
            });
            messageHandlerThread.Start();
        }

        /// <summary>
        /// Envia a Mensagem para o servidor
        /// </summary>
        /// <param name="mensagem">Mensagem a enviar</param>
        public void EnviarMensagem(Mensagem mensagem)
        {
            Response resp = new Response(Response.Operation.SendMessage, UtilizadorLigado, mensagem);
            Helpers.SendSerializedMessage(_tcpClient, resp);
        }

        /// <summary>
        /// Cria a Mensagem e Response para o envio do ficheiro.
        /// <para>Executa o procedimento de envio de ficheiro.</para>
        /// </summary>
        /// <param name="caminhoFicheiro">Caminho do ficheiro</param>
        /// <param name="mensagem">Mensagem a aparecer no chat</param>
        public void EnviarFicheiro(string caminhoFicheiro, Mensagem mensagem)
        {
            // Criar mensagem para aparecer no chat
            Response resp = new Response(Response.Operation.SendFile, UtilizadorLigado, mensagem);
            Helpers.SendSerializedMessage(_tcpClient, resp);
            Helpers.SendFile(_tcpClient, Path.GetExtension(caminhoFicheiro), caminhoFicheiro);
        }
    }
}