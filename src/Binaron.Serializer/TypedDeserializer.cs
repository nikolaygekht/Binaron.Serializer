using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Binaron.Serializer.Accessors;
using Binaron.Serializer.Creators;
using Binaron.Serializer.Enums;
using Binaron.Serializer.Extensions;
using Binaron.Serializer.Infrastructure;
using Activator = System.Activator;
using TypeCode = Binaron.Serializer.Enums.TypeCode;

namespace Binaron.Serializer
{
    internal static class TypedDeserializer
    {
        private interface IObjectReader
        {
            object Read(ReaderState reader);
        }

        private static class GetObjectReaderGeneric<T>
        {
            public static readonly IObjectReader Reader = ObjectReaders.CreateReader<T>();
        }

        public static object ReadObject<T>(ReaderState reader, object identifier)
        {
            if (reader.CustomObjectFactories == null || !reader.CustomObjectFactories.TryGetValue(typeof(T), out var customObjectCreator))
                return GetObjectReaderGeneric<T>.Reader.Read(reader);

            var result = customObjectCreator.Create(identifier);
            ObjectReaders.Populate<T>(reader, result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadObject<T>(ReaderState reader) => GetObjectReaderGeneric<T>.Reader.Read(reader);

        public static object ReadValue<T>(ReaderState reader)
        {
            var valueType = (SerializedType) reader.Read<byte>();
            return ReadValue<T>(reader, valueType);
        }

        private interface IValueReader
        {
            object Read(ReaderState reader, SerializedType valueType);
        }

        private static class GetValueReader<T>
        {
            public static readonly IValueReader Reader = ValueReaders<T>.CreateReader();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadValue<T>(ReaderState reader, SerializedType valueType) => GetValueReader<T>.Reader.Read(reader, valueType);

        public static void DiscardValue(ReaderState reader, SerializedType? knownType = null)
        {
            var valueType = knownType ?? (SerializedType) reader.Read<byte>();
            switch (valueType)
            {
                case SerializedType.CustomObject:
                    DiscardValue(reader); // custom object ID
                    while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem) 
                    { 
                        reader.ReadString(); // key 
                        DiscardValue(reader); // value 
                    } 
                    break; 
                case SerializedType.Object: 
                    while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem) 
                    { 
                        reader.ReadString(); // key 
                        DiscardValue(reader); // value 
                    } 
                    break; 
                case SerializedType.Dictionary: 
                    var kvpCount = reader.Read<int>(); 
                    for (var i = 0; i < kvpCount; i++) 
                    { 
                        DiscardValue(reader); // key 
                        DiscardValue(reader); // value 
                    } 
                    break; 
                case SerializedType.List: 
                    var count = reader.Read<int>(); 
                    for (var i = 0; i < count; i++) 
                        DiscardValue(reader); 
                    break; 
                case SerializedType.Enumerable: 
                    while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem) 
                        DiscardValue(reader); 
                    break;    
                case SerializedType.String:
                    Reader.ReadString(reader);
                    break;
                case SerializedType.Byte:
                    Reader.ReadByte(reader);
                    break;
                case SerializedType.SByte:
                    Reader.ReadSByte(reader);
                    break;
                case SerializedType.UShort:
                    Reader.ReadUShort(reader);
                    break;
                case SerializedType.Short:
                    Reader.ReadShort(reader);
                    break;
                case SerializedType.UInt:
                    Reader.ReadUInt(reader);
                    break;
                case SerializedType.Int:
                    Reader.ReadInt(reader);
                    break;
                case SerializedType.ULong:
                    Reader.ReadULong(reader);
                    break;
                case SerializedType.Long:
                    Reader.ReadLong(reader);
                    break;
                case SerializedType.Float:
                    Reader.ReadFloat(reader);
                    break;
                case SerializedType.Double:
                    Reader.ReadDouble(reader);
                    break;
                case SerializedType.Bool:
                    Reader.ReadBool(reader);
                    break;
                case SerializedType.Decimal:
                    Reader.ReadDecimal(reader);
                    break;
                case SerializedType.DateTime:
                    Reader.ReadDateTime(reader);
                    break;
                case SerializedType.Guid:
                    Reader.ReadGuid(reader);
                    break;
                case SerializedType.Char:
                    Reader.ReadChar(reader);
                    break;
                case SerializedType.Null:
                    break;
                default:
                    throw new NotSupportedException($"SerializedType '{valueType}' is not supported");
            }
        }

