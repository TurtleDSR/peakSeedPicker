using System;
using System.Collections.Generic;

namespace peakseedpicker;

public sealed class Version : ICloneable, IComparable<Version>, IEquatable<Version> {
  public int major;
  public int minor;
  public char build;

  public Version(int major, int minor, char build) {
    this.major = major;
    this.minor = minor;
    this.build = build;
  }

  public Version(string version) {
    string[] split = version.Trim('.').Split('.');

    major = Convert.ToInt32(split[0]);
    minor = Convert.ToInt32(split[1]);
    build = split[2][0];
  }

  object ICloneable.Clone() {
    return new Version(major, minor, build);
  }

  public int CompareTo(Version other) {
    int comp;
    if((comp = minor - other.minor) != 0) {
      return comp;
    } else if((comp = minor - other.minor) != 0) {
      return comp;
    } else if((comp = build - other.build) != 0) {
      return comp;
    } else {
      return 0;
    }
  }

  public bool Equals(Version other) {
    if(other.major == major && other.minor == minor && other.build == build) {
      return true;
    } else {
      return false;
    }
  }

  public override bool Equals(object o) {
    if(o == null) return false;
    var obj = o as Version; 
    return obj != null && this == obj;
  }

  public override int GetHashCode() {
    return EqualityComparer<Version>.Default.GetHashCode(this);
  }

  public override string ToString() {
    return $"v{major}.{minor}.{build}";
  }

  public static bool operator >(Version v1, Version v2) {
    return v1.CompareTo(v2) > 0;
  }

  public static bool operator <(Version v1, Version v2) {
    return v1.CompareTo(v2) < 0;
  }

  public static bool operator >=(Version v1, Version v2) {
    return v1.CompareTo(v2) >= 0;
  }

  public static bool operator <=(Version v1, Version v2) {
    return v1.CompareTo(v2) <= 0;
  }

  public static bool operator ==(Version v1, Version v2) {
    return v1.Equals(v2);
  }

  public static bool operator !=(Version v1, Version v2) {
    return !v1.Equals(v2);
  }
}