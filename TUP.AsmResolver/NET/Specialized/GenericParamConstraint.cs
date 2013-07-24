﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParamConstraint : MetaDataMember
    {
        private GenericParameter _owner;
        private TypeReference _constraint;

        public GenericParamConstraint(MetaDataRow row)
            : base(row)
        {
        }

        public GenericParamConstraint(GenericParameter owner, TypeReference constraint)
            : base(new MetaDataRow(owner.TableIndex, 0U))
        {
            this._owner = owner;
            this._constraint = constraint;
        }

        public GenericParameter Owner
        {
            get
            {
                if (_owner == null)
                {
                    MetaDataTable table = _netheader.TablesHeap.GetTable(MetaDataTableType.GenericParam);
                    int index = Convert.ToInt32(_metadatarow._parts[0]) - 1;
                    if (index > 0 || index < table.Members.Length)
                        _owner = table.Members[index] as GenericParameter;
                }
                return _owner;
            }
        }
        public TypeReference Constraint
        {
            get
            {
                if (_constraint == null)
                {
                    _netheader.TablesHeap.TypeDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _constraint);

                }
                return _constraint;
            }
        }

        public override void ClearCache()
        {
            _owner = null;
            _constraint = null;
        }

        public override void LoadCache()
        {
            _owner = Owner;
            _constraint = Constraint;
        }
    }
}
