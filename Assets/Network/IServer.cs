using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServer {

    void SetPort(int port);
    int Port { get; }

    List<string> Paths { get; }

    void StartServer();
    void StopServer();
}
