using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Preloader : MonoBehaviour
{

  private static string TAG = "Preloader";
  private Logger logger;

  private void Awake()
  {
    string[] args = System.Environment.GetCommandLineArgs();

    bool isServer = false;

    for (int i = 0; i < args.Length; i++)
    {
      switch (args[i])
      {
        case "--server":
          isServer = true;
          break;
      }
    }

    if (isServer) { StartServer(); } else { StartClient(); }
  }

  private void StartServer()
  {

  }

  private void StartClient()
  {

  }

  // Start is called before the first frame update
  void Start()
  {
    logger = new Logger(new LoggerHandler());
    logger.Log(TAG, "Start");
  }

  // Update is called once per frame
  void Update()
  {

  }
}
