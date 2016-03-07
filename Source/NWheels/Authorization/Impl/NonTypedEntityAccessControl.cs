﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.DataObjects;

namespace NWheels.Authorization.Impl
{
    internal class NonTypedEntityAccessControl : IEntityAccessControl, INonTypedEntityAccessControlBuilder
    {
        private Func<IAccessControlContext, bool> _globalRetrieve = null;
        private Func<IAccessControlContext, bool> _globalInsert = null;
        private Func<IAccessControlContext, bool> _globalUpdate = null;
        private Func<IAccessControlContext, bool> _globalDelete = null;
        private readonly StringBuilder _buildLog = new StringBuilder();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuntimeEntityAccessRule

        public virtual void AuthorizeRetrieve(IAccessControlContext context)
        {
            ValidateOrThrow("Retrieve", CanRetrieve(context));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IQueryable AuthorizeQuery(IAccessControlContext context, IQueryable source)
        {
            return source;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeInsert(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Insert", CanInsert(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeUpdate(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Update", CanUpdate(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AuthorizeDelete(IAccessControlContext context, object entity)
        {
            ValidateOrThrow("Delete", CanDelete(context, entity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanRetrieve(IAccessControlContext context)
        {
            if ( _globalRetrieve != null )
            {
                return _globalRetrieve(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanInsert(IAccessControlContext context)
        {
            if ( _globalInsert != null )
            {
                return _globalInsert(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanUpdate(IAccessControlContext context)
        {
            if ( _globalUpdate != null )
            {
                return _globalUpdate(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanDelete(IAccessControlContext context)
        {
            if ( _globalDelete != null )
            {
                return _globalDelete(context);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanRetrieve(IAccessControlContext context, object entity)
        {
            return CanRetrieve(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanInsert(IAccessControlContext context, object entity)
        {
            return CanInsert(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanUpdate(IAccessControlContext context, object entity)
        {
            return CanUpdate(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool? CanDelete(IAccessControlContext context, object entity)
        {
            if ( !CanDelete(context).GetValueOrDefault(false) )
            {
                return false;
            }

            return null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of INonTypedEntityAccessControlBuilder

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsDenied()
        {
            _buildLog.AppendLine("IsDenied");

            _globalRetrieve = _s_globalFalse;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsReadOnly()
        {
            _buildLog.AppendLine("IsReadOnly");

            _globalRetrieve = _s_globalTrue;
            _globalInsert = _s_globalFalse;
            _globalUpdate = _s_globalFalse;
            _globalDelete = _s_globalFalse;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsDeniedIf(Func<IAccessControlContext, bool> condition)
        {
            _buildLog.AppendLine("IsDeniedIf(...)");

            CombineAndNot(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsReadOnlyIf(Func<IAccessControlContext, bool> condition)
        {
            _buildLog.AppendLine("IsReadOnlyIf(...)");

            CombineOr(ref _globalRetrieve, condition);
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);
            
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsDeniedUnless(Func<IAccessControlContext, bool> condition)
        {
            _buildLog.AppendLine("IsDeniedUnless(...)");

            CombineAnd(ref _globalRetrieve, condition);
            CombineAnd(ref _globalInsert, condition);
            CombineAnd(ref _globalUpdate, condition);
            CombineAnd(ref _globalDelete, condition);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsReadOnlyUnless(Func<IAccessControlContext, bool> condition)
        {
            _buildLog.AppendLine("IsDeniedUnless(...)");

            _globalRetrieve = _s_globalTrue;
            CombineAndNot(ref _globalInsert, condition);
            CombineAndNot(ref _globalUpdate, condition);
            CombineAndNot(ref _globalDelete, condition);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsDefinedHard(bool? canRetrieve, bool? canInsert, bool? canUpdate, bool? canDelete)
        {
            _buildLog.AppendFormat(
                "IsDefinedHard(C={0},R={1},U={2},D={3})",
                canInsert != null ? canInsert.ToString() : "null",
                canRetrieve != null ? canRetrieve.ToString() : "null",
                canUpdate != null ? canUpdate.ToString() : "null",
                canDelete != null ? canDelete.ToString() : "null");

            _buildLog.AppendFormat("IsDefinedHard(C={0},R={1},U={2},D={3})", canInsert, canRetrieve, canUpdate, canDelete);

            if ( canRetrieve.HasValue )
            {
                _globalRetrieve = (canRetrieve.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canInsert.HasValue )
            {
                _globalInsert = (canInsert.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canUpdate.HasValue )
            {
                _globalUpdate = (canUpdate.Value ? _s_globalTrue : _s_globalFalse);
            }

            if ( canDelete.HasValue )
            {
                _globalDelete = (canDelete.Value ? _s_globalTrue : _s_globalFalse);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        INonTypedEntityAccessControlBuilder INonTypedEntityAccessControlBuilder.IsDefinedByContext(
            Func<IAccessControlContext, bool> canRetrieve,
            Func<IAccessControlContext, bool> canInsert,
            Func<IAccessControlContext, bool> canUpdate,
            Func<IAccessControlContext, bool> canDelete)
        {
            _buildLog.AppendFormat(
                "IsDefinedByContext(C={0},R={1},U={2},D={3})", 
                canInsert != null ? "f(...)" : "null",
                canRetrieve != null ? "f(...)" : "null",
                canUpdate != null ? "f(...)" : "null",
                canDelete != null ? "f(...)" : "null");

            if ( canRetrieve != null )
            {
                CombineAnd(ref _globalRetrieve, canRetrieve);
            }

            if ( canInsert != null )
            {
                CombineAnd(ref _globalInsert, canInsert);
            }

            if ( canUpdate != null )
            {
                CombineAnd(ref _globalUpdate, canUpdate);
            }

            if ( canDelete != null )
            {
                CombineAnd(ref _globalDelete, canDelete);
            }

            return this;
        }

        #if false
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsFilteredByQuery(
            Func<IAccessControlContext, Expression<Func<TEntity, bool>>> canRetrieveWhere)
        {
            if ( _entityQuery == null )
            {
                _entityQuery = canRetrieveWhere;
            }
            else
            {
                throw new NotSupportedException("Multiple narrowing queries are not supported within one rule.");
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypedEntityAccessControlBuilder<TEntity> ITypedEntityAccessControlBuilder<TEntity>.IsDefinedByPredicate(
            Func<IAccessControlContext, TEntity, bool> canRetrieve, 
            Func<IAccessControlContext, TEntity, bool> canInsert, 
            Func<IAccessControlContext, TEntity, bool> canUpdate, 
            Func<IAccessControlContext, TEntity, bool> canDelete)
        {
            if ( canRetrieve != null )
            {
                CombineAnd(ref _entityRetrieve, canRetrieve);
            }

            if ( canInsert != null )
            {
                CombineAnd(ref _entityInsert, canInsert);
            }

            if ( canUpdate != null )
            {
                CombineAnd(ref _entityUpdate, canUpdate);
            }

            if ( canDelete != null )
            {
                CombineAnd(ref _entityDelete, canDelete);
            }

            return this;
        }

        #endif

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override string ToString()
        {
            return _buildLog.ToString();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ValidateOrThrow(string operation, bool? evaluatedAuthorizarion)
        {
            if ( !evaluatedAuthorizarion.GetValueOrDefault(false) )
            {
                throw new SecurityException(string.Format("User is not authorized to perform the requested '{0}' operation.", operation));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected StringBuilder BuildLog
        {
            get { return _buildLog; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool HasGlobalRetrieve
        {
            get { return _globalRetrieve != null; } 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected bool HasGlobalInsert
        {
            get { return _globalInsert != null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected bool HasGlobalUpdate
        {
            get { return _globalUpdate != null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected bool HasGlobalDelete
        {
            get { return _globalDelete != null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static readonly Func<IAccessControlContext, bool> _s_globalTrue = context => true;
        protected static readonly Func<IAccessControlContext, bool> _s_globalFalse = context => false;

        #if false
        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityTrue = (context, entity) => true;
        private static readonly Func<IAccessControlContext, TEntity, bool> _s_entityFalse = (context, entity) => false;
        #endif
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static void CombineOr(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) || newFunc(context);
                };
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false
        private static void CombineOr(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) || newFunc(context, entity);
                };
            }
        }
        #endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static void CombineAnd(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) && newFunc(context);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false
        private static void CombineAnd(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) && newFunc(context, entity);
                };
            }
        }
        #endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static void CombineAndNot(ref Func<IAccessControlContext, bool> current, Func<IAccessControlContext, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context) => {
                    return existingFunc(context) && !newFunc(context);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false
        private static void CombineAndNot(ref Func<IAccessControlContext, TEntity, bool> current, Func<IAccessControlContext, TEntity, bool> additional)
        {
            if ( current == null )
            {
                current = additional;
            }
            else
            {
                var existingFunc = current;
                var newFunc = additional;
                current = (context, entity) => {
                    return existingFunc(context, entity) && !newFunc(context, entity);
                };
            }
        }
        #endif
    }
}   
