Semi colons are required to end a statement or declaration

A delegate (runtime name for a typed function pointer) is defined with the following type of code. Can return any type, and take any number of parameters (within reason)
```csharp
delegate void MyFunctionPointer(int a, string b);
```

Classes are defined with the following snippets. Classes are namespaced with `::`, no way to namespace all classes in a file. The class name is the full name, so 2 classes with the same name can be in different namespaces
```csharp
class MyClass {

}

class MyNamespace::MyClass {}
```

Comments are `#`. No multiline comments
```
# this is a comment.
```

Inside a class can be any number of fields. Static fields are not supported.
```csharp
  field int x; # field named x of type int

  field auto x; # auto field not supported

  field int z = 15; # fields can be initialized to a constant

  field auto v = 15; # auto field not supported even with type

  field int a = "hello"; # types must match

```

Inside a class can be any number of constructors. A constructor can not have the same signature. Signature means same number of parameters, taking the same parameter types.
```csharp
  constructor() {
    ...
  }

  constructor(int a, int b) {
    ...
  }
```

Inside a class can be any number of methods.
Methods have a return type.
Methods have an identifier as a name
Methods can be marked with addition attributes entrypoint or static, but not both. Only 1 entrypoint allowed per program, and it is implictly static
Methods can have any number of parameters.
A method is not allowed to have the same name, return type, or parameters as another function. However any change in any of these is allowed (with an exception for same name and parameters, but different return type. Support as functions, but not supported as callees)
Entrypoint can be any name, must be void or int with either no parameters or a string[] parameter (so 4 allowed combinations)
Parameters with the same name as a field will shadow the field
```csharp
  method void MyMethod() {}
  method static void MyStaticMethod() {}
  method entrypoint void MyEntrypoint() {}
  method int MyOverloadedMethod(int x) {}
  method int MyOverloadedMethod(string x) {}
  method string MyOverloadedMethod() {} # allowed, as parameters are different
  method int MyOverloadedMethod() {} # can be compiled, but being called is undefined for which overload is taken.
  # Many more examples but they all match this.
```

Locals can be declared anywhere in a method as a statement. Locals will shadow same named parameters, and will shadow locals. `auto` is allowed, but only for initialized variables. Uninitialized variables are implicitly 0 or null. Local names must be unique between all locals
```csharp
  auto x; # not allowed, need an expression to init to
  auto x = 42; # allowed, type int
  int x; # allowed
  string x; # not allowed if int x is defined as above.
  auto x = 42 + 25; # expressions are allowed for initialization
  int x = CallIntReturningMethod(); #
```

Static methods on any type must be called with full namespaced syntax. If in root namespace, name of class is used as identifier
```csharp
  System::Console.WriteLine(56); # Calling WriteLine(int) in the System::Console class
  MyClass.MyStaticFunction("Hello"); # Calling MyStaticFunction in the MyClass class, which has no namespace
```

Instances of objects can be created with new.
```csharp
  object x = new object();
  MyClass n = new MyClass();
  MyClass z = new MyClass(42);
```

Instance methods can be accessed with a `.`. These can also be nested. `this` is a local variable to access the local instance in an instance method. Instance fields are accessed the same way, except `this` does not need to be explicitly defined. For a method call on `this`, `this` cannot be inferred.
```csharp
  MyClass n = new MyClass();
  n.PrintLine(42); # Calls PrintLine(int) method on the n instance

  this.PrintLine(56) # Calls PrintLine(int) on this. Assuming in an instance method
  PrintLine(56) # Not allowed, even if PrintLine(int) exists

  this.a = 56;
  this.PrintLine(this.a);
```

Arrays can be created with newarr. Indexing works like arrays in C. There is no way to get the length of an array currently
```csharp
  int[] arr = newarr int(56); # the parameter is the length
  arr[42] = 86;
  arr[51] = arr[42];
```

A delegate can be created to point to a function.
```csharp
delegate int MyDelegate(int a, int b);

class DelegateTester {
  method int MyMethod(int a, int b) {
    return a + b;
  }

  method void MyInstanceMethod() {
    MyDelegate del = this.MyMethod; # Delegate points to this
    del.Invoke(15, 36); # Invoke calls a delegate
  }

  method static void TakeDelegate(MyDelegate del) {
    del.Invoke(42, 56);
  }

  method static void MyStatic() {
    #MyDelegate del = DelegateTester.MyMethod; # Not allowed, can't take a delegate to a static. This will compile however, which is a bug
    auto n = new DelegateTester();
    MyDelegate del = n.MyMethod;

    DelegateTester.TakeDelegate(del); # Can pass a delegate

    #auto autoDel = n.MyMethod; # Can't type infer delegates, as mutliple delegates with same signature could exist
    #DelegateTester.TakeDelegate(n.MyMethod); # Can't create a delegate inline to call a function. Delegate must be explicitly created before
  }
}
```


