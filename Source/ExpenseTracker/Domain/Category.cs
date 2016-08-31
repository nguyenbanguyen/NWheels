using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NWheels.Api;
using NWheels.Api.Ddd;
using NWheels.Api.Ddd.Exceptions;

namespace ExpenseTracker.Domain
{
    [DomainModel.Entity(IsTreeStructure = true)]
    public class Category : IEnumerable<Category>
    {
        private readonly IBudgetManagementLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Category(IBudgetManagementLogger logger)
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<Category> IEnumerable<Category>.GetEnumerator()
        {
            return this.SubCategories.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SubCategories.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Move(
            [Guard.NotNull] Category newParent,
            Category insertBefore = null)
        {
            if (newParent.Id != ParentCategory.Id && this.Contains(newParent))
            {
                throw _logger.CannotMoveCategory(BudgetManagementError.DestinationIsSubCategoryOfSource, this, newParent);
            }

            if (insertBefore != null && insertBefore.ParentCategory.Id != newParent.Id)
            {
                throw _logger.CannotMoveCategory(BudgetManagementError.DestinationSiblingCategoryMismatch, this, newParent);
            }

            ParentCategory.DetachSubCategory(this);
            newParent.AttachSubCategory(this, insertBefore);
            ParentCategory = newParent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool Contains(Category subCategory)
        {
            foreach (var sub in SubCategories)
            {
                if (sub.Id == subCategory.Id)
                {
                    return true;
                }
            }

            foreach (var sub in SubCategories)
            {
                if (sub.Contains(subCategory))
                {
                    return true;
                }
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.EntityId(AutoGenerated = true)]
        public virtual Guid Id { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue]
        public virtual string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue, DomainModel.Relation.CompositionParent]
        public virtual Category ParentCategory { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue]
        public virtual bool IsProrated { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue]
        public virtual decimal? DefaultBudget { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue]
        public virtual decimal? Budget { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue]
        public virtual decimal Expense { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.ThisEntityReference]
        public virtual CategoryReference AsReference { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DomainModel.PersistedValue, DomainModel.Relation.Composition]
        protected virtual IList<Category> SubCategories { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void AttachSubCategory(Category sub, Category insertBefore = null)
        {
            int insertionIndex;

            if (insertBefore != null)
            {
                insertionIndex = this.SubCategories.TakeWhile(c => c.Id != insertBefore.Id).Count();
            }
            else
            {
                insertionIndex = this.SubCategories.Count;
            }
            
            this.SubCategories.Insert(insertionIndex, sub);

            for (var parent = this; parent != null; parent = parent.ParentCategory)
            {
                parent.Budget += sub.Budget;
                parent.Expense += sub.Expense;
                parent.DefaultBudget += sub.DefaultBudget;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void DetachSubCategory(Category sub)
        {
            var subCategoryItemToRemove = this.SubCategories.FirstOrDefault(c => c.Id == sub.Id);

            if (subCategoryItemToRemove == null)
            {
                throw _logger.CannotMoveCategory(BudgetManagementError.CouldNotFindCategoryToDetach, sub, null);
            }

            if (this.SubCategories.Remove(subCategoryItemToRemove))
            {
                for (var parent = this; parent != null; parent = parent.ParentCategory)
                {
                    parent.Budget -= sub.Budget;
                    parent.Expense -= sub.Expense;
                    parent.DefaultBudget -= sub.DefaultBudget;
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class CategoryReference : EntityReference<Category, Guid> { }
}