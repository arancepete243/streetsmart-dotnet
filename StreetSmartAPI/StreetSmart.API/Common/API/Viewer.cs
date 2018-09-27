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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using CefSharp;

#if WINFORMS
using CefSharp.WinForms;
#else
using CefSharp.Wpf;
#endif

using StreetSmart.Common.Interfaces.API;
using StreetSmart.Common.Exceptions;

namespace StreetSmart.Common.API
{
  internal class Viewer : IViewer
  {
    #region Tasks

    private readonly Dictionary<string, TaskCompletionSource<object>> _resultTask;

    #endregion

    #region Properties

    protected ChromiumWebBrowser Browser { get; }

    protected ViewerList ViewerList { get; }

    public string Name { get; protected set; }

    public bool Destroyed { private get; set; }

    public string JsThis => ViewerList.JsThis;

    public string JsResult => ViewerList.JsResult;

    public string JsImNotFound => (ViewerList as PanoramaViewerList)?.JsImNotFound;

    #endregion

    #region Constructors

    public Viewer(ChromiumWebBrowser browser, ViewerList viewerList)
    {
      Browser = browser;
      ViewerList = viewerList;
      Destroyed = false;
      _resultTask = new Dictionary<string, TaskCompletionSource<object>>();
    }

    public Viewer(ChromiumWebBrowser browser, ViewerList viewerList, string name)
    {
      Browser = browser;
      ViewerList = viewerList;
      Name = name;
      Destroyed = false;
      _resultTask = new Dictionary<string, TaskCompletionSource<object>>();
    }

    #endregion

    #region Interface Functions

    public async Task<string> GetId()
    {
      return (string) await CallJsAsync(GetScript("getId()"));
    }

    public async Task<bool> GetNavbarExpanded()
    {
      return (bool) await CallJsAsync(GetScript("getNavbarExpanded()"));
    }

    public async Task<bool> GetNavbarVisible()
    {
      return (bool) await CallJsAsync(GetScript("getNavbarVisible()"));
    }

    public async Task<bool> GetTimeTravelExpanded()
    {
      return (bool) await CallJsAsync(GetScript("getTimeTravelExpanded()"));
    }

    public async Task<bool> GetTimeTravelVisible()
    {
      return (bool) await CallJsAsync(GetScript("getTimeTravelVisible()"));
    }

    public void ToggleNavbarExpanded(bool expanded)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleNavbarExpanded({expanded.ToJsBool()});");
    }

    public void ToggleNavbarVisible(bool visible)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleNavbarVisible({visible.ToJsBool()});");
    }

    public void ToggleTimeTravelExpanded(bool expanded)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleTimeTravelExpanded({expanded.ToJsBool()});");
    }

    public void ToggleTimeTravelVisible(bool visible)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleTimeTravelVisible({visible.ToJsBool()});");
    }

    public void ZoomIn()
    {
      Browser.ExecuteScriptAsync($"{Name}.zoomIn();");
    }

    public void ZoomOut()
    {
      Browser.ExecuteScriptAsync($"{Name}.zoomOut();");
    }

    public void SetBrightness(double value)
    {
      Browser.ExecuteScriptAsync($"{Name}.setBrightness({value});");
    }

    public void SetContrast(double value)
    {
      Browser.ExecuteScriptAsync($"{Name}.setContrast({value});");
    }

    #endregion

    #region Callbacks viewer

    public void OnResult(object result, string funcName)
    {
      CheckResultTask(funcName);
      _resultTask[funcName].TrySetResult(result);
    }

    public void OnImageNotFoundException(string message, string funcName)
    {
      CheckResultTask(funcName);
      _resultTask[funcName].TrySetResult(new StreetSmartImageNotFoundException(message));
    }

    #endregion

    #region Functions

    private bool CheckResultTask(string funcName)
    {
      bool result = true;

      if (!_resultTask.ContainsKey(funcName))
      {
        _resultTask.Add(funcName, new TaskCompletionSource<object>());
        result = false;
      }

      return result;
    }

    protected async Task<bool> GetButtonEnabled(Enum buttonId)
    {
      return (bool) await CallJsAsync(GetScript($"getButtonEnabled({buttonId.Description()})"));
    }

    protected void ToggleButtonEnabled(Enum buttonId, bool enabled)
    {
      Browser.ExecuteScriptAsync($"{Name}.toggleButtonEnabled({buttonId.Description()},{enabled.ToJsBool()})");
    }

    protected async Task<object> CallJsAsync(string script, [CallerMemberName] string memberName = "")
    {
      if (!Destroyed)
      {
        if (CheckResultTask(memberName))
        {
          _resultTask[memberName] = new TaskCompletionSource<object>();
        }

        Browser.ExecuteScriptAsync(script);
        await _resultTask[memberName].Task;
        return _resultTask[memberName].Task.Result;
      }

      throw new StreetSmartViewerDoesNotExistException();
    }

    protected string GetScript(string funcName, [CallerMemberName] string memberName = "")
    {
      return $"{JsThis}.{JsResult}('{Name}',{Name}.{funcName},{memberName.ToQuote()});";
    }

    #endregion
  }
}
