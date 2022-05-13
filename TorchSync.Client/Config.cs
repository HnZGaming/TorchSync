﻿using System.Net;
using TorchSync.Client.Managers;
using Torch;
using Torch.Views;

namespace TorchSync.Client;

public class Config : ViewModel, IClientNetworkConfig
{
    [Display(Name = "Port")]
    public int Port { get; set; } = 4853;
    
    // TODO: make editor for IpAddress
    [Display(Name = "Target Ip")]
    public IPAddress TargetIp { get; } = IPAddress.Loopback;
}