        private interface IDictionaryReader
        {
            object Read(ReaderState reader);
        }

        private static class GetDictionaryReader<T>
        {
            public static readonly IDictionaryReader Reader = DictionaryReaders.CreateReader<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadDictionary<T>(ReaderState reader) => GetDictionaryReader<T>.Reader.Read(reader);

        private static object ReadDictionaryNonGeneric<T>(ReaderState reader)
        {
            var count = reader.Read<int>();
            var result = CreateDictionaryResultObject<T>(count);
            switch (result)
            {
                case IDictionary d:
                {
                    for (var i = 0; i < count; i++)
                    {
                        var key = Deserializer.ReadValue(reader);
                        var value = Deserializer.ReadValue(reader);
                        d.Add(key, value);
                    }

                    break;
                }
                default:
                {
                    for (var i = 0; i < count; i++)
                    {
                        DiscardValue(reader); // key
                        DiscardValue(reader); // value
                    }

                    break;
                }
            }

            return result;
        }

        private interface IEnumerableReader
        {
            object Read(ReaderState reader);
        }

        private static class GetEnumerableReader<T>
        {
            public static readonly IEnumerableReader Reader = EnumerableReaders.CreateReader<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadEnumerable<T>(ReaderState reader) => GetEnumerableReader<T>.Reader.Read(reader);

        private static object ReadEnumerableNonGeneric<T>(ReaderState reader)
        {
            var result = CreateResultObject<T>();
            if (result is IList l)
            {
                while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem)
                    l.Add(Deserializer.ReadValue(reader));
            }
            else
            {
                while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem)
                    DiscardValue(reader);
            }

            return result;
        }

        private interface IListReader
        {
            object Read(ReaderState reader);
        }

        private static class GetListReader<T>
        {
            public static readonly IListReader Reader = ListReaders.CreateReader<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadList<T>(ReaderState reader) => GetListReader<T>.Reader.Read(reader);

        private static object ReadListNonGeneric<T>(ReaderState reader)
        {
            var count = reader.Read<int>();
            var result = CreateResultObject<T>(count);
            if (result is IList l)
            {
                for (var i = 0; i < count; i++)
                    l.Add(Deserializer.ReadValue(reader));
            }
            else
            {
                for (var i = 0; i < count; i++)
                    DiscardValue(reader);
            }

            return result;
        }

