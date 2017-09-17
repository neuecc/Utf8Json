namespace Utf8Json
{
    public enum JsonToken : byte
    {
        None,
        /// <summary>{</summary>
        BeginObject,
        /// <summary>}</summary>
        EndObject,
        /// <summary>[</summary>
        BeginArray,
        /// <summary>]</summary>
        EndArray,
        /// <summary>0~9, -</summary>
        Number,
        /// <summary>"</summary>
        String,
        /// <summary>t</summary>
        True,
        /// <summary>f</summary>
        False,
        /// <summary>n</summary>
        Null,
        /// <summary>,</summary>
        ValueSeparator,
        /// <summary>:</summary>
        NameSeparator,
    }
}