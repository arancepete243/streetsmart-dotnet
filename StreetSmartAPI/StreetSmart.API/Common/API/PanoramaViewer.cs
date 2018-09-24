﻿/*
 * Street Smart .NET integration
 * Copyright (c) 2016 - 2018, CycloMedia, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using CefSharp;

#if WINFORMS
using CefSharp.WinForms;
#else
using CefSharp.Wpf;
#endif

using StreetSmart.Common.Data;
using StreetSmart.Common.Events;
using StreetSmart.Common.Interfaces.API;
using StreetSmart.Common.Interfaces.Data;
using StreetSmart.Common.Interfaces.Events;

using StreetSmart.Common.API.Events;

using Orientation = StreetSmart.Common.Data.Orientation;

namespace StreetSmart.Common.API
{
  internal class PanoramaViewer : Viewer, IPanoramaViewer
  {
    #region Members

    private ApiEventList _panoramaViewerEventList;

    #endregion

    #region Events

    public event EventHandler<IEventArgs<IElevationInfo>> ElevationChange;
    public event EventHandler<IEventArgs<object>> ImageChange;
    public event EventHandler<IEventArgs<IRecordingClickInfo>> RecordingClick;
    public event EventHandler<IEventArgs<IDepthInfo>> SurfaceCursorChange;
    public event EventHandler<IEventArgs<IDictionary<string, object>>> TileLoadError;
    public event EventHandler<IEventArgs<ITimeTravelInfo>> TimeTravelChange;
    public event EventHandler<IEventArgs<IOrientation>> ViewChange;
    public event EventHandler<IEventArgs<object>> ViewLoadEnd;
    public event EventHandler<IEventArgs<object>> ViewLoadStart;

    #endregion

    #region Properties

    public string JsElevationChange => (ViewerList as PanoramaViewerList)?.JsElevationChange;

    public string JsImChange => (ViewerList as PanoramaViewerList)?.JsImChange;

    public string JsSurfaceCursorChange => (ViewerList as PanoramaViewerList)?.JsSurfaceCursorChange;

    public string JsRecClick => (ViewerList as PanoramaViewerList)?.JsRecClick;

    public string JsTileLoadError => (ViewerList as PanoramaViewerList)?.JsTileLoadError;

    public string JsViewChange => (ViewerList as PanoramaViewerList)?.JsViewChange;

    public string JsViewLoadEnd => (ViewerList as PanoramaViewerList)?.JsViewLoadEnd;

    public string JsViewLoadStart => (ViewerList as PanoramaViewerList)?.JsViewLoadStart;

    public string JsTimeTravelChange => (ViewerList as PanoramaViewerList)?.JsTimeTravelChange;

    public string DisconnectEventsScript => _panoramaViewerEventList.Destroy;

    public string ConnectEventsScript => $"{_panoramaViewerEventList}";

    #endregion

    #region Constructors

    public PanoramaViewer(ChromiumWebBrowser browser, PanoramaViewerList panoramaViewerList, string name)
      : base(browser, panoramaViewerList)
    {
      Name = name;
      ConnectEvents();
    }

    #endregion

    #region Interface Functions

    public async Task<bool> Get3DCursorVisible()
    {
      return (bool) await CallJsAsync(GetScript("get3DCursorVisible()"));
    }

    public async Task<bool> GetButtonEnabled(PanoramaViewerButtons buttonId)
    {
      return await base.GetButtonEnabled(buttonId);
    }

    public async Task<IOrientation> GetOrientation()
    {
      return new Orientation((Dictionary<string, object>) await CallJsAsync(GetScript("getOrientation()")));
    }

    public async Task<IRecording> GetRecording()
    {
      string funcName = $"{nameof(GetRecording).ToQuote()}";
      var script = $@"recording{Name}={Name}.getRecording();delete recording{Name}.thumbs;
                   {JsThis}.{JsResult}('{Name}',recording{Name},{funcName});";
      return new Recording((Dictionary<string, object>) await CallJsAsync(script));
    }

    public async Task<bool> GetRecordingsVisible()
    {
      return (bool) await CallJsAsync(GetScript("getRecordingsVisible()"));
    }

    public async Task<Color> GetViewerColor()
    {
      return GetColor((object[]) await CallJsAsync(GetScript("getViewerColor()")));
    }

    public void LookAtCoordinate(ICoordinate coordinate, string srs = null)
    {
      Browser.ExecuteScriptAsync($"{Name}.lookAtCoordinate({coordinate}{srs.SrsComponent()});");
    }

    public async Task<IRecording> OpenByAddress(string query, string srs = null)
    {
      return await SearchRecordingAsync("openByAddress", query.ToQuote(), srs);
    }

    public async Task<IRecording> OpenByCoordinate(ICoordinate coordinate, string srs = null)
    {
      return await SearchRecordingAsync("openByCoordinate", coordinate.ToString(), srs);
    }

    public async Task<IRecording> OpenByImageId(string imageId, string srs = null)
    {
      return await SearchRecordingAsync("openByImageId", imageId.ToQuote(), srs);
    }

    public void RotateDown(double deltaPitch)
    {
      Browser.ExecuteScriptAsync($"{Name}.rotateDown({deltaPitch});");
    }

    public void RotateLeft(double deltaYaw)
    {
      Browser.ExecuteScriptAsync($"{Name}.rotateLeft({deltaYaw});");
    }

    public void RotateRight(double deltaYaw)
    {
      Browser.ExecuteScriptAsync($"{Name}.rotateRight({deltaYaw});");
    }

    public void RotateUp(double deltaPitch)
    {
      Browser.ExecuteScriptAsync($"{Name}.rotateUp({deltaPitch});");
    }

    public void SetOrientation(IOrientation orientation)
    {
      Browser.ExecuteScriptAsync($"{Name}.setOrientation({orientation});");
    }

    public void SetSelectedFeatureByProperties(IJson properties, string layerId)
    {
      Browser.ExecuteScriptAsync($"{Name}.setSelectedFeatureByProperties({properties},{layerId.ToQuote()});");
    }

    public void Toggle3DCursor(bool visible)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggle3DCursor({visible.ToJsBool()});");
    }

    public void ToggleButtonEnabled(PanoramaViewerButtons buttonId, bool enabled)
    {
      base.ToggleButtonEnabled(buttonId, enabled);
    }

    public void ToggleRecordingsVisible(bool visible)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleRecordingsVisible({visible.ToJsBool()});");
    }

    #endregion

    #region Events from StreetSmartAPI

    public void OnElevationChange(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      ElevationChange?.Invoke(this, new EventArgs<ElevationInfo>(new ElevationInfo(detail)));
    }

    public void OnImageChange(Dictionary<string, object> args)
    {
      ImageChange?.Invoke(this, new EventArgs<object>(new object()));
    }

    public void OnRecordingClick(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      Dictionary<string, object> recording = (Dictionary<string, object>) detail["recording"];
      Dictionary<string, object> eventData = (Dictionary<string, object>) detail["eventData"];
      RecordingClick?.Invoke(this, new EventArgs<RecordingClickInfo>(new RecordingClickInfo(recording, eventData)));
    }

    public void OnTileLoadError(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      TileLoadError?.Invoke(this, new EventArgs<Dictionary<string, object>>
        ((Dictionary<string, object>) detail["request"]));
    }

    public void OnViewChange(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      ViewChange?.Invoke(this, new EventArgs<Orientation>(new Orientation(detail)));
    }

    public void OnSurfaceCursorChange(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      SurfaceCursorChange?.Invoke(this, new EventArgs<IDepthInfo>(new DepthInfo(detail)));
    }

    public void OnViewLoadEnd(Dictionary<string, object> args)
    {
      ViewLoadEnd?.Invoke(this, new EventArgs<object>(new object()));
    }

    public void OnViewLoadStart(Dictionary<string, object> args)
    {
      ViewLoadStart?.Invoke(this, new EventArgs<object>(new object()));
    }

    public void OnTimeTravelChange(Dictionary<string, object> args)
    {
      Dictionary<string, object> detail = (Dictionary<string, object>) args["detail"];
      TimeTravelChange?.Invoke(this, new EventArgs<ITimeTravelInfo>(new TimeTravelInfo(detail)));
    }

    #endregion

    #region Functions

    public void ConnectEvents()
    {
      _panoramaViewerEventList = new ApiEventList
      {
        new PanoramaRecordingClickViewerEvent(this, "RECORDING_CLICK", JsRecClick),
        new PanoramaViewerEvent(this, "ELEVATION_CHANGE", JsElevationChange),
        new PanoramaViewerEvent(this, "IMAGE_CHANGE", JsImChange),
        new PanoramaViewerEvent(this, "SURFACE_CURSOR_CHANGE", JsSurfaceCursorChange),
        new PanoramaViewerEvent(this, "VIEW_CHANGE", JsViewChange),
        new PanoramaViewerEvent(this, "VIEW_LOAD_START", JsViewLoadStart),
        new PanoramaViewerEvent(this, "VIEW_LOAD_END", JsViewLoadEnd),
        new PanoramaViewerEvent(this, "TILE_LOAD_ERROR", JsTileLoadError),
        new PanoramaViewerEvent(this, "TIME_TRAVEL_CHANGE", JsTimeTravelChange)
      };

      Browser.ExecuteScriptAsync($"{_panoramaViewerEventList}");
    }

    private Color GetColor(object[] color)
    {
      return Color.FromArgb((int) ((double) color[3] * 255), (int) color[0], (int) color[1], (int) color[2]);
    }

    public async Task<IRecording> SearchRecordingAsync(string func, string query, string srs,
      [CallerMemberName] string funcName = "")
    {
      string script = $@"{Name}.{func}({query}{srs.SrsComponent()}).catch
                      (function(e){{{JsThis}.{JsImNotFound}('{Name}',e.message,{funcName})}}).then
                      (function(r){{delete r.thumbs;{JsThis}.{JsResult}('{Name}',r,{funcName})}});";
      object result = await CallJsAsync(script);

      if (result is Exception exception)
      {
        throw exception;
      }

      return new Recording((Dictionary<string, object>) result);
    }

    #endregion
  }
}