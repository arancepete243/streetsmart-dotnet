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

using System;
using System.Collections.Generic;
using StreetSmart.WinForms.Interfaces;

namespace StreetSmart.WinForms.Data
{
  internal class ViewerOptions : NotifyPropertyChanged, IViewerOptions
  {
    private IViewerTypes _viewerTypes;
    private string _srs;

    public ViewerOptions(IList<ViewerType> viewerTypes, string srs)
    {
      ViewerTypes = new ViewerTypes(viewerTypes);
      Srs = srs;
    }

    public IViewerTypes ViewerTypes
    {
      get { return _viewerTypes; }
      set
      {
        _viewerTypes = value;
        RaisePropertyChanged();
      }
    }

    public string Srs
    {
      get { return _srs; }
      set
      {
        _srs = value;
        RaisePropertyChanged();
      }
    }

    public override string ToString()
    {
      return $",{{viewerType:{ViewerTypes},srs:{Srs.ToQuote()}}}";
    }
  }
}