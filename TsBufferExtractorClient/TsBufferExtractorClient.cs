﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using TsBufferExtractor.Interface;
using MediaPortal.Profile;


namespace TsBufferExtractorClient
{
  public class TsBufferExtractorClient : IPlugin
  {
    TsBufferExtractorInterface remoteObject = null;
    HttpChannel httpChannel = new HttpChannel();

    public void Start()
    {
      try
      {
        ChannelServices.RegisterChannel(httpChannel);
      }
      catch (Exception ex)
      {
        Log.Error("TsBufferExtractorClient exception: {0}", ex);
      }

      string hostName;
      using (Settings reader = new MPSettings())
      {
        hostName = reader.GetValueAsString("tvservice", "hostname", "localhost");
      }

      string url = string.Format("http://{0}:9998/TsBufferExtractorServer", hostName);
      
      try
      {
        remoteObject =
          (TsBufferExtractorInterface)Activator.GetObject(typeof(TsBufferExtractorInterface), url);
        GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);
      }
      catch 
      {
        Log.Error("TsBufferExtractorClient: unable to connect to TsBufferExtractor server: {0}", url);
      }
      Log.Debug("TsBufferExtractorClient: Started");
    }

    private void OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_MANUAL_RECORDING_STARTED)
      {
        try
        {
          remoteObject.ManualRecordingStarted(Dns.GetHostName());
          Log.Debug("TsBufferExtractorClient: Manual record detected.");
        }
        catch (Exception ex)
        {
          Log.Error("TsBufferExtractorClient: unable to connect to TsBufferExtractor server. {0}", ex);
        }
      }
    }

    public void Stop()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnMessage);
      ChannelServices.UnregisterChannel(httpChannel);
      Log.Debug("TsBufferExtractorClient: Stop");
    }

    public string Author()
    {
      return "regeszter";
    }

    public string PluginName()
    {
      return "TsBufferExtractorClient";
    }

    public string Description()
    {
      return "TsBufferExtractorClient";
    }

    public bool HasSetup()
    {
      return false;
    }

    public bool DefaultEnabled()
    {
      return false;
    }
  }
}
