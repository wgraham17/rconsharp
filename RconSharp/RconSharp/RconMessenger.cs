﻿using System;
using System.Threading.Tasks;

/*
The MIT License (MIT)

Copyright (c) 2014 Stefano Driussi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RconSharp
{
	/// <summary>
	/// Rcon protocol messages handler
	/// </summary>
	public class RconMessenger : IDisposable
	{
		private INetworkSocket _socket;

		/// <summary>
		/// Class constructor
		/// </summary>
		public RconMessenger()
		{
            _socket = new RconSocket();
		}

		/// <summary>
		/// Connect the socket to the remote endpoint
		/// </summary>
		/// <param name="host">remote host address</param>
		/// <param name="port">remote host port</param>
		/// <returns>True if the connection was successfully; False if the connection is already estabilished</returns>
		public async Task<bool> ConnectAsync(string host, int port)
		{
			if (!_socket.IsConnected)
				return await _socket.ConnectAsync(host, port);
			else
				return false;
		}

		/// <summary>
		/// Send the proper authentication packet and parse the response
		/// </summary>
		/// <param name="password">Current server password</param>
		/// <returns>True if the connection has been authenticated; False elsewhere</returns>
		/// <remarks>This method must be called prior to sending any other command</remarks>
		/// <exception cref="ArgumentException">Is thrown if <paramref name="password"/> parameter is null or empty</exception>
		public async Task<bool> AuthenticateAsync(string password)
		{
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException("password parameter must be a non null non empty string");

			var authPacket = new RconPacket(PacketType.Auth, password);
			var response = await _socket.SendDataAndReadResponseAsync(authPacket.GetBytes());
			var responsePacket = RconPacket.FromBytes(response);
			return responsePacket.Id != -1;
		}

		/// <summary>
		/// Send a command encapsulated into an Rcon message packet and get the response
		/// </summary>
		/// <param name="command">Command to be executed</param>
		/// <returns>The response to this command</returns>
		/// <exception cref="ArgumentException">Is thrown if <paramref name="command"/> parameter is null or empty</exception>
		/// <exception cref="InvalidOperationException">Is thrown if the connection is not properly opened and authenticated</exception>
		public async Task<string> ExecuteCommandAsync(string command)
		{
			if (string.IsNullOrEmpty(command))
				throw new ArgumentException("command parameter must be a non null non empty string");

			if (!_socket.IsConnected)
				throw new InvalidOperationException("You must authenticate the connection before sending any command to the server");

			var commandPacket = new RconPacket(PacketType.ExecCommand, command);
			var response = await _socket.SendDataAndReadResponseAsync(commandPacket.GetBytes());
			var responsePacket = RconPacket.FromBytes(response);
			return responsePacket.Body;
		}

		/// <summary>
		/// Close the remote connection
		/// </summary>
		public void CloseConnection()
		{
			_socket.CloseConnection();
		}

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Dispose();
            }
        }
    }
}
