using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using TsBufferExtractor.Interface;
using MediaPortal.Profile;


namespace TsBufferExtractorClient
{
  [PluginIcons("TsBufferExtractorClient.TsBufferExtractorClientEnabled.bmp",
  "TsBufferExtractorClient.TsBufferExtractorClientDisabled.bmp")]
  public class TsBufferExtractorClient : GUIInternalWindow, IPlugin, ISetupForm
  {
    TsBufferExtractorInterface remoteObject = null;
    HttpChannel httpChannel = new HttpChannel();

    public void Start()
    {
      try
      {
        ChannelServices.RegisterChannel(httpChannel, false);
      }
      catch (Exception ex)
      {
        //Log.Error("TsBufferExtractorClient exception: {0}", ex);
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

    public void OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_MANUAL_RECORDING_STARTED)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);

        if (dlgYesNo != null)
        {
          dlgYesNo.SetHeading("Confirm");
          dlgYesNo.SetLine(1, "Do you want to save the timeshift buffer?");
          dlgYesNo.TimeOut = 10;
          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
          if (!dlgYesNo.IsConfirmed)
          {
            Log.Debug("TsBufferExtractorClient: Manual record detected, but not confirmed.");
            return;
          }
        }
        
        try
        {
          remoteObject.ManualRecordingStarted(Dns.GetHostName());
          Log.Debug("TsBufferExtractorClient: Manual record detected. Msg sent to server.");
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
      try
      {
        ChannelServices.UnregisterChannel(httpChannel);
      }
      catch (Exception ex)
      {
        //Log.Error("TsBufferExtractor exception: {0}", ex);
      }
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

    public bool CanEnable()
    {
      return true;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public void ShowPlugin()
    {
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                    out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return false;
    }
  }
}
