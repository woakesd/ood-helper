using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;
using OodHelper.Rules;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for the boat-selection rule trees (the <c>select_rules</c> table).
    /// Owns the recursive load/save/delete that previously lived in <see cref="BoatSelectRule"/>,
    /// leaving that class a pure in-memory tree.
    /// </summary>
    internal sealed class SelectRuleRepository : ISelectRuleRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public SelectRuleRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<SelectRuleListItem> GetTopLevel(string filter)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                IQueryable<SelectRule> q = ctx.SelectRules.Where(r => r.Parent == null);
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string pattern = $"%{filter.Trim()}%";
                    q = q.Where(r => EF.Functions.Like(r.Name, pattern));
                }

                return q.OrderBy(r => r.Name)
                    .Select(r => new SelectRuleListItem(r.Id, r.Name))
                    .ToList();
            }
        }

        public BoatSelectRule? GetTree(Guid id)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var all = ctx.SelectRules.AsNoTracking().ToList();
                var root = all.FirstOrDefault(r => r.Id == id);
                return root == null ? null : BuildTree(root, all);
            }
        }

        public BoatSelectRule GetTree(string name)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var all = ctx.SelectRules.AsNoTracking().ToList();
                var root = all.FirstOrDefault(r => r.Name == name);
                //
                // An unknown class has no rule, so it never auto-matches a boat; SelectBoats then
                // falls back to the rule selector. Preserve that by returning an empty compound.
                //
                if (root == null)
                    return new BoatSelectRule { Name = name, Rule = RuleType.Compound, Application = null };
                return BuildTree(root, all);
            }
        }

        public void Save(BoatSelectRule root)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                SaveNode(ctx, root);
                ctx.SaveChanges();
                tx.Commit();
            }
        }

        public void Delete(Guid id)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var all = ctx.SelectRules.ToList();
                var toDelete = new List<SelectRule>();
                CollectSubtree(id, all, toDelete);
                ctx.SelectRules.RemoveRange(toDelete);
                ctx.SaveChanges();
            }
        }

        //
        // The root of a saved rule is always treated as a compound node, mirroring the old
        // BoatSelectRule(id)/BoatSelectRule(name) constructors.
        //
        private static BoatSelectRule BuildTree(SelectRule entity, List<SelectRule> all)
        {
            var node = new BoatSelectRule
            {
                Id = entity.Id,
                Name = entity.Name,
                Rule = RuleType.Compound,
                Application = entity.Application.HasValue ? (Apply?)entity.Application.Value : null
            };
            AddChildren(node, all);
            return node;
        }

        private static void AddChildren(BoatSelectRule parent, List<SelectRule> all)
        {
            foreach (var r in all.Where(x => x.Parent == parent.Id))
            {
                var child = new BoatSelectRule { Id = r.Id };
                if (r.Field != null)
                {
                    child.Field = BoatSelectRule.Fields.First(n => n.Name.Equals(r.Field));
                    child.Condition = (ConditionType)(r.Condition ?? 0);
                    child.Rule = RuleType.Simple;
                    child.StringValue = r.StringValue;
                    child.Bound1 = r.NumberBound1;
                    child.Bound2 = r.NumberBound2;
                }
                else
                {
                    child.Rule = RuleType.Compound;
                    child.Application = r.Application.HasValue ? (Apply)r.Application.Value : Apply.Any;
                    AddChildren(child, all);
                }

                parent.Add(child);
            }
        }

        private static void SaveNode(OodHelperContext ctx, BoatSelectRule node)
        {
            if (node.Id == null)
                node.Id = Guid.NewGuid();

            var entity = ctx.SelectRules.Find(node.Id.Value);
            if (entity == null)
            {
                entity = new SelectRule { Id = node.Id.Value };
                ctx.SelectRules.Add(entity);
            }

            entity.Name = node.Name;
            entity.Parent = node.Parent?.Id;
            entity.Application = (int?)node.Application;
            entity.Field = node.Field?.Name;
            entity.Condition = (int)node.Condition;
            entity.StringValue = node.StringValue;
            entity.NumberBound1 = node.Bound1;
            entity.NumberBound2 = node.Bound2;

            foreach (var child in node.Children)
                SaveNode(ctx, child);

            foreach (var del in node.DeletedChildren)
                DeleteNode(ctx, del);
        }

        private static void DeleteNode(OodHelperContext ctx, BoatSelectRule node)
        {
            if (node.Id.HasValue)
            {
                var entity = ctx.SelectRules.Find(node.Id.Value);
                if (entity != null)
                    ctx.SelectRules.Remove(entity);
            }

            foreach (var child in node.Children)
                DeleteNode(ctx, child);

            foreach (var del in node.DeletedChildren)
                DeleteNode(ctx, del);
        }

        private static void CollectSubtree(Guid id, List<SelectRule> all, List<SelectRule> acc)
        {
            var node = all.FirstOrDefault(r => r.Id == id);
            if (node == null) return;

            acc.Add(node);
            foreach (var child in all.Where(r => r.Parent == id))
                CollectSubtree(child.Id, all, acc);
        }
    }
}
