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

using System.Xml.Serialization;

using StreetSmart.Common.Interfaces.SLD;

namespace StreetSmart.Common.Data.SLD
{
  #pragma warning disable 1591
  public class SvgParameter<T> : NotifyPropertyChanged, ISvgParameter<T>
  {
    private T _name;

    [XmlAttribute("name", Namespace = "http://www.opengis.net/se")]
    public T Name
    {
      get => _name;
      set
      {
        _name = value;
        RaisePropertyChanged();
      }
    }

    private string _value;

    [XmlText]
    public string Value
    {
      get => _value;
      set
      {
        _value = value;
        RaisePropertyChanged();
      }
    }
  }
  #pragma warning restore 1591
}
