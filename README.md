## What is a tangl? 
A tangl is a formal relationship between two otherwise unconnected things (classes, members, etc.). A tangl prevents run-time errors by keeping types in sync and managing [model pollution](https://github.com/mnbeer/Tangl/wiki/Model-Pollution#what-is-model-pollution). To see an example of the type of error a tangl can prevent, [go to this page in the wiki](wiki/Preventing-Errors#how-does-a-tangl-prevent-errors).

The name is (very, very loosely) inspired by quantum entanglement where:

> each particle of the pair or group cannot be described independently of the state of the others, including when the particles are separated by a large distance.
> https://en.wikipedia.org/wiki/Quantum_entanglement

* [What does this repository do?](#what-does-this-repository-do)
* [Can I see an example](#can-i-see-an-example)
* [Go to the wiki for more info](https://github.com/mnbeer/Tangl/wiki)

## What does this repository do?
It allows C# developers to tangl class properties.

## Can I see an example?

Here are three sample classes. The first two are domain classes and define a Person and an Address. A person may have multiple addresses. 

```c#
namespace Example.Domain
{
    public class Person
    {
        public int PersonId { get; set; }
        public List<Address> Addresses { get; set; }        
        // other properties here
    }
    
    public class Address
    {
        public int AddressId { get; set; }
        public int PersonId { get; set; }
        public string AddressType { get; set; }
        // other properties here
    }
}

```


The third class is a model (or data transfer object - DTO, if you prefer) that combines some elements of both classes to be shipped to a UI for display.

```c#
namespace Example
{
    class PersonViewModel
    {     
        public int PersonId { get; set; }
        public Address HomeAddress { get; set; }
        // other properties here
    }
}

```
Now, what happens when the data adminstrator changes the key type of the People table's primary key PersonId from an _int_ to a _long_? The application developer gets a ticket, quickly changes Person.PersonId from an _int_ to a _long_, compiles successfully and moves on.

And after that, mysterious errors start popping up at various points in the chain because longer numbers cannot be forced into smaller ones.

Tangls solve the problem by explicitly tying PersonViewModel.PersonId to Person.PersonId. If one is changed, warnings are raised and fixes provided.

It is as simple as defining the property with a Tangl attribute, like this:


```c#
namespace Example
{
    class PersonViewModel
    {     
        [Tangl("Example.Person.PersonId")]
        public int PersonId { get; set; }
        public Address HomeAddress { get; set; }
        // other properties here
    }
}

```

or, to avoid strings

```c#
namespace Example
{
    class PersonViewModel
    {     
        [Tangl(typeof(Example.Domain.Person), nameof(Person.PersonId))]
        public int PersonId { get; set; }
        public Address HomeAddress { get; set; }
        // other properties here
    }
}

```

This is what Visual Studio looks like when it discovers a broken tangl.

![Fix Provider](https://github.com/mnbeer/tangl/wiki/images/tangl-fix.png)

