using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Messenger
{
    public partial class MainWindow : Window
    {
        private Socket socket;
        private List<Socket> clients = new List<Socket>();
        private List<string> usernames = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 2000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen(1000);
            ListenToClients();
        }

        private async Task ListenToClients()
        {
            while (true)
            {
                var client = await socket.AcceptAsync();
                clients.Add(client);

                byte[] buffer = new byte[1024];
                await client.ReceiveAsync(buffer, SocketFlags.None);
                string username = Encoding.UTF8.GetString(buffer).Trim();

                usernames.Add(username);
                UpdateUsersList();

                foreach (var item in clients)
                {
                    await SendMessage(item, $"JOIN {username}");
                }

                await RecieveMessage(client);
            }
        }

        private async Task RecieveMessage(Socket client)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes);

                string username = usernames[clients.IndexOf(client)];

                AllMessages.Items.Add($"[сообщение от {username}]: {message}");

                foreach (var item in clients)
                {
                    await SendMessage(item, $"[сообщение от {username}]: {message}");
                }
            }
        }

        private async Task SendMessage(Socket client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        private void UpdateUsersList()
        {
            Dispatcher.Invoke(() =>
            {
                ListUsers.Items.Clear();
                foreach (string user in usernames)
                {
                    ListUsers.Items.Add(user);
                }
            });
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            string message = txt.Text;
            foreach (var client in clients)
            {
                await SendMessage(client, message);
            }
            txt.Text = string.Empty;
        }
    }
}