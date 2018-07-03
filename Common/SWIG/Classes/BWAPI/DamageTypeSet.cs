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

public partial class DamageTypeSet : global::System.IDisposable 
#if !SWIG_DOTNET_1
    , global::System.Collections.Generic.ICollection<DamageType>
#endif
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal DamageTypeSet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(DamageTypeSet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~DamageTypeSet() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          bwapiPINVOKE.delete_DamageTypeSet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }


  
  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsReadOnly {
    get { 
      return false; 
    }
  }

#if !SWIG_DOTNET_1
 public global::System.Collections.Generic.ICollection<DamageType> Values {
    get {
      global::System.Collections.Generic.ICollection<DamageType> values = new global::System.Collections.Generic.List<DamageType>();
      global::System.IntPtr iter = create_iterator_begin();
      try {
		  for (int i = 0;i < size();i++){
			values.Add(get_next_key(iter));
		}
      } catch (global::System.ArgumentOutOfRangeException) {
      }
	  return values;
    }
  }
 
  public bool Contains(DamageType item) {
    if ( ContainsKey(item)) {
      return true;
    } else {
      return false;
    }
  }

  public void CopyTo(DamageType[] array) {
    CopyTo(array, 0);
  }

  public void CopyTo( DamageType[] array, int arrayIndex) {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (arrayIndex+this.Count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");

   global::System.Collections.Generic.IList<DamageType> keyList = new global::System.Collections.Generic.List<DamageType>(this.Values);
    for (int i = 0; i < this.Count; i++) {
      DamageType currentKey = keyList[i];
      array.SetValue( currentKey, arrayIndex+i);
    }
  }

  global::System.Collections.Generic.IEnumerator< DamageType> global::System.Collections.Generic.IEnumerable<DamageType>.GetEnumerator() {
    return new DamageTypeSetEnumerator(this);
  }

  global::System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
    return new DamageTypeSetEnumerator(this);
  }

  public DamageTypeSetEnumerator GetEnumerator() {
    return new DamageTypeSetEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class DamageTypeSetEnumerator : global::System.Collections.IEnumerator, 
      global::System.Collections.Generic.IEnumerator< DamageType>
  {
    private DamageTypeSet collectionRef;
    private global::System.Collections.Generic.IList<DamageType> keyCollection;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public DamageTypeSetEnumerator(DamageTypeSet collection) {
      collectionRef = collection;
      keyCollection = new global::System.Collections.Generic.List<DamageType>(collection.Values);
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public  DamageType Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return ( DamageType)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = keyCollection[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
	  if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
      currentIndex = -1;
      currentObject = null;
    }
  }
#endif
  

  public DamageTypeSet() : this(bwapiPINVOKE.new_DamageTypeSet__SWIG_0(), true) {
  }

  public DamageTypeSet(DamageTypeSet other) : this(bwapiPINVOKE.new_DamageTypeSet__SWIG_1(DamageTypeSet.getCPtr(other)), true) {
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = bwapiPINVOKE.DamageTypeSet_size(swigCPtr);
    return ret;
  }

  public bool empty() {
    bool ret = bwapiPINVOKE.DamageTypeSet_empty(swigCPtr);
    return ret;
  }

  public void Clear() {
    bwapiPINVOKE.DamageTypeSet_Clear(swigCPtr);
  }

  public DamageType getitem(DamageType key) {
    DamageType ret = new DamageType(bwapiPINVOKE.DamageTypeSet_getitem(swigCPtr, DamageType.getCPtr(key)), false);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ContainsKey(DamageType key) {
    bool ret = bwapiPINVOKE.DamageTypeSet_ContainsKey(swigCPtr, DamageType.getCPtr(key));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Add(DamageType key) {
    bwapiPINVOKE.DamageTypeSet_Add(swigCPtr, DamageType.getCPtr(key));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool Remove(DamageType key) {
    bool ret = bwapiPINVOKE.DamageTypeSet_Remove(swigCPtr, DamageType.getCPtr(key));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public global::System.IntPtr create_iterator_begin() {
    global::System.IntPtr ret = bwapiPINVOKE.DamageTypeSet_create_iterator_begin(swigCPtr);
    return ret;
  }

  public DamageType get_next_key(global::System.IntPtr swigiterator) {
    DamageType ret = new DamageType(bwapiPINVOKE.DamageTypeSet_get_next_key(swigCPtr, swigiterator), false);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}
