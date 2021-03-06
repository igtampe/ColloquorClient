﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Switchboard {

    //In the event of using this outside the console, probably remove the Basicrender calls.
    public class SwitchboardClient {

        //------------------------------[Variables]------------------------------

        /// <summary>Main TCP Client</summary>
        private readonly TcpClient Client;

        /// <summary>haha River like a larger stream ahahaha</summary>
        private NetworkStream River;

        /// <summary>Whether or not the client is connected to the server.</summary>
        public bool Connected => Client.Connected;

        /// <summary>Whether or not the client has data available to read.</summary>
        public bool Available => River.DataAvailable;

        /// <summary>IP of the remote server.</summary>
        private readonly String IP;

        /// <summary>Port of the remote server</summary>
        private readonly int Port;

        /// <summary>
        /// Indicates wether the client is busy or not.
        /// Should help prevent a user/programmer from attempting to send/receive data when the client is already trying to send/receive data.
        /// </summary>
        private bool Busy;

        /// <summary>Result of a login attempt.</summary>
        public enum LoginResult {
            /// <summary>Successfully logged in</summary>
            SUCCESS = 0,

            /// <summary>Invalid login credentials were sent</summary>
            INVALID = 1,

            /// <summary>Already logged in on this connection</summary>
            ALREADY = 2,

            /// <summary>Already logged in on another connection</summary>
            OTHERLOCALE = 3
        }

        //------------------------------[Constructor]------------------------------

        /// <summary>Generates a Switchboard Client, but does not start it</summary>
        public SwitchboardClient(String IP,int Port) {
            this.IP = IP;
            this.Port = Port;
            Client = new TcpClient();
        }

        //------------------------------[Getters]------------------------------

        public String GetIP() { return IP; }
        public int GetPort() { return Port; }
        public bool IsBusy() { return Busy; }

        //------------------------------[Functions]------------------------------

        /// <summary>Initiate the connection</summary>
        /// <returns>True if it managed to connect, false otherwise</returns>
        public Boolean Connect() {

            //Attempt to connect.
            Client.ConnectAsync(IP,Port);

            //15 second time out
            for(int i = 0; i < 30; i++) {
                if(Client.Connected) { break; }
                Thread.Sleep(500);
            }

            //Verify if we've connected.
            if(!Client.Connected) {return false;}

            //Neat we connected!
            River = Client.GetStream();
            return true;
        }

        /// <summary>Close the Client's connection</summary>
        public void Close() {
            if(!Connected) { return; } //Make sure attempting to close an already closed connection doesn't cause an exception. That's kinda bobo.
            //Send CLOSE to the server, closing that side.
            try { Send("CLOSE",false); } catch(IOException) { } //try to close remotely. This may fail because of an IOException so if algo just shhh.

            //I mean that should close the TCPClient and the stream, no?
            River.Close();
            Client.Close();

        }

        /// <summary>Send data to the switchboard server, and retrieve a response</summary>
        public String SendReceive(String Data) {
            Send(Data,true); ;
            return Receive();
        }

        /// <summary>Sends data to the Server</summary>
        /// <param name="data">Data to send</param>
        /// <param name="KeepBusy">Whether or not to keep the client busy (in case you're going to receive data right after sending it)</param>
        public void Send(String data,bool KeepBusy) {
            if(!Connected) { throw new InvalidOperationException("This client is not connected right now!"); }
            Busy = true;
            Byte[] Bytes = Encoding.ASCII.GetBytes(data); //Convert the string to bytes.
            River.Write(Bytes,0,Bytes.Length); //send the bytes
            if(!KeepBusy) { Busy = false; }
        }

        /// <summary>Only receive data</summary>
        public String Receive() {
            if(!Connected) { throw new InvalidOperationException("This client is not connected right now!"); }
            Busy = true;

            //10 second time out.
            for(int X = 0; X < 100; X++) {
                if(Available) { break; }
                Thread.Sleep(100);
            }

            if(!Available) { throw new TimeoutException("Server did not respond in 10 seconds. Probably CLOSE the connection"); }

            List<Byte> Bytes = new List<Byte>();
            while(Available) { Bytes.Add((byte)(River.ReadByte())); } //Get all the bytes in a nice little array.
            Busy = false;
            return Encoding.ASCII.GetString(Bytes.ToArray()); //convert the array of bytes back into a neat little bit of text, and return it.
        }

        /// <summary>Login on the server</summary>
        /// <returns>The appropriate login result</returns>
        public LoginResult Login(String Username,String Password) {
            switch(SendReceive("LOGIN " + Username + " " + Password)) {
                case "0":
                    return LoginResult.SUCCESS;
                case "1":
                    return LoginResult.INVALID;
                case "2":
                    return LoginResult.ALREADY;
                case "3":
                    return LoginResult.OTHERLOCALE;
                default:
                    return LoginResult.INVALID;
            }

        }

        /// <summary>Logout on the server</summary>
        /// <returns>True if logout was successful, false otherwise.</returns>
        public bool Logout() { return SendReceive("Logout") == "1"; }

    }
}
