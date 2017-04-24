using System;
using System.Collections.Generic;
using Nitra.Declarations;
using Type = Nitra.Declarations.Type;

namespace Ammy.VisualStudio.Service
{
    class AstCollectorVisitor : IAstVisitor
    {
        public IReadOnlyList<IAst> CollectedItems => _collectedItems;

        private readonly Func<IAst, bool> _predicate;
        private readonly List<IAst> _collectedItems = new List<IAst>();

        public AstCollectorVisitor(Func<IAst, bool> predicate)
        {
            _predicate = predicate;
        }

        public void Visit(IAst ast)
        {
            TryAddItem(ast);

            ast.Accept(this);
        }

        public void Visit(Reference reference)
        {
            TryAddItem(reference);
        }

        public void Visit(Name name)
        {
            TryAddItem(name);
        }

        public void Visit(IRef r)
        {}

        private void TryAddItem(IAst ast)
        {
            if (_predicate(ast))
                _collectedItems.Add(ast);
        }
    }
}