//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace SWIG.BWAPIC {
 
	// defaults
	using System; 
	using System.Runtime.InteropServices; 
	// BWAPI
	using BWAPI;

public partial class Client : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal Client(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Client obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~Client() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          bwapiclientPINVOKE.delete_Client(swigCPtr);
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
    if (obj is Client)
      equal = (((Client)obj).swigCPtr.Handle == this.swigCPtr.Handle);
    return equal;
}
  
public bool Equals(Client obj) 
{
    if (obj == null) return false;
    return (obj.swigCPtr.Handle == this.swigCPtr.Handle);
}

public static bool operator ==(Client obj1, Client obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return true;
    if (object.ReferenceEquals(obj1, null)) return false;
    if (object.ReferenceEquals(obj2, null)) return false;
   
    return obj1.Equals(obj2);
}

public static bool operator !=(Client obj1, Client obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return false;
    if (object.ReferenceEquals(obj1, null)) return true;
    if (object.ReferenceEquals(obj2, null)) return true;

    return !obj1.Equals(obj2);
}




  public Client() : this(bwapiclientPINVOKE.new_Client(), true) {
  }

  public GameData data {
    set {
      bwapiclientPINVOKE.Client_data_set(swigCPtr, GameData.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = bwapiclientPINVOKE.Client_data_get(swigCPtr);
      GameData ret = (cPtr == global::System.IntPtr.Zero) ? null : new GameData(cPtr, false);
      return ret;
    } 
  }

  public bool isConnected() {
    bool ret = bwapiclientPINVOKE.Client_isConnected(swigCPtr);
    return ret;
  }

  public bool connect() {
    bool ret = bwapiclientPINVOKE.Client_connect(swigCPtr);
    return ret;
  }

  public void disconnect() {
    bwapiclientPINVOKE.Client_disconnect(swigCPtr);
  }

  public void update() {
    bwapiclientPINVOKE.Client_update(swigCPtr);
  }

}

}
