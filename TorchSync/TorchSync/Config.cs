﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using NLog;
using Torch;
using Utils.Torch;
using VRage.Collections;

namespace TorchSync
{
    public sealed class Config : ViewModel, FileLoggingConfigurator.IConfig
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public const string DefaultPath = "Logs/TorchSync-${shortdate}.log";
        public static Config Instance { get; set; }

        readonly ConcurrentCachingHashSet<IpPort> _remoteIps;
        readonly ConcurrentCachingHashSet<string> _remoteChatAuthors;
        bool _countRemotePlayerCount;
        int _port = 8100;
        string _name = "Example";
        bool _suppressWpfOutput;
        bool _enableLoggingTrace;
        bool _enableLoggingDebug;
        string _logFilePath = DefaultPath;
        bool _specifyPlayerCount;
        int _playerCount;
        IpPort _redirectIpAddress = new();
        bool _enableRedirect;

        public Config()
        {
            _remoteIps = new ConcurrentCachingHashSet<IpPort>();
            CollectionChangedEventManager.AddHandler(RemoteIps, (_, _) =>
            {
                _remoteIps.Clear();
                foreach (var remotePort in RemoteIps)
                {
                    _remoteIps.Add(remotePort);
                }

                _remoteIps.ApplyChanges();

                OnPropertyChanged(nameof(RemoteIps));
            });

            _remoteChatAuthors = new ConcurrentCachingHashSet<string>();
            CollectionChangedEventManager.AddHandler(RemoteChatAuthors, (_, _) =>
            {
                _remoteChatAuthors.Clear();
                foreach (var chatAuthor in RemoteChatAuthors)
                {
                    _remoteChatAuthors.Add(chatAuthor.Name);
                }

                _remoteChatAuthors.ApplyChanges();

                OnPropertyChanged(nameof(RemoteChatAuthors));
            });
        }

        [XmlElement]
        public bool CountRemotePlayerCount
        {
            get => _countRemotePlayerCount;
            set => SetValue(ref _countRemotePlayerCount, value);
        }

        [XmlElement]
        public bool SpecifyPlayerCount
        {
            get => _specifyPlayerCount;
            set => SetValue(ref _specifyPlayerCount, value);
        }

        [XmlElement]
        public int PlayerCount
        {
            get => _playerCount;
            set => SetValue(ref _playerCount, value);
        }

        [XmlElement]
        public int Port
        {
            get => _port;
            set => SetValue(ref _port, value);
        }

        [XmlArray]
        public ViewModelCollection<IpPort> RemoteIps { get; } = new(); // parser will merge stuff

        [XmlArray]
        public ViewModelCollection<ChatAuthor> RemoteChatAuthors { get; } = new(); // parser will merge stuff

        // use this instead
        [XmlIgnore]
        public IEnumerable<IpPort> RemoteIpsSet => _remoteIps;

        [XmlElement]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        // use this instead
        [XmlIgnore]
        public IEnumerable<string> RemoteChatAuthorSet => _remoteChatAuthors;

        [XmlElement]
        public bool EnableRedirect
        {
            get => _enableRedirect;
            set => SetValue(ref _enableRedirect, value);
        }

        [XmlElement]
        public IpPort RedirectIpAddress
        {
            get => _redirectIpAddress;
            set => SetValue(ref _redirectIpAddress, value);
        }

        [XmlElement]
        public bool SuppressWpfOutput
        {
            get => _suppressWpfOutput;
            set => SetValue(ref _suppressWpfOutput, value);
        }

        [XmlElement]
        public bool EnableLoggingTrace
        {
            get => _enableLoggingTrace;
            set => SetValue(ref _enableLoggingTrace, value);
        }

        [XmlElement]
        public bool EnableLoggingDebug
        {
            get => _enableLoggingDebug;
            set => SetValue(ref _enableLoggingDebug, value);
        }

        [XmlElement]
        public string LogFilePath
        {
            get => _logFilePath;
            set => SetValue(ref _logFilePath, value);
        }
    }
}