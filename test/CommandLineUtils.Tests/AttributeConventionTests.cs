// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit;

namespace McMaster.Extensions.CommandLineUtils.Tests
{
    public class AttributeConventionTests
    {
        private sealed class MyAttributeConvention : Attribute, IConvention
        {
            private readonly string _name;

            public MyAttributeConvention(string name)
            {
                _name = name;
            }

            public void Apply(ConventionContext context)
            {
                context.Application.Name = _name;
            }
        }

        [Fact]
        public void ItFindsAttributesOnType()
        {
            var app = new CommandLineApplication<Yellow>();
            app.Conventions.UseAttributes();
            Assert.Equal("yellow", app.Name);
        }

        [MyAttributeConvention("yellow")]
        private class Yellow
        {
        }

        [Fact]
        public void ItDoesNotIConventionOnMemberAttribute()
        {
            var app = new CommandLineApplication<YellowMember>();
            app.Conventions.UseAttributes();
            Assert.Null(app.Name);
        }

        private class YellowMember
        {
            [MyAttributeConvention("yellow")]
            private string? _color;

            public string? Color { get => _color; set => _color = value; }
        }

        [Fact]
        public void ItFindsMemberAttributes()
        {
            var app = new CommandLineApplication<YellowMemberWithDefault>();
            app.Conventions.UseAttributes();
            app.Parse();
            Assert.Equal("yellow", app.Model.Color);
        }

        private sealed class MyDefaultValue : Attribute, IMemberConvention
        {
            private readonly string _value;

            public MyDefaultValue(string value)
            {
                _value = value;
            }

            public void Apply(ConventionContext context, MemberInfo member)
            {
                if (member is FieldInfo field)
                {
                    context.Application.OnParsingComplete(r =>
                    {
                        var modelAccessor = context.ModelAccessor;
                        if (modelAccessor != null)
                        {
                            field.SetValue(modelAccessor.GetModel(), _value);
                        }
                    });
                }
            }
        }

        private class YellowMemberWithDefault
        {
            [MyDefaultValue("yellow")]
            private string? _color;

            public string? Color { get => _color; set => _color = value; }
        }

        [Fact]
        public void ItFindsMemberConventionsOnAllMembers()
        {
            var app = new CommandLineApplication<MemberProgram>();
            app.Conventions.UseAttributes();
            Assert.Equal(16, app.Commands.Count);
        }

#pragma warning disable CS0169, CS0649, CS0067, CS8618
        private class MemberProgram
        {
            [Custom]
            private string _privateField;

            [Custom]
            public string _publicField;

            [Custom]
            private static string s_privateStaticField;

            [Custom]
            public static string s_publicStaticField;


            [Custom]
            public void Method() { }

            [Custom]
            private void PrivateMethod() { }

            [Custom]
            public static void StaticMethod() { }

            [Custom]
            private static void StaticPrivateMethod() { }


            [Custom]
            public int Prop { get; set; }

            [Custom]
            private int PrivateProp { get; set; }

            [Custom]
            public static int StaticProp { get; set; }

            [Custom]
            private static int PrivateStaticProp { get; set; }


            [Custom]
            public event Action Event;

            [Custom]
            private event Action PrivateEvent;

            [Custom]
            public static event Action StaticEvent;

            [Custom]
            private static event Action PrivateStaticEvent;
        }
#pragma warning restore CS0169, CS0649, CS0067


        private class CustomAttribute : Attribute, IMemberConvention
        {
            public void Apply(ConventionContext context, MemberInfo member)
            {
                context.Application.Command(member.Name, _ => { });
            }
        }
    }
}
