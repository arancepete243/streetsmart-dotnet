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

using System.Collections.Generic;

using StreetSmart.WinForms.Data;
using StreetSmart.WinForms.Interfaces.Data;

namespace StreetSmart.WinForms.Factories
{
  /// <summary>
  /// Factory for the viewer options which are used as options when a cyclorama viewer is opened.
  /// </summary>
  public static class ViewerOptionsFactory
  {
    /// <summary>
    /// Creates a viewer options object
    /// </summary>
    /// <param name="viewerType">An collection of viewerTypes</param>
    /// <param name="srs">The SRS of the viewer</param>
    /// <returns>The viewer options used for open viewers</returns>
    public static IViewerOptions Create(IList<ViewerType> viewerType, string srs)
      => Create(viewerType, srs, null, null);

    /// <summary>
    /// Creates a viewer options object
    /// </summary>
    /// <param name="viewerType">An collection of viewerTypes</param>
    /// <param name="srs">The SRS of the viewer</param>
    /// <param name="panoramaViewer">The panorama viewer options</param>
    /// <returns>The viewer options used for open the viewers</returns>
    public static IViewerOptions Create(IList<ViewerType> viewerType, string srs, IPanoramaViewerOptions panoramaViewer)
      => Create(viewerType, srs, panoramaViewer, null);

    /// <summary>
    /// Creates a viewer options object
    /// </summary>
    /// <param name="viewerType">An collection of viewerTypes</param>
    /// <param name="srs">The SRS of the viewer</param>
    /// <param name="panoramaViewer">The panorama viewer options</param>
    /// <param name="obliqueViewer">The oblique viewer options</param>
    /// <returns>The viewer options used for open the viewers</returns>
    public static IViewerOptions Create(IList<ViewerType> viewerType, string srs, IPanoramaViewerOptions panoramaViewer,
      IObliqueViewerOptions obliqueViewer)
      => new ViewerOptions(viewerType, srs, panoramaViewer, obliqueViewer);
  }
}
