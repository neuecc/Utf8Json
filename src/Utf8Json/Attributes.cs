﻿using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Formatters;

namespace Utf8Json
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonFormatterAttribute : Attribute
    {
        public Type FormatterType { get; private set; }
        public object[] Arguments { get; private set; }

        public JsonFormatterAttribute(Type formatterType)
        {
            this.FormatterType = formatterType;
        }

        public JsonFormatterAttribute(Type formatterType, params object[] arguments)
        {
            this.FormatterType = formatterType;
            this.Arguments = arguments;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PolymorphicFormatterAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class SerializationConstructorAttribute : Attribute
    {

    }
}
