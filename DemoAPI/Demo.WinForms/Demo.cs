﻿/*
 * Integration in ArcMap for Cycloramas
 * Copyright (c) 2016, CycloMedia, All rights reserved.
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

using StreetSmart.WinForms.Exceptions;
using StreetSmart.WinForms.Factories;
using StreetSmart.WinForms.Interfaces;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

using static Demo.WinForms.Properties.Resources;

namespace Demo.WinForms
{
  public partial class Demo : Form
  {
    #region Members

    private readonly IStreetSmartAPI _api;
    private readonly List<IPanoramaViewer> _viewers;
    private readonly CultureInfo _ci;

    #endregion

    #region Properties

    private IPanoramaViewer Viewer => (_viewers.Count == 0) ? null : _viewers[_viewers.Count - 1];

    private int DeltaYawPitch
    {
      get
      {
        int result;

        if (!int.TryParse(txtDeltaYawPitch.Text, out result))
        {
          result = 0;
        }

        return result;
      }
    }

    #endregion

    public Demo()
    {
      InitializeComponent();
      _ci = CultureInfo.InvariantCulture;
      _viewers = new List<IPanoramaViewer>();
      _api = StreetSmartAPIFactory.Create();
      _api.APIReady += OnAPIReady;
      plStreetSmart.Controls.Add(_api.GUI);
      grLogin.Enabled = false;
      grOpenByAddress.Enabled = false;
      grViewerToggles.Enabled = false;
      grRotationsZoomInOut.Enabled = false;
      grAPIInfo.Enabled = false;
      grOpenCloseViewer.Enabled = false;
      grCoordinate.Enabled = false;
      grOrientation.Enabled = false;
      grOpenByImageId.Enabled = false;
      grRecordingViewerColorPermissions.Enabled = false;
    }

    #region events api

    // ReSharper disable once InconsistentNaming
    private void OnAPIReady(object sender, EventArgs args)
    {
      if (grLogin.InvokeRequired)
      {
        grLogin.Invoke(new MethodInvoker(() => grLogin.Enabled = true));
      }
      else
      {
        grLogin.Enabled = true;
      }
    }

    private void OnImageChange(object sender, IEventArgs<IDictionary<string, object>>  args)
    {
      string text = "Image change";
      AddViewerEventsText(text);
    }

    private void OnRecordingClick(object sender, IEventArgs<IRecordingClickInfo> args)
    {
      string text = $"Recording click: {args.Value.Recording.Id}";
      AddViewerEventsText(text);
    }

    private void OnTileLoadError(object sender, IEventArgs<IDictionary<string, object>> args)
    {
      string text = "Tile load error";
      AddViewerEventsText(text);
    }

    private void OnViewChange(object sender, IEventArgs<IOrientation> args)
    {
      IOrientation orientation = args.Value;
      string text = $"View change args, pitch: {orientation.Pitch}, yaw: {orientation.Yaw}, hFov: {orientation.HFov}";
      AddViewerEventsText(text);
    }

    private void OnViewLoadEnd(object sender, IEventArgs<IDictionary<string, object>> args)
    {
      string text = "Image load end";
      AddViewerEventsText(text);
    }

    private void OnViewLoadStart(object sender, IEventArgs<IDictionary<string, object>> args)
    {
      string text = "Image load start";
      AddViewerEventsText(text);
    }

    #endregion

    #region events from user interface

    private async void btnLogin_Click(object sender, EventArgs e)
    {
      IAddressSettings addressSettings = AddressSettingsFactory.Create("nl", "CMDatabase");
      IOptions options = OptionsFactory.Create(txtUsername.Text, txtPassword.Text, txtAPIKey.Text, srs, locale, addressSettings);

      try
      {
        await _api.InitAsync(options);

        if (grAPIInfo.InvokeRequired)
        {
          grAPIInfo.Invoke(new MethodInvoker(() => grAPIInfo.Enabled = true));
        }
        else
        {
          grAPIInfo.Enabled = true;
        }

        if (grOpenCloseViewer.InvokeRequired)
        {
          grOpenCloseViewer.Invoke(new MethodInvoker(() => grOpenCloseViewer.Enabled = true));
        }
        else
        {
          grOpenCloseViewer.Enabled = true;
        }

        MessageBox.Show("Login successfully");
      }
      catch (StreetSmartLoginFailedException ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void btRotateLeft_Click(object sender, EventArgs e)
    {
      Viewer?.RotateLeft(DeltaYawPitch);
    }

    private void btnRotateRight_Click(object sender, EventArgs e)
    {
      Viewer?.RotateRight(DeltaYawPitch);
    }

    private void btnDestroyViewer_Click(object sender, EventArgs e)
    {
      if (Viewer != null)
      {
        Viewer.ImageChange -= OnImageChange;
        Viewer.RecordingClick -= OnRecordingClick;
        Viewer.TileLoadError -= OnTileLoadError;
        Viewer.ViewChange -= OnViewChange;
        Viewer.ViewLoadEnd -= OnViewLoadEnd;
        Viewer.ViewLoadStart -= OnViewLoadStart;

        _api?.DestroyPanoramaViewer(Viewer);
        _viewers.RemoveAt(_viewers.Count - 1);

        if (_viewers.Count == 0)
        {
          ToggleViewerEnables(false);
          EnableImageEnables(false);
        }
      }
    }

    private async void btnApiReadyState_Click(object sender, EventArgs e)
    {
      bool apiReadyState = await _api.GetAPIReadyStateAsync();
      txtAPIResult.Text = apiReadyState.ToString();
    }

    private async void btnApplicationVersion_Click(object sender, EventArgs e)
    {
      string version = await _api.GetApplicationVersionAsync();
      txtAPIResult.Text = version;
    }

    private async void btnApplicationName_Click(object sender, EventArgs e)
    {
      string name = await _api.GetApplicationNameAsync();
      txtAPIResult.Text = name;
    }

    private async void btnPermissions_Click(object sender, EventArgs e)
    {
      string[] permissions = await _api.GetPermissionsAsync();
      string permissionsString = permissions.Aggregate(string.Empty,
        (current, permission) => $"{current}{permission}{Environment.NewLine}");
      txtRecordingViewerColorPermissions.Text = permissionsString;
    }

    private async void btnOpenByAddress_Click(object sender, EventArgs e)
    {
      try
      {
        IRecording recording;

        if (string.IsNullOrEmpty(txtAddressSrs.Text))
        {
          recording = await Viewer.OpenByAddressAsync(txtAdress.Text);
        }
        else
        {
          recording = await Viewer.OpenByAddressAsync(txtAdress.Text, txtAddressSrs.Text);
        }
      }
      catch (StreetSmartImageNotFoundException ex)
      {
        MessageBox.Show(ex.Message);
      }

      EnableImageEnables(true);
    }

    private async void btnGetViewerColor_Click(object sender, EventArgs e)
    {
      Color color = await Viewer.GetViewerColorAsync();
      string text = $"Alpha: {color.A}{Environment.NewLine}Red: {color.R}{Environment.NewLine}Green: {color.G}{Environment.NewLine}Blue: {color.B}";
      txtRecordingViewerColorPermissions.Text = text;
    }

    private void btnRotateUp_Click(object sender, EventArgs e)
    {
      Viewer.RotateUp(DeltaYawPitch);
    }

    private void btnRotateDown_Click(object sender, EventArgs e)
    {
      Viewer.RotateDown(DeltaYawPitch);
    }

    private async void btnOrientation_Click(object sender, EventArgs e)
    {
      IOrientation orientation = await Viewer.GetOrientationAsync();
      txtYaw.Text = orientation.Yaw.ToString();
      txtPitch.Text = orientation.Pitch.ToString();
      txthFov.Text = orientation.HFov.ToString();
    }

    private void btnSetOrientation_Click(object sender, EventArgs e)
    {
      double? hFov = string.IsNullOrEmpty(txthFov.Text) ? null : (double?) ParseDouble(txthFov.Text);
      double? yaw = string.IsNullOrEmpty(txtYaw.Text) ? null : (double?) ParseDouble(txtYaw.Text);
      double? pitch = string.IsNullOrEmpty(txtPitch.Text) ? null : (double?) ParseDouble(txtPitch.Text);
      IOrientation orientation = OrientationFactory.Create(yaw, pitch, hFov);
      Viewer.SetOrientation(orientation);
    }

    private async void btnGetRecording_Click(object sender, EventArgs e)
    {
      IRecording recording = await Viewer.GetRecordingAsync();
      string text = $"Id: {recording.Id}{Environment.NewLine}recordedAt: {recording.RecordedAt}";
      txtRecordingViewerColorPermissions.Text = text;
    }

    private void btnLookAtCoordinate_Click(object sender, EventArgs e)
    {
      ICoordinate coordinate = string.IsNullOrEmpty(txtZ.Text)
        ? CoordinateFactory.Create(ParseDouble(txtX.Text), ParseDouble(txtY.Text))
        : CoordinateFactory.Create(ParseDouble(txtX.Text), ParseDouble(txtY.Text), ParseDouble(txtZ.Text));

      if (string.IsNullOrEmpty(txtCoordinateSrs.Text))
      {
        Viewer.LookAtCoordinate(coordinate);
      }
      else
      {
        Viewer.LookAtCoordinate(coordinate, txtCoordinateSrs.Text);
      }
    }

    private async void btnToggleRecordingsVisible_Click(object sender, EventArgs e)
    {
      bool visible = await Viewer.GetRecordingsVisibleAsync();
      Viewer.ToggleRecordingsVisible(!visible);
    }

    private async void btnToggleNavbarVisible_Click(object sender, EventArgs e)
    {
      bool visible = await Viewer.GetNavbarVisibleAsync();
      Viewer.ToggleNavbarVisible(!visible);
    }

    private async void btnToggleNavbarExpanded_Click(object sender, EventArgs e)
    {
      bool expanded = await Viewer.GetNavbarExpandedAsync();
      Viewer.ToggleNavbarExpanded(!expanded);
    }

    private async void btnToggleTimeTravelVisible_Click(object sender, EventArgs e)
    {
      bool visible = await Viewer.GetTimeTravelVisibleAsync();
      Viewer.ToggleTimeTravelVisible(!visible);
    }

    private async void btnToggleTimeTravelExpanded_Click(object sender, EventArgs e)
    {
      bool expanded = await Viewer.GetTimeTravelExpandedAsync();
      Viewer.ToggleTimeTravelExpanded(!expanded);
    }

    private async void btnOpenByImageId_Click(object sender, EventArgs e)
    {
      try
      {
        IRecording recording;

        if (string.IsNullOrEmpty(txtOpenByImageSrs.Text))
        {
          recording = await Viewer.OpenByImageIdAsync(txtImageId.Text);
        }
        else
        {
          recording = await Viewer.OpenByImageIdAsync(txtImageId.Text, txtOpenByImageSrs.Text);
        }
      }
      catch (StreetSmartImageNotFoundException ex)
      {
        MessageBox.Show(ex.Message);
      }

      EnableImageEnables(true);
    }

    private async void btnOpenByCoordinate_Click(object sender, EventArgs e)
    {
      try
      {
        ICoordinate coordinate = string.IsNullOrEmpty(txtZ.Text)
          ? CoordinateFactory.Create(ParseDouble(txtX.Text), ParseDouble(txtY.Text))
          : CoordinateFactory.Create(ParseDouble(txtX.Text), ParseDouble(txtY.Text), ParseDouble(txtZ.Text));

        IRecording recording;

        if (string.IsNullOrEmpty(txtCoordinateSrs.Text))
        {
          recording = await Viewer.OpenByCoordinateAsync(coordinate);
        }
        else
        {
          recording = await Viewer.OpenByCoordinateAsync(coordinate, txtCoordinateSrs.Text);
        }
      }
      catch (StreetSmartImageNotFoundException ex)
      {
        MessageBox.Show(ex.Message);
      }

      EnableImageEnables(true);
    }

    private void btnZoomIn_Click(object sender, EventArgs e)
    {
      Viewer.ZoomIn();
    }

    private void btnZoomOut_Click(object sender, EventArgs e)
    {
      Viewer.ZoomOut();
    }

    private void btnOpenViewer_Click(object sender, EventArgs e)
    {
      int width, height, top, left;
      bool result = int.TryParse(txtWidth.Text, out width);
      result = int.TryParse(txtHeight.Text, out height) && result;
      result = int.TryParse(txtTop.Text, out top) && result;
      result = int.TryParse(txtLeft.Text, out left) && result;

      if (result || rbDefault.Checked)
      {
        IPanoramaViewerOptions options = PanoramaViewerOptionsFactory.Create(true, true, true);
        IDomElement element = rbDefault.Checked
          ? DomElementFactory.Create()
          : DomElementFactory.Create(width, height, top, left);

        _viewers.Add(_api.AddPanoramaViewer(element, options));
        ToggleViewerEnables(true);

        Viewer.ImageChange += OnImageChange;
        Viewer.RecordingClick += OnRecordingClick;
        Viewer.TileLoadError += OnTileLoadError;
        Viewer.ViewChange += OnViewChange;
        Viewer.ViewLoadEnd += OnViewLoadEnd;
        Viewer.ViewLoadStart += OnViewLoadStart;
      }
      else
      {
        MessageBox.Show("Can not parse parameters");
      }
    }

    private async void btnGetAddress_Click(object sender, EventArgs e)
    {
      IAddressSettings addressSettings = await _api.GetAddressSettingsAsync();
      string text = $"Locale: {addressSettings.Locale}{Environment.NewLine}Database: {addressSettings.Database}";
      txtRecordingViewerColorPermissions.Text = text;
    }

    private void btnShowDefTools_Click(object sender, EventArgs e)
    {
      _api?.ShowDefTools();
    }

    private void btnCloseDefTools_Click(object sender, EventArgs e)
    {
      _api?.CloseDefTools();
    }

    #endregion

    #region Private Functions

    private double ParseDouble(string text)
    {
      double result;
      text = text.Replace(",", ".");

      if (!double.TryParse(text, NumberStyles.Float, _ci, out result))
      {
        result = 0.0;
      }

      return result;
    }

    private void AddViewerEventsText(string text)
    {
      if (lbViewerEvents.InvokeRequired)
      {
        lbViewerEvents.Invoke(new MethodInvoker(() => lbViewerEvents.Items.Add(text)));
      }
      else
      {
        lbViewerEvents.Items.Add(text);
      }
    }

    private void ToggleViewerEnables(bool value)
    {
      if (grOpenByAddress.InvokeRequired)
      {
        grOpenByAddress.Invoke(new MethodInvoker(() => grOpenByAddress.Enabled = value));
      }
      else
      {
        grOpenByAddress.Enabled = value;
      }

      if (grCoordinate.InvokeRequired)
      {
        grCoordinate.Invoke(new MethodInvoker(() => grCoordinate.Enabled = value));
      }
      else
      {
        grCoordinate.Enabled = value;
      }

      if (grOpenByImageId.InvokeRequired)
      {
        grOpenByImageId.Invoke(new MethodInvoker(() => grOpenByImageId.Enabled = value));
      }
      else
      {
        grOpenByImageId.Enabled = value;
      }
    }

    private void EnableImageEnables(bool value)
    {
      if (grViewerToggles.InvokeRequired)
      {
        grViewerToggles.Invoke(new MethodInvoker(() => grViewerToggles.Enabled = value));
      }
      else
      {
        grViewerToggles.Enabled = value;
      }

      if (grRotationsZoomInOut.InvokeRequired)
      {
        grRotationsZoomInOut.Invoke(new MethodInvoker(() => grRotationsZoomInOut.Enabled = value));
      }
      else
      {
        grRotationsZoomInOut.Enabled = value;
      }

      if (grOrientation.InvokeRequired)
      {
        grOrientation.Invoke(new MethodInvoker(() => grOrientation.Enabled = value));
      }
      else
      {
        grOrientation.Enabled = value;
      }

      if (grRecordingViewerColorPermissions.InvokeRequired)
      {
        grRecordingViewerColorPermissions.Invoke(
          new MethodInvoker(() => grRecordingViewerColorPermissions.Enabled = value));
      }
      else
      {
        grRecordingViewerColorPermissions.Enabled = value;
      }
    }

    #endregion
  }
}
