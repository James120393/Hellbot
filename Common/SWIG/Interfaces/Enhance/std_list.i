/* -----------------------------------------------------------------------------
 * See the LICENSE file for information on copyright, usage and redistribution
 * of SWIG, and the README file for authors - http://www.swig.org/release.html.
 *
 * std_vector.i
 *
 * SWIG typemaps for std::vector<T>
 * C# implementation
 * The C# wrapper is made to look and feel like a C# System.Collections.Generic.List<> collection.
 * For .NET 1 compatibility, define SWIG_DOTNET_1 when compiling the C# code; then the C# wrapper is 
 * made to look and feel like a typesafe C# System.Collections.ArrayList.
 *
 * Note that IEnumerable<> is implemented in the proxy class which is useful for using LINQ with 
 * C++ std::vector wrappers. The IList<> interface is also implemented to provide enhanced functionality
 * whenever we are confident that the required C++ operator== is available. This is the case for when 
 * T is a primitive type or a pointer. If T does define an operator==, then use the SWIG_STD_VECTOR_ENHANCED
 * macro to obtain this enhanced functionality, for example:
 *
 *   SWIG_STD_VECTOR_ENHANCED(SomeNamespace::Klass)
 *   %template(VectKlass) std::vector<SomeNamespace::Klass>;
 *
 * Warning: heavy macro usage in this file. Use swig -E to get a sane view on the real file contents!
 * ----------------------------------------------------------------------------- */

// Warning: Use the typemaps here in the expectation that the macros they are in will change name.


%include <std_common.i>

// MACRO for use within the std::vector class body
%define SWIG_STD_LIST_MINIMUM_INTERNAL(CSINTERFACE, CONST_REFERENCE_TYPE, CTYPE...)
%typemap(csinterfaces) std::list<CTYPE > "global::System.IDisposable, global::System.Collections.IEnumerable\n , global::System.Collections.Generic.CSINTERFACE<$typemap(cstype, CTYPE)>\n\n";

