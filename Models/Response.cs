﻿using System;

namespace Models
{
    [Serializable]
    public class Response
    {
        public enum Operation
        {
            Login,
            BlockLogin,
            EntrarChat,
            LeaveChat, // Operação desnecessária???
            SendMessage,
            SendMessageFile,
            PedirFile,
            GetUserInfo,
            NewUserOnline,
            SendFile
        }

        public Operation Operacao { get; set; }

        public Mensagem Mensagem { get; set; }

        // public List<Mensagem> HistoricoChat { get; set; }
        public Utilizador Utilizador { get; set; }

        public Response()
        {
        }

        /// <summary>
        /// Construtor de uma Response
        /// </summary>
        /// <param name="operacao">Operação</param>
        /// <param name="utilizador">Utilizador que a cria</param>
        /// <param name="mensagem">Mensagem da Response</param>
        // /// <param name="historicoChat"></param>
        public Response(Operation operacao, Utilizador utilizador,
            Mensagem mensagem = null /*, List<Mensagem> historicoChat=null*/)
        {
            Operacao = operacao;
            Utilizador = utilizador;
            Mensagem = mensagem;
            // HistoricoChat = historicoChat;
        }
    }
}