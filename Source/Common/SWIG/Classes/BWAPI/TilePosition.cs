//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace SWIG.BWAPI {

public partial class TilePosition : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TilePosition(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TilePosition obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~TilePosition() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          bwapiPINVOKE.delete_TilePosition(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  
public override int GetHashCode()
{
   return this.swigCPtr.Handle.GetHashCode();
}

public override bool Equals(object obj)
{
    bool equal = false;
    if (obj is TilePosition)
      equal = (((TilePosition)obj).swigCPtr.Handle == this.swigCPtr.Handle);
    return equal;
}
  
public bool Equals(TilePosition obj) 
{
    if (obj == null) return false;
    return (obj.swigCPtr.Handle == this.swigCPtr.Handle);
}

public static bool operator ==(TilePosition obj1, TilePosition obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return true;
    if (object.ReferenceEquals(obj1, null)) return false;
    if (object.ReferenceEquals(obj2, null)) return false;
   
    return obj1.Equals(obj2);
}

public static bool operator !=(TilePosition obj1, TilePosition obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return false;
    if (object.ReferenceEquals(obj1, null)) return true;
    if (object.ReferenceEquals(obj2, null)) return true;

    return !obj1.Equals(obj2);
}




  public TilePosition() : this(bwapiPINVOKE.new_TilePosition__SWIG_0(), true) {
  }

  public TilePosition(Position position) : this(bwapiPINVOKE.new_TilePosition__SWIG_1(Position.getCPtr(position)), true) {
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public TilePosition(int x, int y) : this(bwapiPINVOKE.new_TilePosition__SWIG_2(x, y), true) {
  }

  public bool opEquals(TilePosition TilePosition) {
    bool ret = bwapiPINVOKE.TilePosition_opEquals(swigCPtr, TilePosition.getCPtr(TilePosition));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool opNotEquals(TilePosition TilePosition) {
    bool ret = bwapiPINVOKE.TilePosition_opNotEquals(swigCPtr, TilePosition.getCPtr(TilePosition));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool opLessThan(TilePosition TilePosition) {
    bool ret = bwapiPINVOKE.TilePosition_opLessThan(swigCPtr, TilePosition.getCPtr(TilePosition));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool isValid() {
    bool ret = bwapiPINVOKE.TilePosition_isValid(swigCPtr);
    return ret;
  }

  public bool opNonzero() {
    bool ret = bwapiPINVOKE.TilePosition_opNonzero(swigCPtr);
    return ret;
  }

  public TilePosition opPlus(TilePosition position) {
    TilePosition ret = new TilePosition(bwapiPINVOKE.TilePosition_opPlus(swigCPtr, TilePosition.getCPtr(position)), true);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public TilePosition opMinus(TilePosition position) {
    TilePosition ret = new TilePosition(bwapiPINVOKE.TilePosition_opMinus(swigCPtr, TilePosition.getCPtr(position)), true);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public TilePosition makeValid() {
    TilePosition ret = new TilePosition(bwapiPINVOKE.TilePosition_makeValid(swigCPtr), false);
    return ret;
  }

  public TilePosition opAdd(TilePosition position) {
    TilePosition ret = new TilePosition(bwapiPINVOKE.TilePosition_opAdd(swigCPtr, TilePosition.getCPtr(position)), false);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public TilePosition opSubtract(TilePosition position) {
    TilePosition ret = new TilePosition(bwapiPINVOKE.TilePosition_opSubtract(swigCPtr, TilePosition.getCPtr(position)), false);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public double getDistance(TilePosition position) {
    double ret = bwapiPINVOKE.TilePosition_getDistance(swigCPtr, TilePosition.getCPtr(position));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public double getLength() {
    double ret = bwapiPINVOKE.TilePosition_getLength(swigCPtr);
    return ret;
  }

  public bool hasPath(TilePosition destination) {
    bool ret = bwapiPINVOKE.TilePosition_hasPath(swigCPtr, TilePosition.getCPtr(destination));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_int x() {
    SWIGTYPE_p_int ret = new SWIGTYPE_p_int(bwapiPINVOKE.TilePosition_x(swigCPtr), false);
    return ret;
  }

  public SWIGTYPE_p_int y() {
    SWIGTYPE_p_int ret = new SWIGTYPE_p_int(bwapiPINVOKE.TilePosition_y(swigCPtr), false);
    return ret;
  }

  public int xConst() {
    int ret = bwapiPINVOKE.TilePosition_xConst(swigCPtr);
    return ret;
  }

  public int yConst() {
    int ret = bwapiPINVOKE.TilePosition_yConst(swigCPtr);
    return ret;
  }

}

}
