﻿/*
 * Street Smart .NET integration
 * Copyright (c) 2016 - 2019, CycloMedia, All rights reserved.
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

namespace StreetSmart.Common.Data
{
  class DataConvert : NotifyPropertyChanged
  {
    public double ToDouble(object value)
    {
      return double.TryParse(value?.ToString(), out var outValue) ? outValue : 0.0;
    }

    public double ToDouble(Dictionary<string, object> details, string value)
    {
      return ToDouble(GetValue(details, value));
    }

    public double ToDouble(object[] array, int nr)
    {
      return array.Length > nr ? ToDouble(array[nr]) : 0.0;
    }

    public double? ToNullDouble(object value)
    {
      return double.TryParse(value?.ToString(), out var outValue) ? double.IsNaN(outValue) ? (double?) null : outValue : null;
    }

    public double? ToNullDouble(Dictionary<string, object> details, string value)
    {
      return ToNullDouble(GetValue(details, value));
    }

    public int ToInt(object value)
    {
      return int.TryParse(value?.ToString(), out var outValue) ? outValue : 0;
    }

    public int? ToNullInt(object value)
    {
      return int.TryParse(value?.ToString(), out var outValue) ? (int?) outValue : null;
    }

    public int ToInt(object[] array, int nr)
    {
      return array.Length > nr ? ToInt(array[nr]) : 0;
    }

    public int ToInt(Dictionary<string, object> details, string value)
    {
      return ToInt(GetValue(details, value));
    }

    public int? ToNullInt(Dictionary<string, object> details, string value)
    {
      return ToNullInt(GetValue(details, value));
    }

    public string ToString(object value)
    {
      return value?.ToString() ?? string.Empty;
    }

    public string ToString(Dictionary<string, object> details, string value)
    {
      return ToString(GetValue(details, value));
    }

    public DateTime? ToNullDateTime(object value)
    {
      return value == null ? null : (DateTime?) DateTime.Parse(value.ToString());
    }

    public DateTime? ToNullDateTime(Dictionary<string, object> details, string value)
    {
      return ToNullDateTime(GetValue(details, value));
    }

    public DateTime ToDateTime(object value)
    {
      return DateTime.Parse(value?.ToString());
    }

    public DateTime ToDateTime(Dictionary<string, object> details, string value)
    {
      return ToDateTime(GetValue(details, value));
    }

    public object ToEnum(Type type, object value)
    {
      return Enum.Parse(type, ToString(value));
    }

    public object ToEnum(Type type, Dictionary<string, object> details, string value)
    {
      return ToEnum(type, GetValue(details, value));
    }

    public bool ToBool(object value)
    {
      return (bool) (value ?? false);
    }

    public bool ToBool(Dictionary<string, object> details, string value)
    {
      return ToBool(GetValue(details, value));
    }

    public Dictionary<string, object> ToDictionary(object value)
    {
      return value as Dictionary<string, object> ?? new Dictionary<string, object>();
    }

    public IList<object> ToList(object value)
    {
      return value as IList<object> ?? new List<object>();
    }

    public object[] ToArray(object value)
    {
      return value as object[] ?? new object[0];
    }

    public object[] GetArrayValue(Dictionary<string, object> details, string value)
    {
      return ToArray(GetValue(details, value));
    }

    public object GetValue(Dictionary<string, object> details, string value)
    {
      return details?.ContainsKey(value) ?? false ? details[value] : null;
    }

    public Dictionary<string, object> GetDictValue(Dictionary<string, object> details, string value)
    {
      return ToDictionary(GetValue(details, value));
    }

    public IList<object> GetListValue(Dictionary<string, object> details, string value)
    {
      return ToList(GetValue(details, value));
    }
  }
}
