﻿using System;
using System.Linq;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents an event definition defined in a <see cref="TypeDefinition" />.
    /// </summary>
    public class EventDefinition : MetadataMember<MetadataRow<ushort, uint, uint>>, IHasSemantics, IResolvable
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<ITypeDefOrRef> _eventType;
        private readonly LazyValue<EventMap> _eventMap;
        private MetadataImage _image;

        public EventDefinition(string name, ITypeDefOrRef eventType)
            : base(new MetadataToken(MetadataTokenType.Event))
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            _name = new LazyValue<string>(name);
            _eventType = new LazyValue<ITypeDefOrRef>(eventType);

            _eventMap = new LazyValue<EventMap>();
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
        }

        internal EventDefinition(MetadataImage image, MetadataRow<EventAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Attributes = row.Column1;
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));

            _eventType = new LazyValue<ITypeDefOrRef>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.TypeDefOrRef);
                var eventTypeToken = encoder.DecodeIndex(row.Column3);
                return image.TryResolveMember(eventTypeToken, out var member) ? (ITypeDefOrRef) member : null;
            });
            
            _eventMap = new LazyValue<EventMap>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.EventMap);
                var mapRow = table.GetRowClosestToKey(1, row.MetadataToken.Rid);
                return mapRow != null ? (EventMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _eventMap.IsInitialized && _eventMap.Value != null 
            ? _eventMap.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the attributes of the event definition.
        /// </summary>
        public EventAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if (DeclaringType == null)
                    return Name;
                return DeclaringType.FullName + "::" + Name;
            }
        }

        /// <summary>
        /// Gets the event map of the type that this event is declared in.
        /// </summary>
        public EventMap EventMap
        {
            get => _eventMap.Value;
            internal set
            {
                _eventMap.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets the type declaring the event.
        /// </summary>
        public TypeDefinition DeclaringType => EventMap?.Parent;

        ITypeDefOrRef IMemberReference.DeclaringType => DeclaringType;

        /// <summary>
        /// Gets the delegate type of the event handler.
        /// </summary>
        public ITypeDefOrRef EventType
        {
            get => _eventType.Value;
            set => _eventType.Value = value;
        }

        /// <inheritdoc />
        public MethodSemanticsCollection Semantics
        {
            get;
        }


        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Gets the method responsible for adding a handler to the event (if present). 
        /// </summary>
        public MethodDefinition AddMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.AddOn));
                return semantic?.Method;
            }
        }

        /// <summary>
        /// Gets the method responsible for removing a handler to the event (if present). 
        /// </summary>
        public MethodDefinition RemoveMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.RemoveOn));
                return semantic?.Method;
            }
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}