        private static class EnumerableResultObjectCreator<T>
        {
            public static readonly ResultObjectCreator.Enumerable Creator = new ResultObjectCreator.Enumerable(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateResultObject<T>() => EnumerableResultObjectCreator<T>.Creator.Create();

        private static class ListResultObjectCreator<T>
        {
            public static readonly ResultObjectCreator.List Creator = new ResultObjectCreator.List(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateResultObject<T>(int count) => ListResultObjectCreator<T>.Creator.Create(ListCapacity.Clamp(count));

        private static class DictionaryResultObjectCreator<T>
        {
            public static readonly ResultObjectCreator.Dictionary Creator = new ResultObjectCreator.Dictionary(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateDictionaryResultObject<T>(int count)
        {
            return DictionaryResultObjectCreator<T>.Creator.Create(ListCapacity.Clamp(count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(T obj, ReaderState reader) => ObjectReaders.Populate<T>(reader, obj);

        private static class ObjectReaders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Populate<T>(ReaderState reader, object obj)
            {
                var type = obj.GetType();
                if (!typeof(T).IsAssignableFrom(type))
                    throw new InvalidCastException($"'{type}' cannot be assigned to '{typeof(T)}'");

                Populate(obj, reader, SetterHandler.GetSetterHandlers(type));
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Populate(object obj, ReaderState reader, IDictionary<string, IMemberSetterHandler<ReaderState>> setterHandlers)
            {
                while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem)
                {
                    var key = reader.ReadString();
                    if (!setterHandlers.TryGetValue(key, out var setter))
                    {
                        DiscardValue(reader);
                        continue;
                    }

                    setter.Handle(reader, obj);
                }
            }

            public static IObjectReader CreateReader<T>() => CreateReader(typeof(T));

            private static IObjectReader CreateReader(Type type) => type == typeof(object) ? new DynamicObjectReader() : CreateReader(type, SetterHandler.GetActivatorAndSetterHandlers(type));

            private static IObjectReader CreateReader(Type type, (Func<object> Activate, IDictionary<string, IMemberSetterHandler<ReaderState>> Setters, Type IDictionaryValueType, Type ActualType) typeInfo)
            {
                var valueType = typeInfo.IDictionaryValueType;
                if (valueType != null)
                    return new GenericDictionaryReader(type, valueType);

                if (typeof(IDictionary).IsAssignableFrom(typeInfo.ActualType))
                    return new DictionaryReader(type, typeInfo.Activate);

                return new ObjectReader(type, typeInfo.Activate, typeInfo.Setters);
            }

            private class ObjectReader : IObjectReader
            {
                private readonly Type type;
                private readonly Func<object> activate;
                private readonly IDictionary<string, IMemberSetterHandler<ReaderState>> setterHandlers;

                public ObjectReader(Type type, Func<object> activate, IDictionary<string, IMemberSetterHandler<ReaderState>> setterHandlers)
                {
                    this.type = type;
                    this.activate = activate;
                    this.setterHandlers = setterHandlers;
                }

                public object Read(ReaderState reader)
                {
                    var activator = reader.ObjectActivator;
                    var result = activator == null ? (activate ?? throw NoParamlessCtorException(type))() : activator.Create(type);
                    Populate(result, reader, setterHandlers);
                    return result;
                }
            }

            private class DictionaryReader : IObjectReader
            {
                private readonly Func<object> activate;

                public DictionaryReader(Type type, Func<object> activate)
                {
                    this.activate = activate ?? throw NoParamlessCtorException(type);
                }

                public object Read(ReaderState reader)
                {
                    var result = (IDictionary) activate();
                    while ((EnumerableType) reader.Read<byte>() == EnumerableType.HasItem)
                    {
                        var key = reader.ReadString();
                        var value = Deserializer.ReadValue(reader);
                        result.Add(key, value);
                    }
                    return result;
                }
            }

            private static Exception NoParamlessCtorException(Type type)
            {
                return new ArgumentException($"No parameterless constructor was found for type '{type.FullName}'", nameof(type));
            }

            private class GenericDictionaryReader : IObjectReader
            {
                private readonly Type type;
                private readonly Type elementType;

                public GenericDictionaryReader(Type type, Type elementType)
                {
                    this.type = type;
                    this.elementType = elementType;
                }

                public object Read(ReaderState reader) => GenericReader.ReadObjectAsDictionary(reader, type, elementType);
            }

            private class DynamicObjectReader : IObjectReader
            {
                public object Read(ReaderState reader) => Deserializer.ReadObject(reader);
            }
        }

        private static class DictionaryReaders
        {
            public static IDictionaryReader CreateReader<T>()
            {
                var type = typeof(T);
                var dictionaryGenericType = GenericType.GetIDictionaryReader(type);
                if (dictionaryGenericType.KeyType != null)
                    return new GenericDictionaryReader(type, dictionaryGenericType);

                if (type == typeof(object))
                    return new DynamicDictionaryReader();

                return new DictionaryReader<T>();
            }

            private class DynamicDictionaryReader : IDictionaryReader
            {
                public object Read(ReaderState reader) => Deserializer.ReadDictionary(reader);
            }

            private class DictionaryReader<T> : IDictionaryReader
            {
                public object Read(ReaderState reader) => ReadDictionaryNonGeneric<T>(reader);
            }

            private class GenericDictionaryReader : IDictionaryReader
            {
                private readonly Type type;
                private readonly (Type KeyType, Type ValueType) elementType;

                public GenericDictionaryReader(Type type, (Type KeyType, Type ValueType) elementType)
                {
                    this.type = type;
                    this.elementType = elementType;
                }

                public object Read(ReaderState reader)
                {
                    var count = reader.Read<int>();
                    return GenericReader.ReadDictionary(reader, type, elementType, count);
                }
            }
        }

        private static class EnumerableReaders
        {
            public static IEnumerableReader CreateReader<T>()
            {
                var type = typeof(T);
                var elementType = GenericType.GetIEnumerable(type);
                if (elementType.Type != null)
                {
                    if (elementType.Type.IsEnum)
                        return (IEnumerableReader) Activator.CreateInstance(typeof(GenericEnumEnumerableReader<,>).MakeGenericType(type, elementType.Type));

                    return (IEnumerableReader) Activator.CreateInstance(typeof(GenericEnumerableReader<,>).MakeGenericType(type, elementType.Type));
                }

                if (type == typeof(object))
                    return new DynamicEnumerableReader();

                return new EnumerableReader<T>();
            }

            private class DynamicEnumerableReader : IEnumerableReader
            {
                public object Read(ReaderState reader) => Deserializer.ReadEnumerable(reader);
            }

            private class GenericEnumEnumerableReader<T, TElement> : IEnumerableReader
            {
                public object Read(ReaderState reader) => GenericReader.ReadEnums<T, TElement>(reader);
            }

            private class EnumerableReader<T> : IEnumerableReader
            {
                public object Read(ReaderState reader) => ReadEnumerableNonGeneric<T>(reader);
            }

            private class GenericEnumerableReader<T, TElement> : IEnumerableReader
            {
                public object Read(ReaderState reader) => GenericReader.ReadTypedEnumerable<T, TElement>(reader);
            }
        }

        private static class ListReaders
        {
            public static IListReader CreateReader<T>()
            {
                var type = typeof(T);
                var elementType = GenericType.GetIEnumerable(type);
                if (elementType.Type != null)
                {
                    if (elementType.Type.IsEnum)
                        return (IListReader) Activator.CreateInstance(typeof(GenericEnumListReader<,>).MakeGenericType(type, elementType.Type));

                    return (IListReader) Activator.CreateInstance(typeof(GenericListReader<,>).MakeGenericType(type, elementType.Type));
                }

                if (type == typeof(object))
                    return new DynamicListReader();

                return new ListReader<T>();
            }

            private class DynamicListReader : IListReader
            {
                public object Read(ReaderState reader) => Deserializer.ReadList(reader);
            }

            private class GenericEnumListReader<T, TElement> : IListReader
            {
                public object Read(ReaderState reader)
                {
                    var count = reader.Read<int>();
                    return GenericReader.ReadEnums<T, TElement>(reader, count);
                }
            }

            private class ListReader<T> : IListReader
            {
                public object Read(ReaderState reader) => ReadListNonGeneric<T>(reader);
            }

            private class GenericListReader<T, TElement> : IListReader
            {
                public object Read(ReaderState reader)
                {
                    var count = reader.Read<int>();
                    return GenericReader.ReadTypedList<T, TElement>(reader, count);
                }
            }
        }

        private static class ValueReaders<T>
        {
            public static IValueReader CreateReader()
            {
                return TypeOf<T>.TypeCode switch
                {
                    TypeCode.Boolean => new BoolReader(),
                    TypeCode.Byte => new ByteReader(),
                    TypeCode.Char => new CharReader(),
                    TypeCode.DateTime => new DateTimeReader(),
                    TypeCode.Guid => new GuidReader(),
                    TypeCode.Decimal => new DecimalReader(),
                    TypeCode.Double => new DoubleReader(),
                    TypeCode.Int16 => new ShortReader(),
                    TypeCode.Int32 => new IntReader(),
                    TypeCode.Int64 => new LongReader(),
                    TypeCode.SByte => new SByteReader(),
                    TypeCode.Single => new FloatReader(),
                    TypeCode.String => new StringReader(),
                    TypeCode.UInt16 => new UShortReader(),
                    TypeCode.UInt32 => new UIntReader(),
                    TypeCode.UInt64 => new ULongReader(),
                    _ => new ObjectReader()
                };
            }

            private class BoolReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsBool(reader, valueType);
            }

            private class ByteReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsByte(reader, valueType);
            }

            private class CharReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsChar(reader, valueType);
            }

            private class DateTimeReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsDateTime(reader, valueType);
            }

            private class GuidReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsGuid(reader, valueType);
            }

            private class DecimalReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsDecimal(reader, valueType);
            }

            private class DoubleReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsDouble(reader, valueType);
            }

            private class ShortReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsShort(reader, valueType);
            }

            private class IntReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsInt(reader, valueType);
            }

            private class LongReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsLong(reader, valueType);
            }

            private class SByteReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsSByte(reader, valueType);
            }

            private class FloatReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsFloat(reader, valueType);
            }

            private class StringReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsString(reader, valueType);
            }

            private class UShortReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsUShort(reader, valueType);
            }

            private class UIntReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsUInt(reader, valueType);
            }

            private class ULongReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsULong(reader, valueType);
            }

            private class ObjectReader : IValueReader
            {
                public object Read(ReaderState reader, SerializedType valueType) => SelfUpgradingReader.ReadAsObject<T>(reader, valueType);
            }
        }
    }
}