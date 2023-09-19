using _1BChatApp.Core;
using _1BChatApp.MVVM.Model;
using _1BChatApp.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _1BChatApp.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactModel> Contacts { get; set; }
        public ObservableCollection<UserModel> Users { get; set; }
        public string Username { get; set; }

        /* Commands */
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }

        private Server _server;

        private ContactModel _selectedContact;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set { _selectedContact = value; OnPropertyChanged(); }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();
            Users = new ObservableCollection<UserModel>();

            _server = new Server();

            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += UserDisconnected;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o => { _server.SendMessageToServer(_message); Message = ""; }, o => !string.IsNullOrEmpty(Message));

           
        }

        private void UserDisconnected()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(new MessageModel { FirstMessage = false, Message= msg}));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(), 
                UID = _server.PacketReader.ReadMessage()
            };

            if (!Users.Any(x=> x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
    }
}
