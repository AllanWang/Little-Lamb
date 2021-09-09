using System;
using MLAPI.Transports;
using MLAPI.Transports.PhotonRealtime;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Pick between ip and relay transports.
/// </summary>
public class TransportPicker : MonoBehaviour
{
  [SerializeField]
  NetworkTransport ipHostTransport;

  [SerializeField]
  NetworkTransport relayTransport;

  /// <summary>
  /// The transport used when hosting the game on an IP address.
  /// </summary>
  public NetworkTransport IpHostTransport => ipHostTransport;

  /// <summary>
  /// The transport used when hosting the game over a relay server.
  /// </summary>
  public NetworkTransport RelayTransport => relayTransport;

  void OnValidate()
  {
    Assert.IsTrue(ipHostTransport == null || (ipHostTransport as UNetTransport),
        "IpHost transport must be either Unet or LiteNetLib transport.");

    Assert.IsTrue(relayTransport == null || (relayTransport as PhotonRealtimeTransport),
        "Relay transport must be PhotonRealtimeTransport");
  }
}