%typemap(cscode) std::list<CTYPE > %{
  public $csclassname(global::System.Collections.ICollection c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach ($typemap(cstype, CTYPE) element in c) {
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
   
  public global::System.Collections.Generic.ICollection<$typemap(cstype, CTYPE)> Values {
    get {
      global::System.Collections.Generic.ICollection<$typemap(cstype, CTYPE)> values = new global::System.Collections.Generic.List<$typemap(cstype, CTYPE)>();
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
  public void CopyTo($typemap(cstype, CTYPE)[] array)
#endif
  {
    CopyTo(0, array, 0, this.Count);
  }

#if SWIG_DOTNET_1
  public void CopyTo(System.Array array, int arrayIndex)
#else
  public void CopyTo($typemap(cstype, CTYPE)[] array, int arrayIndex)
#endif
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

#if SWIG_DOTNET_1
  public void CopyTo(int index, System.Array array, int arrayIndex, int count)
#else
  public void CopyTo(int index, $typemap(cstype, CTYPE)[] array, int arrayIndex, int count)
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
  
  global::System.Collections.Generic.IList<$typemap(cstype, CTYPE)> keyList = new global::System.Collections.Generic.List<$typemap(cstype, CTYPE)>(this.Values);
    for (int i = 0; i < this.Count; i++) {
      $typemap(cstype, CTYPE) currentKey = keyList[i];
      array.SetValue( currentKey, arrayIndex+i);
    }
  }

  global::System.Collections.Generic.IEnumerator< $typemap(cstype, CTYPE) > global::System.Collections.Generic.IEnumerable< $typemap(cstype, CTYPE) >.GetEnumerator() {
    return new $csclassnameEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new $csclassnameEnumerator(this);
  }

  public $csclassnameEnumerator GetEnumerator() {
    return new $csclassnameEnumerator(this);
  }
  

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class $csclassnameEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<$typemap(cstype, CTYPE)>
  {
    private $csclassname collectionRef;
	private System.Collections.Generic.IList<$typemap(cstype, CTYPE)> keyCollection;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public $csclassnameEnumerator($csclassname collection) {
      collectionRef = collection;
      currentIndex = -1;
	  keyCollection = new System.Collections.Generic.List<$typemap(cstype, CTYPE)>(collection.Values);
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public $typemap(cstype, CTYPE) Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return ($typemap(cstype, CTYPE))currentObject;
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
%}

  public:
    typedef size_t size_type;
    typedef CTYPE value_type;
    typedef CONST_REFERENCE_TYPE const_reference;
    %rename(Clear) clear;
    void clear();
    %rename(Add) push_back;
    void push_back(const value_type& x);
    size_type size() const;
    list();
    list(const list &other);
    %extend {
      //list(int capacity) throw (std::out_of_range) {
       // std::list<CTYPE >* pv = 0;
       // if (capacity >= 0) {
        //  pv = new std::list<CTYPE >();
        //  pv->reserve(capacity);
       //} else {
       //   throw std::out_of_range("capacity");
       //}
       //return pv;
      //}

	  // create_iterator_begin() and get_next_key() work together to provide a collection of keys to C#
      %apply void *VOID_INT_PTR { std::list< CTYPE >::iterator *create_iterator_begin }
      %apply void *VOID_INT_PTR { std::list< CTYPE >::iterator *swigiterator }

      std::list< CTYPE >::iterator *create_iterator_begin() {
        return new std::list< CTYPE >::iterator($self->begin());
      }

      const value_type& get_next_key(std::list< CTYPE >::iterator *swigiterator) throw (std::out_of_range) {
        std::list< CTYPE >::iterator iter = *swigiterator;
        if (iter == $self->end()) {
          delete swigiterator;
          throw std::out_of_range("no more set elements");
        }
        (*swigiterator)++;
        return (*iter);
      }
    }
%enddef

%define SWIG_STD_LIST_MINIMUM(CTYPE...)
SWIG_STD_LIST_MINIMUM_INTERNAL(IEnumerable, const value_type&, CTYPE)
%enddef

// Extra methods added to the collection class if operator== is defined for the class being wrapped
// The class will then implement IList<>, which adds extra functionality
%define SWIG_STD_LIST_EXTRA_OP_EQUALS_EQUALS(CTYPE...)
    %extend {
      bool Contains(const value_type& value) {
        return std::find($self->begin(), $self->end(), value) != $self->end();
      }
     
      bool Remove(const value_type& value) {
        std::list<CTYPE >::iterator it = std::find($self->begin(), $self->end(), value);
        if (it != $self->end()) {
          $self->erase(it);
	  return true;
        }
        return false;
      }
    }
%enddef

// Macros for std::vector class specializations/enhancements
%define SWIG_STD_LIST_ENHANCED(CTYPE...)
namespace std {
  template<> class list<CTYPE > {
    SWIG_STD_LIST_MINIMUM_INTERNAL(ICollection, const value_type&, CTYPE)
    SWIG_STD_LIST_EXTRA_OP_EQUALS_EQUALS(CTYPE)
  };
}
%enddef


%{
#include <list>
#include <algorithm>
#include <stdexcept>
%}

%csmethodmodifiers std::list::getitemcopy "private"
%csmethodmodifiers std::list::getitem "private"
%csmethodmodifiers std::list::setitem "private"
%csmethodmodifiers std::list::size "private"


namespace std {
  // primary (unspecialized) class template for std::vector
  // does not require operator== to be defined
  template<class T> class list {
    SWIG_STD_LIST_MINIMUM(T)
  };
  // specializations for pointers
  template<class T> class list<T*> {
    SWIG_STD_LIST_MINIMUM_INTERNAL(ICollection, const value_type&, T*)
    SWIG_STD_LIST_EXTRA_OP_EQUALS_EQUALS(T*)
  };
  template<class T> class list<const T*> {
    SWIG_STD_LIST_MINIMUM_INTERNAL(ICollection, const value_type&, const T*)
    SWIG_STD_LIST_EXTRA_OP_EQUALS_EQUALS(const T*)
  };
  // bool is a bit different in the C++ standard
  template<> class list<bool> {
    SWIG_STD_LIST_MINIMUM_INTERNAL(ICollection, bool, bool)
    SWIG_STD_LIST_EXTRA_OP_EQUALS_EQUALS(bool)
  };
}

// template specializations for std::vector
// these provide extra collections methods as operator== is defined
SWIG_STD_LIST_ENHANCED(char)
SWIG_STD_LIST_ENHANCED(signed char)
SWIG_STD_LIST_ENHANCED(unsigned char)
SWIG_STD_LIST_ENHANCED(short)
SWIG_STD_LIST_ENHANCED(unsigned short)
SWIG_STD_LIST_ENHANCED(int)
SWIG_STD_LIST_ENHANCED(unsigned int)
SWIG_STD_LIST_ENHANCED(long)
SWIG_STD_LIST_ENHANCED(unsigned long)
SWIG_STD_LIST_ENHANCED(long long)
SWIG_STD_LIST_ENHANCED(unsigned long long)
SWIG_STD_LIST_ENHANCED(float)
SWIG_STD_LIST_ENHANCED(double)
SWIG_STD_LIST_ENHANCED(std::string) // also requires a %include <std_string.i>

