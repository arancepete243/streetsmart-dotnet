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

using StreetSmart.WinForms.Interfaces.GeoJson;

namespace StreetSmart.WinForms.Data.GeoJson
{
  internal class DetailsForwardIntersection : Details, IDetailsForwardIntersection
  {
    public DetailsForwardIntersection(Dictionary<string, object> detailsForwardIntersection)
    {
      double? positionX = detailsForwardIntersection?["PositionX"] as double?;
      double? positionY = detailsForwardIntersection?["PositionY"] as double?;
      double? positionZ = detailsForwardIntersection?["PositionZ"] as double?;
      Dictionary<string, object> resultDirections = detailsForwardIntersection?["ResultDirections"] as Dictionary<string, object>;
      IList<object> resultDirection = resultDirections?["ResultDirection"] as IList<object> ?? new List<object>();

      Position = new Position(positionX, positionY, positionZ);
      ResultDirections = new List<IResultDirection>();

      foreach (var intResultDirection in resultDirection)
      {
        ResultDirections.Add(new ResultDirection(intResultDirection as Dictionary<string, object>));
      }
    }

    public IPosition Position { get; }

    public IList<IResultDirection> ResultDirections { get; }
  }
}