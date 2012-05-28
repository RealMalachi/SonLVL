﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace SonicRetro.SonLVL.API
{
    public class PatternIndex
    {
        public bool Priority { get; set; }
        private byte _pal;
        public byte Palette
        {
            get
            {
                return _pal;
            }
            set
            {
                _pal = (byte)(value & 0x3);
            }
        }
        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        private ushort _ind;

        [Browsable(false)]
        public ushort Tile
        {
            get
            {
                return _ind;
            }
            set
            {
                _ind = (ushort)(value & 0x7FF);
            }
        }

        [DisplayName("Tile")]
        [Editor(typeof(TileEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string _Tile
        {
            get
            {
                return _ind.ToString("X4");
            }
            set
            {
                _ind = (ushort)(ushort.Parse(value, System.Globalization.NumberStyles.HexNumber) & 0x7FF);
            }
        }

        public static int Size { get { return 2; } }

        public PatternIndex() { }

        public PatternIndex(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            Priority = (val & 0x8000) == 0x8000;
            Palette = (byte)((val >> 13) & 0x3);
            YFlip = (val & 0x1000) == 0x1000;
            XFlip = (val & 0x800) == 0x800;
            _ind = (ushort)(val & 0x7FF);
        }

        public byte[] GetBytes()
        {
            ushort val = _ind;
            if (XFlip) val |= 0x800;
            if (YFlip) val |= 0x1000;
            val |= (ushort)(Palette << 13);
            if (Priority) val |= 0x8000;
            return ByteConverter.GetBytes(val);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PatternIndex)) return false;
            PatternIndex other = (PatternIndex)obj;
            if (Priority != other.Priority) return false;
            if (Palette != other.Palette) return false;
            if (XFlip != other.XFlip) return false;
            if (YFlip != other.YFlip) return false;
            if (Tile != other.Tile) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Block
    {
        public PatternIndex[,] tiles { get; set; }

        public static int Size { get { return PatternIndex.Size * 4; } }

        public Block()
        {
            tiles = new PatternIndex[2, 2];
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    tiles[x, y] = new PatternIndex();
        }

        public Block(byte[] file, int address)
        {
            tiles = new PatternIndex[2, 2];
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    tiles[x, y] = new PatternIndex(file, address + ((x + (y * 2)) * PatternIndex.Size));
        }

        public byte[] GetBytes()
        {
            List<byte> val = new List<byte>();
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    val.AddRange(tiles[x, y].GetBytes());
            return val.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Block)) return false;
            Block other = (Block)obj;
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    if (tiles[x, y] != other.tiles[x, y]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum Solidity : byte
    {
        NotSolid = 0,
        TopSolid = 1,
        LRBSolid = 2,
        AllSolid = 3
    }

    public abstract class ChunkBlock
    {
        protected byte _so1;
        public Solidity Solid1
        {
            get
            {
                return (Solidity)_so1;
            }
            set
            {
                _so1 = (byte)(value & Solidity.AllSolid);
            }
        }

        public bool XFlip { get; set; }
        public bool YFlip { get; set; }
        public ushort _ind;
        [Browsable(false)]
        public ushort Block
        {
            get
            {
                return _ind;
            }
            set
            {
                _ind = (ushort)(value & 0x3FF);
            }
        }

        [DisplayName("Block")]
        [Editor(typeof(BlockEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string _Block
        {
            get
            {
                return _ind.ToString("X4");
            }
            set
            {
                _ind = (ushort)(ushort.Parse(value, System.Globalization.NumberStyles.HexNumber) & 0x3FF);
            }
        }

        public static int Size { get { return 2; } }

        public abstract byte[] GetBytes();

        public override bool Equals(object obj)
        {
            if (!(obj is ChunkBlock)) return false;
            ChunkBlock other = (ChunkBlock)obj;
            if (Solid1 != other.Solid1) return false;
            if (XFlip != other.XFlip) return false;
            if (YFlip != other.YFlip) return false;
            if (Block != other.Block) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class S2ChunkBlock : ChunkBlock
    {
        private byte _so2;
        public Solidity Solid2
        {
            get
            {
                return (Solidity)_so2;
            }
            set
            {
                _so2 = (byte)(value & Solidity.AllSolid);
            }
        }

        public S2ChunkBlock() { }

        public S2ChunkBlock(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            _so2 = (byte)((val >> 14) & 0x3);
            _so1 = (byte)((val >> 12) & 0x3);
            YFlip = (val & 0x800) == 0x800;
            XFlip = (val & 0x400) == 0x400;
            _ind = (ushort)(val & 0x3FF);
        }

        public override byte[] GetBytes()
        {
            ushort val = _ind;
            if (XFlip) val |= 0x400;
            if (YFlip) val |= 0x800;
            val |= (ushort)(_so1 << 12);
            val |= (ushort)(_so2 << 14);
            return ByteConverter.GetBytes(val);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is S2ChunkBlock)) return false;
            if (!base.Equals(obj)) return false;
            S2ChunkBlock other = (S2ChunkBlock)obj;
            if (Solid2 != other.Solid2) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class S1ChunkBlock : ChunkBlock
    {
        public S1ChunkBlock() { }

        public S1ChunkBlock(byte[] file, int address)
        {
            ushort val = ByteConverter.ToUInt16(file, address);
            _so1 = (byte)((val >> 13) & 0x3);
            YFlip = (val & 0x1000) == 0x1000;
            XFlip = (val & 0x800) == 0x800;
            _ind = (ushort)(val & 0x3FF);
        }

        public override byte[] GetBytes()
        {
            ushort val = _ind;
            if (XFlip) val |= 0x800;
            if (YFlip) val |= 0x1000;
            val |= (ushort)(_so1 << 13);
            return ByteConverter.GetBytes(val);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is S1ChunkBlock)) return false;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Chunk
    {
        public ChunkBlock[,] blocks { get; set; }

        private int size;

        public static int Size { get { return ChunkBlock.Size * (int)Math.Pow(LevelData.chunksz / 16, 2); } }

        public Chunk()
        {
            size = LevelData.chunksz / 16;
            blocks = new ChunkBlock[size, size];
            switch (LevelData.Level.ChunkFormat)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S1ChunkBlock();
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                case EngineVersion.SBoom:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S2ChunkBlock();
                    break;
            }
        }

        public Chunk(byte[] file, int address)
        {
            size = LevelData.chunksz / 16;
            blocks = new ChunkBlock[size, size];
            switch (LevelData.Level.ChunkFormat)
            {
                case EngineVersion.S1:
                case EngineVersion.SCD:
                case EngineVersion.SCDPC:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S1ChunkBlock(file, address + ((x + (y * size)) * ChunkBlock.Size));
                    break;
                case EngineVersion.S2:
                case EngineVersion.S2NA:
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                case EngineVersion.SBoom:
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                            blocks[x, y] = new S2ChunkBlock(file, address + ((x + (y * size)) * ChunkBlock.Size));
                    break;
            }
        }

        public byte[] GetBytes()
        {
            List<byte> val = new List<byte>();
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    val.AddRange(blocks[x, y].GetBytes());
            return val.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Chunk)) return false;
            Chunk other = (Chunk)obj;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (blocks[x, y] != other.blocks[x, y]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [TypeConverter(typeof(PositionConverter))]
    [Serializable]
    public class Position
    {
        [NonSerialized]
        private Entry ent;
        private ushort x, y;
        [Browsable(false)]
        public ushort X { get { if (ent != null) x = ent.X; return x; } set { x = value; if (ent != null) ent.X = value; } }
        [Browsable(false)]
        public ushort Y { get { if (ent != null) y = ent.Y; return y; } set { y = value; if (ent != null) ent.Y = value; } }

        [DisplayName("X")]
        [Description("The horizontal component of the position.")]
        public string XHex { get { return X.ToString("X4"); } set { X = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber); } }
        [DisplayName("Y")]
        [Description("The vertical component of the position.")]
        public string YHex { get { return Y.ToString("X4"); } set { Y = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber); } }

        public Position() { }

        public Position(Entry item)
        {
            ent = item;
            x = item.X;
            y = item.Y;
        }

        public Position(string data)
        {
            string[] a = data.Split(',');
            X = ushort.Parse(a[0], System.Globalization.NumberStyles.HexNumber);
            Y = ushort.Parse(a[1], System.Globalization.NumberStyles.HexNumber);
        }

        public Position(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X.ToString("X4") + ", " + Y.ToString("X4");
        }

        public ushort[] ToArray()
        {
            ushort[] result = new ushort[2];
            result[0] = X;
            result[1] = Y;
            return result;
        }

        public ushort this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }

    public class PositionConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Position))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Position)
                return ((Position)value).ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
                return new Position((string)value);
            return base.ConvertFrom(context, culture, value);
        }
    }

    [Serializable]
    public abstract class Entry : IComparable<Entry>
    {
        [Browsable(false)]
        public ushort X { get; set; }
        [NonSerialized]
        protected Position pos;
        [Category("Standard")]
        [Description("The location of the item within the level.")]
        public Position Position
        {
            get
            {
                return pos;
            }
            set
            {
                X = value.X;
                Y = value.Y;
                pos = new Position(this);
            }
        }
        [Browsable(false)]
        public ushort Y { get; set; }

        [Category("Standard")]
        [Description("The hexadecimal representation of the item.")]
        public string Data
        {
            get
            {
                byte[] value = GetBytes();
                List<string> stuff = new List<string>();
                for (int i = 0; i < value.Length; i += 2)
                    stuff.Add(ByteConverter.ToUInt16(value, i).ToString("X4"));
                return string.Join(" ", stuff.ToArray());
            }
            set
            {
                string data = string.Empty;
                foreach (char item in value)
                    if (!char.IsWhiteSpace(item))
                        data += item;
                byte[] bytes = GetBytes();
                data = data.PadRight(bytes.Length * 2, '0');
                data = data.Substring(0, bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = byte.Parse(data.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                FromBytes(bytes);
            }
        }

        public abstract byte[] GetBytes();

        public abstract void FromBytes(byte[] bytes);

        [Browsable(false)]
        public Sprite Sprite { get; protected set; }

        public abstract void UpdateSprite();

        [ReadOnly(true)]
        [ParenthesizePropertyName(true)]
        [Category("Meta")]
        [Description("The type of the item.")]
        public string Type
        {
            get
            {
                return GetType().Name;
            }
        }

        [ReadOnly(true)]
        [ParenthesizePropertyName(true)]
        [Category("Meta")]
        [Description("The name of the item.")]
        public abstract string Name { get; }

        int IComparable<Entry>.CompareTo(Entry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }
    }

    [Serializable]
    public abstract class ObjectEntry : Entry, IComparable<ObjectEntry>
    {
        [DefaultValue(false)]
        [Description("Flips the object vertically.")]
        [DisplayName("Y Flip")]
        public virtual bool YFlip { get; set; }
        [DefaultValue(false)]
        [Description("Flips the object horizontally.")]
        [DisplayName("X Flip")]
        public virtual bool XFlip { get; set; }
        private byte id;
        [Browsable(false)]
        public virtual byte ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                if (isLoaded) LevelData.ChangeObjectType(this);
            }
        }
        [DefaultValue("00")]
        [Description("The ID number of the object.")]
        [DisplayName("ID")]
        [Editor(typeof(IDEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual string _ID
        {
            get
            {
                return ID.ToString("X2");
            }
            set
            {
                ID = byte.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }
        [Browsable(false)]
        public virtual byte SubType { get; set; }
        [DefaultValue("00")]
        [Description("The subtype of the object.")]
        [DisplayName("Subtype")]
        [Editor(typeof(SubTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string _SubType
        {
            get
            {
                return SubType.ToString("X2");
            }
            set
            {
                SubType = byte.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        protected bool isLoaded = false;

        int IComparable<ObjectEntry>.CompareTo(ObjectEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }

        public override void UpdateSprite()
        {
            Sprite = LevelData.GetObjectDefinition(ID).GetSprite(this);
        }

        public override string Name
        {
            get
            {
                string ret = LevelData.unkobj.Name();
                if (LevelData.ObjTypes.ContainsKey(ID))
                    ret = LevelData.ObjTypes[ID].Name();
                return ret;
            }
        }
    }

    [DefaultProperty("ID")]
    [Serializable]
    public class S2ObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [Description("If true, the object will stay destroyed after it leaves the screen.")]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        [DefaultValue(false)]
        [DisplayName("Long Distance")]
        public bool LongDistance { get; set; }

        public static int Size { get { return 6; } }

        public S2ObjectEntry() { pos = new Position(this); isLoaded = true; }

        public S2ObjectEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (LongDistance) val |= 0x1000;
            if (XFlip) val |= 0x2000;
            if (YFlip) val |= 0x4000;
            if (RememberState) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add(ID);
            ret.Add(SubType);
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            ushort val = ByteConverter.ToUInt16(bytes, 2);
            RememberState = (val & 0x8000) == 0x8000;
            YFlip = (val & 0x4000) == 0x4000;
            XFlip = (val & 0x2000) == 0x2000;
            LongDistance = (val & 0x1000) == 0x1000;
            Y = (ushort)(val & 0xFFF);
            ID = bytes[4];
            SubType = bytes[5];
        }
    }

    [DefaultProperty("ID")]
    [Serializable]
    public class S1ObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [Description("If true, the object will stay destroyed after it leaves the screen.")]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        public override string _ID
        {
            get
            {
                return ID.ToString("X2");
            }
            set
            {
                ID = byte.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public override byte ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = (byte)(value & 0x7F);
            }
        }

        public static int Size { get { return 6; } }

        public S1ObjectEntry() { pos = new Position(this); isLoaded = true; }

        public S1ObjectEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x4000;
            if (YFlip) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add((byte)(ID | (RememberState ? 0x80 : 0)));
            ret.Add(SubType);
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            ushort val = ByteConverter.ToUInt16(bytes, 2);
            YFlip = (val & 0x8000) == 0x8000;
            XFlip = (val & 0x4000) == 0x4000;
            Y = (ushort)(val & 0xFFF);
            ID = bytes[4];
            RememberState = (bytes[4] & 0x80) == 0x80;
            SubType = bytes[5];
        }
    }

    [DefaultProperty("ID")]
    [Serializable]
    public class S3KObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [Description("If true, the object will be loaded when it is in horizontal range of the screen, regardless of its Y position.")]
        [DisplayName("Make object manager ignore Y position")]
        public virtual bool SomeFlag { get; set; }

        public static int Size { get { return 6; } }

        public S3KObjectEntry() { pos = new Position(this); isLoaded = true; }

        public S3KObjectEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x2000;
            if (YFlip) val |= 0x4000;
            if (SomeFlag) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add(ID);
            ret.Add(SubType);
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            ushort val = ByteConverter.ToUInt16(bytes, 2);
            SomeFlag = (val & 0x8000) == 0x8000;
            YFlip = (val & 0x4000) == 0x4000;
            XFlip = (val & 0x2000) == 0x2000;
            Y = (ushort)(val & 0xFFF);
            ID = bytes[4];
            SubType = bytes[5];
        }
    }

    [DefaultProperty("ID")]
    [Serializable]
    public class SCDObjectEntry : ObjectEntry
    {
        [DefaultValue(false)]
        [Description("If true, the object will stay destroyed after it leaves the screen.")]
        [DisplayName("Remember State")]
        public virtual bool RememberState { get; set; }

        [Description("If true, the object should be loaded in the Present time zone.")]
        [DisplayName("Show in Present")]
        public virtual bool ShowPresent { get; set; }
        [Description("If true, the object should be loaded in the Past time zone.")]
        [DisplayName("Show in Past")]
        public virtual bool ShowPast { get; set; }
        [Description("If true, the object should be loaded in the Future time zone.")]
        [DisplayName("Show in Future")]
        public virtual bool ShowFuture { get; set; }

        public override string _ID
        {
            get
            {
                return ID.ToString("X2");
            }
            set
            {
                ID = byte.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public override byte ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = (byte)(value & 0x7F);
            }
        }

        public static int Size { get { return 8; } }

        public SCDObjectEntry() { pos = new Position(this); isLoaded = true; }

        public SCDObjectEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
            isLoaded = true;
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            if (XFlip) val |= 0x4000;
            if (YFlip) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            ret.Add((byte)(ID | (RememberState ? 0x80 : 0)));
            ret.Add(SubType);
            byte b = 0;
            if (ShowPresent) b |= 0x40;
            if (ShowPast) b |= 0x80;
            if (ShowFuture) b |= 0x20;
            ret.Add(b);
            ret.Add(0);
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            ushort val = ByteConverter.ToUInt16(bytes, 2);
            YFlip = (val & 0x8000) == 0x8000;
            XFlip = (val & 0x4000) == 0x4000;
            Y = (ushort)(val & 0xFFF);
            ID = bytes[4];
            RememberState = (bytes[4] & 0x80) == 0x80;
            SubType = bytes[5];
            ShowPresent = (bytes[6] & 0x40) == 0x40;
            ShowPast = (bytes[6] & 0x80) == 0x80;
            ShowFuture = (bytes[6] & 0x20) == 0x20;
        }
    }

    [Serializable]
    public abstract class RingEntry : Entry, IComparable<RingEntry>
    {
        int IComparable<RingEntry>.CompareTo(RingEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }

        public override string Name
        {
            get { return "Ring"; }
        }
    }

    [DefaultProperty("Count")]
    [Serializable]
    public class S2RingEntry : RingEntry
    {
        [DefaultValue(Direction.Horizontal)]
        [Description("The direction of the ring group.")]
        public Direction Direction { get; set; }
        private byte _count;
        [DefaultValue(1)]
        [Description("The number of rings in this group.")]
        public byte Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = Math.Max(Math.Min(value, (byte)8), (byte)1);
            }
        }

        public static int Size { get { return 4; } }

        public S2RingEntry() { pos = new Position(this); Count = 1; }

        public S2RingEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ushort val = (ushort)(Y & 0xFFF);
            val |= (ushort)((Count - 1) << 12);
            if (Direction == Direction.Vertical) val |= 0x8000;
            ret.AddRange(ByteConverter.GetBytes(val));
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            ushort val = ByteConverter.ToUInt16(bytes, 2);
            Direction = (val & 0x8000) == 0x8000 ? Direction.Vertical : Direction.Horizontal;
            Count = (byte)(((val & 0x7000) >> 12) + 1);
            Y = (ushort)(val & 0xFFF);
        }

        public override void UpdateSprite()
        {
            Sprite = LevelData.S2RingDef.GetSprite(this);
        }

        public override string Name
        {
            get { return LevelData.S2RingDef.Name(); }
        }
    }

    [Serializable]
    public class S3KRingEntry : RingEntry
    {
        public static int Size { get { return 4; } }

        public S3KRingEntry() { pos = new Position(this); }

        public S3KRingEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            Y = ByteConverter.ToUInt16(bytes, 2);
        }

        public override void UpdateSprite()
        {
            Sprite = LevelData.S3KRingDef.GetSprite(this);
        }
    }

    public enum Direction
    {
        Horizontal,
        Vertical
    }

    [DefaultProperty("ID")]
    [Serializable]
    public class CNZBumperEntry : Entry, IComparable<CNZBumperEntry>
    {
        [Browsable(false)]
        public ushort ID { get; set; }

        [DefaultValue("0000")]
        [Description("The type of bumper.")]
        [DisplayName("ID")]
        public string _ID
        {
            get
            {
                return ID.ToString("X4");
            }
            set
            {
                ID = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public static int Size { get { return 6; } }

        public CNZBumperEntry() { pos = new Position(this); }

        public CNZBumperEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(ID));
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            ID = ByteConverter.ToUInt16(bytes, 0);
            X = ByteConverter.ToUInt16(bytes, 2);
            Y = ByteConverter.ToUInt16(bytes, 4);
        }

        int IComparable<CNZBumperEntry>.CompareTo(CNZBumperEntry other)
        {
            int c = X.CompareTo(other.X);
            if (c == 0) c = Y.CompareTo(other.Y);
            return c;
        }

        public override void UpdateSprite()
        {
            Sprite = LevelData.unkobj.GetSprite(new S2ObjectEntry() { X = X, Y = Y });
        }

        public override string Name
        {
            get { return "Bumper"; }
        }
    }

    public class StartPositionEntry : Entry
    {
        public static int Size { get { return 4; } }

        public StartPositionEntry() { pos = new Position(this); }

        public StartPositionEntry(byte[] file, int address)
        {
            byte[] bytes = new byte[Size];
            Array.Copy(file, address, bytes, 0, Size);
            FromBytes(bytes);
            pos = new Position(this);
        }

        public override byte[] GetBytes()
        {
            List<byte> ret = new List<byte>();
            ret.AddRange(ByteConverter.GetBytes(X));
            ret.AddRange(ByteConverter.GetBytes(Y));
            return ret.ToArray();
        }

        public override void FromBytes(byte[] bytes)
        {
            X = ByteConverter.ToUInt16(bytes, 0);
            Y = ByteConverter.ToUInt16(bytes, 2);
        }

        public override void UpdateSprite()
        {
            Sprite = LevelData.StartPosDefs[LevelData.StartPositions.IndexOf(this)].GetSprite(this);
        }

        public override string Name
        {
            get { return LevelData.StartPosDefs[LevelData.StartPositions.IndexOf(this)].Name(); }
        }
    }

    public class MappingsTile
    {
        public short Y { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public PatternIndex Tile { get; private set; }
        public PatternIndex Tile2 { get; private set; }
        public short X { get; private set; }

        public static int Size(EngineVersion version) { switch (version) { case EngineVersion.S1: return 5; case EngineVersion.S2: case EngineVersion.SBoom: return 8; default: return 6; } }

        public MappingsTile(byte[] file, int address, EngineVersion version)
        {
            Y = unchecked((sbyte)file[address]);
            Width = (byte)(((file[address + 1] & 0xC) >> 2) + 1);
            Height = (byte)((file[address + 1] & 0x3) + 1);
            Tile = new PatternIndex(file, address + 2);
            switch (version)
            {
                case EngineVersion.S1:
                    X = unchecked((sbyte)file[address + 4]);
                    break;
                case EngineVersion.S2:
                case EngineVersion.SBoom:
                    Tile2 = new PatternIndex(file, address + 4);
                    X = ByteConverter.ToInt16(file, address + 6);
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    X = ByteConverter.ToInt16(file, address + 4);
                    break;
            }
        }
    }

    public class MappingsFrame
    {
        public string Name { get; private set; }

        private MappingsTile[] Tiles { get; set; }
        public MappingsTile this[int index] { get { return Tiles[index]; } }

        public int TileCount { get { return Tiles.Length; } }

        public int Size(EngineVersion version) { return (Tiles.Length * MappingsTile.Size(version)) + (version == EngineVersion.S1 ? 1 : 2); }

        public static MappingsFrame[] Load(byte[] file, EngineVersion version)
        {
            return Load(file, version, null);
        }

        public static MappingsFrame[] Load(byte[] file, EngineVersion version, Dictionary<string, int> labels)
        {
            int[] addresses = LevelData.GetOffsetList(file);
            List<MappingsFrame> result = new List<MappingsFrame>();
            foreach (int item in addresses)
            {
                string name = "map_" + item.ToString("X4");
                if (labels != null && labels.ContainsValue(item))
                    foreach (KeyValuePair<string, int> label in labels)
                        if (label.Value == item)
                        {
                            name = label.Key;
                            break;
                        }
                result.Add(new MappingsFrame(file, item, version, name));
            }
            return result.ToArray();
        }

        public MappingsFrame(byte[] file, int address, EngineVersion version, string name)
        {
            Name = name;
            int tileCount;
            if (version == EngineVersion.S1)
                tileCount = file[address];
            else
                tileCount = ByteConverter.ToUInt16(file, address);
            Tiles = new MappingsTile[tileCount];
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i] = new MappingsTile(file, (i * MappingsTile.Size(version)) + address + (version == EngineVersion.S1 ? 1 : 2), version);
        }
    }

    public class DPLCEntry
    {
        public byte TileCount { get; set; }
        public ushort TileNum { get; set; }

        public static int Size { get { return 2; } }

        public DPLCEntry(byte[] file, int address, EngineVersion version)
        {
            switch (version)
            {
                case EngineVersion.S1:
                case EngineVersion.S2:
                case EngineVersion.SBoom:
                    TileNum = ByteConverter.ToUInt16(file, address);
                    TileCount = (byte)((TileNum >> 12) + 1);
                    TileNum &= 0xFFF;
                    break;
                case EngineVersion.S3K:
                case EngineVersion.SKC:
                    TileNum = ByteConverter.ToUInt16(file, address);
                    TileCount = (byte)((TileNum & 0xF) + 1);
                    TileNum = (ushort)(TileNum >> 4);
                    break;
            }
        }
    }

    public class DPLCFrame
    {
        public string Name { get; private set; }

        private DPLCEntry[] Tiles { get; set; }
        public DPLCEntry this[int index]
        {
            get { return Tiles[index]; }
        }

        public int Count { get { return Tiles.Length; } }

        public int Size(EngineVersion version) { return (Tiles.Length * DPLCEntry.Size) + (version == EngineVersion.S1 ? 1 : 2); }

        public static DPLCFrame[] Load(byte[] file, EngineVersion version)
        {
            return Load(file, version, null);
        }

        public static DPLCFrame[] Load(byte[] file, EngineVersion version, Dictionary<string, int> labels)
        {
            int[] addresses = LevelData.GetOffsetList(file);
            List<DPLCFrame> result = new List<DPLCFrame>();
            foreach (int item in addresses)
            {
                string name = "map_" + item.ToString("X4");
                if (labels != null && labels.ContainsValue(item))
                    foreach (KeyValuePair<string, int> label in labels)
                        if (label.Value == item)
                        {
                            name = label.Key;
                            break;
                        }
                result.Add(new DPLCFrame(file, item, version, name));
            }
            return result.ToArray();
        }

        public DPLCFrame(byte[] file, int address, EngineVersion version, string name)
        {
            try
            {
                Name = name;
                int tileCount = 0;
                switch (version)
                {
                    case EngineVersion.S1:
                        tileCount = file[address];
                        break;
                    case EngineVersion.S2NA:
                    case EngineVersion.S2:
                    case EngineVersion.SBoom:
                        tileCount = ByteConverter.ToUInt16(file, address);
                        break;
                    case EngineVersion.S3K:
                    case EngineVersion.SKC:
                        tileCount = ByteConverter.ToUInt16(file, address) + 1;
                        break;
                }
                Tiles = new DPLCEntry[tileCount];
                for (int i = 0; i < Tiles.Length; i++)
                    Tiles[i] = new DPLCEntry(file, (i * DPLCEntry.Size) + address + (version == EngineVersion.S1 ? 1 : 2), version);
            }
            catch { }
        }
    }

    [Serializable]
    public struct Sprite
    {
        public Point Offset;
        public BitmapBits Image;
        public int X { get { return Offset.X; } set { Offset.X = value; } }
        public int Y { get { return Offset.Y; } set { Offset.Y = value; } }
        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }
        public Size Size { get { return Image.Size; } }
        public int Left { get { return X; } }
        public int Top { get { return Y; } }
        public int Right { get { return X + Width; } }
        public int Bottom { get { return Y + Height; } }
        public Rectangle Bounds { get { return new Rectangle(Offset, Size); } }

        public Sprite(BitmapBits spr, Point off)
        {
            Image = spr;
            Offset = off;
        }

        public Sprite(params Sprite[] sprites)
        {
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                left = Math.Min(sprites[i].Left, left);
                right = Math.Max(sprites[i].Right, right);
                top = Math.Min(sprites[i].Top, top);
                bottom = Math.Max(sprites[i].Bottom, bottom);
            }
            Offset = new Point(left, top);
            Image = new BitmapBits(right - left, bottom - top);
            for (int i = 0; i < sprites.Length; i++)
                Image.DrawBitmapComposited(sprites[i].Image, new Point(sprites[i].X - left, sprites[i].Y - top));
        }
    }
}