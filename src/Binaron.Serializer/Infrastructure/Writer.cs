using System;
using System.Runtime.CompilerServices;
using Binaron.Serializer.Enums;
using Binaron.Serializer.IeeeDecimal;

namespace Binaron.Serializer.Infrastructure
{
    internal static class Writer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, int val)
        {
            writer.Write((byte) SerializedType.Int);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, long val)
        {
            writer.Write((byte) SerializedType.Long);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, short val)
        {
            writer.Write((byte) SerializedType.Short);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, double val)
        {
            writer.Write((byte) SerializedType.Double);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, float val)
        {
            writer.Write((byte) SerializedType.Float);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, decimal val)
        {
            // Decimals are stored as IEEE 754-2008 Decimal128 format https://en.wikipedia.org/wiki/Decimal128_floating-point_format
            // The IEEE version has higher precision than .net's decimal implementation and is compatible with other platforms
            writer.Write((byte) SerializedType.Decimal);
            var d = new Decimal128(val);
            writer.Write(d.GetIeeeHighBits());
            writer.Write(d.GetIeeeLowBits());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, bool val)
        {
            writer.Write((byte) SerializedType.Bool);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, byte val)
        {
            writer.Write((byte) SerializedType.Byte);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, sbyte val)
        {
            writer.Write((byte) SerializedType.SByte);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, char val)
        {
            writer.Write((byte) SerializedType.Char);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, DateTime val)
        {
            writer.Write((byte) SerializedType.DateTime);
            writer.Write(val.ToUniversalTime().Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write(WriterState writer, Guid val)
        {
            writer.Write((byte) SerializedType.Guid);
            val.TryWriteBytes(writer.Reserve(sizeof(Guid)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, ushort val)
        {
            writer.Write((byte) SerializedType.UShort);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, uint val)
        {
            writer.Write((byte) SerializedType.UInt);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, ulong val)
        {
            writer.Write((byte) SerializedType.ULong);
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(WriterState writer, string val)
        {
            writer.Write((byte) SerializedType.String);
            writer.WriteString(val);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, int val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, long val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, short val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, double val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, float val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, decimal val)
        {
            // Decimals are stored as IEEE 754-2008 Decimal128 format https://en.wikipedia.org/wiki/Decimal128_floating-point_format
            // The IEEE version has higher precision than .net's decimal implementation and is compatible with other platforms
            var d = new Decimal128(val);
            writer.Write(d.GetIeeeHighBits());
            writer.Write(d.GetIeeeLowBits());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, bool val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, byte val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, sbyte val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, char val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, DateTime val)
        {
            writer.Write(val.ToUniversalTime().Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteValue(WriterState writer, Guid val)
        {
            val.TryWriteBytes(writer.Reserve(sizeof(Guid)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, ushort val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, uint val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, ulong val)
        {
            writer.Write(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(WriterState writer, string val)
        {
            writer.WriteString(val);
        }
    }
}