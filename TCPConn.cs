﻿namespace BotwTrainer
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Windows;

    public class TcpConn
    {
        private TcpClient client;

        private NetworkStream stream;

        public string Host { get; private set; }

        public int Port { get; private set; }

        public TcpConn(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.client = null;
            this.stream = null;
        }

        public bool Connect()
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            this.client = new TcpClient { NoDelay = true };
            var asyncResult = this.client.BeginConnect(this.Host, this.Port, null, null);
            var waitHandle = asyncResult.AsyncWaitHandle;
            try
            {
                if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    this.client.Close();
                    return false;
                }

                this.client.EndConnect(asyncResult);
            }
            finally
            {
                waitHandle.Close();
            }

            this.stream = this.client.GetStream();
            this.stream.ReadTimeout = 10000;
            this.stream.WriteTimeout = 10000;

            return true;
        }

        public void Close()
        {
            try
            {
                if (this.client == null)
                {
                    throw new IOException("Not connected.", new NullReferenceException());
                }

                this.client.Close();
                this.client.Dispose();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                this.client = null;
            }
        }

        public void Read(byte[] buffer, uint nobytes, ref uint bytesRead)
        {
            try
            {
                int offset = 0;
                if (this.stream == null)
                {
                    throw new IOException("Not connected.", new NullReferenceException());
                }

                bytesRead = 0;
                while (nobytes > 0)
                {
                    int read = this.stream.Read(buffer, offset, (int)nobytes);
                    if (read >= 0)
                    {
                        bytesRead += (uint)read;
                        offset += read;
                        nobytes -= (uint)read;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                throw new IOException("Connection closed.", e);
            }
        }

        public void Write(byte[] buffer, int nobytes, ref uint bytesWritten)
        {
            try
            {
                if (this.stream == null)
                {
                    throw new IOException("Not connected.", new NullReferenceException());
                }
                this.stream.Write(buffer, 0, nobytes);
                if (nobytes >= 0)
                {
                    bytesWritten = (uint)nobytes;
                }
                else
                {
                    bytesWritten = 0;
                }

                this.stream.Flush();
            }
            catch (ObjectDisposedException e)
            {
                throw new IOException("Connection closed.", e);
            }
        }
    }
}
