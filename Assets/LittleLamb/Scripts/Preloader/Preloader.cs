using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class Preloader : MonoBehaviour
{

  private static string TAG = "Preloader";
  private Logger logger;

  void Awake()
  {
    string[] args = System.Environment.GetCommandLineArgs();
    logger = new Logger(new LoggerHandler());

    logger.Log("Awake");

    bool isServer = false;
    bool isClient = false;

    for (int i = 0; i < args.Length; i++)
    {
      switch (args[i])
      {
        case "--server":
          isServer = true;
          break;
        case "--client":
          isClient = true;
          break;
      }
    }

    if (isServer)
    {
      StartServer();
    }
    else if (isClient)
    {
      StartClient();
    }
  }

  public void StartHost()
  {
    logger.Log(TAG, "StartHost");
    SceneManager.LoadScene("SampleScene");
  }

  public void StartServer()
  {
    logger.Log(TAG, "StartServer");
    SceneManager.LoadScene("SampleScene");
  }

  public void StartClient()
  {
    logger.Log(TAG, "StartClient");
    SceneManager.LoadScene("SampleScene");
  }

  void Start()
  {
    logger.Log(TAG, "Start");
  }

}
