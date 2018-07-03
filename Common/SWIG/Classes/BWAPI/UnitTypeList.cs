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

public partial class UnitTypeList : global::System.IDisposable, global::System.Collections.IEnumerable
 , global::System.Collections.Generic.IEnumerable<UnitType>

 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UnitTypeList(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UnitTypeList obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UnitTypeList() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          bwapiPINVOKE.delete_UnitTypeList(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public UnitTypeList(global::System.Collections.ICollection c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (UnitType element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }
  
  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }
   
  public global::System.Collections.Generic.ICollection<UnitType> Values {
    get {
      global::System.Collections.Generic.ICollection<UnitType> values = new global::System.Collections.Generic.List<UnitType>();
      global::System.IntPtr iter = create_iterator_begin();
      try {
        while (true) {
          values.Add(get_next_key(iter));
        }
      } catch (global::System.ArgumentOutOfRangeException) {
      }
      return values;
    }
  }

#if SWIG_DOTNET_1
  public void CopyTo(System.Array array)
#else
  public void CopyTo(UnitType[] array)
#endif
  {
    CopyTo(0, array, 0, this.Count);
  }

#if SWIG_DOTNET_1
  public void CopyTo(System.Array array, int arrayIndex)
#else
  public void CopyTo(UnitType[] array, int arrayIndex)
#endif
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

#if SWIG_DOTNET_1
  public void CopyTo(int index, System.Array array, int arrayIndex, int count)
#else
  public void CopyTo(int index, UnitType[] array, int arrayIndex, int count)
#endif
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
  
  global::System.Collections.Generic.IList<UnitType> keyList = new global::System.Collections.Generic.List<UnitType>(this.Values);
    for (int i = 0; i < this.Count; i++) {
      UnitType currentKey = keyList[i];
      array.SetValue( currentKey, arrayIndex+i);
    }
  }

  global::System.Collections.Generic.IEnumerator< UnitType > global::System.Collections.Generic.IEnumerable< UnitType >.GetEnumerator() {
    return new UnitTypeListEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new UnitTypeListEnumerator(this);
  }

  public UnitTypeListEnumerator GetEnumerator() {
    return new UnitTypeListEnumerator(this);
  }
  

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class UnitTypeListEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<UnitType>
  {
    private UnitTypeList collectionRef;
	private System.Collections.Generic.IList<UnitType> keyCollection;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public UnitTypeListEnumerator(UnitTypeList collection) {
      collectionRef = collection;
      currentIndex = -1;
	  keyCollection = new System.Collections.Generic.List<UnitType>(collection.Values);
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public UnitType Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (UnitType)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = keyCollection.Count;
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

#if !SWIG_DOTNET_1
    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
#endif
  }

  public void Clear() {
    bwapiPINVOKE.UnitTypeList_Clear(swigCPtr);
  }

  public void Add(UnitType x) {
    bwapiPINVOKE.UnitTypeList_Add(swigCPtr, UnitType.getCPtr(x));
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = bwapiPINVOKE.UnitTypeList_size(swigCPtr);
    return ret;
  }

  public UnitTypeList() : this(bwapiPINVOKE.new_UnitTypeList__SWIG_0(), true) {
  }

  public UnitTypeList(UnitTypeList other) : this(bwapiPINVOKE.new_UnitTypeList__SWIG_1(UnitTypeList.getCPtr(other)), true) {
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public global::System.IntPtr create_iterator_begin() {
    global::System.IntPtr ret = bwapiPINVOKE.UnitTypeList_create_iterator_begin(swigCPtr);
    return ret;
  }

  public UnitType get_next_key(global::System.IntPtr swigiterator) {
    UnitType ret = new UnitType(bwapiPINVOKE.UnitTypeList_get_next_key(swigCPtr, swigiterator), false);
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}
