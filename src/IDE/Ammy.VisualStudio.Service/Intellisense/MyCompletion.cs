using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DotNet;
using Microsoft.VisualStudio.Language.Intellisense;
using Nitra.Declarations;
using Ammy.Language;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class MyCompletion : Completion
    {
        private readonly IAst[] _stack;
        public CompletionType CompletionType { get; private set; }
        public DeclarationSymbol Symbol { get; set; }
        public int OrderIndex { get; set; }

        public static readonly ImageSource ClassImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Class.png"));
        public static readonly ImageSource PropertyImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Property.png"));
        public static readonly ImageSource ColorWheelImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/ColorWheel.png"));
        public static readonly ImageSource EventImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Event.png"));
        public static readonly ImageSource FieldImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Field.png"));
        public static readonly ImageSource EnumItemImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/EnumItem.png"));
        public static readonly ImageSource MethodImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Method.png"));
        public static readonly ImageSource ComponentImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Component.png"));
        public static readonly ImageSource ReferenceImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/Reference.png"));
        
        public MyCompletion(string displayText, string insertionText, string description, ImageSource iconSource, string iconAutomationText, CompletionType completionType, DeclarationSymbol symbol, IAst[] stack) : base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
            _stack = stack;
            CompletionType = completionType;
            Symbol = symbol;
        }

        public bool IsPropertyValue => GetFirstNonReferenceAst() is PropertyValue;

        private IAst GetFirstNonReferenceAst()
        {
            return _stack.FirstOrDefault(ast => !(ast is Reference) && !(ast is QualifiedReference));
        }

        public static MyCompletion FromSymbol(DeclarationSymbol symbol, IAst[] stack)
        {
            var completionType = CompletionType.Normal;
            var imageSource = ClassImage;
            var name = symbol.Name;

            if (name.Contains('.'))
                name = name.Split('.').Last();

            if (symbol is TypeSymbol) {
                completionType = CompletionType.Node;
                imageSource = ClassImage;

            } else if (symbol is GlobalDeclaration.ContentFunctionSymbol) {
                var fun = (GlobalDeclaration.ContentFunctionSymbol)symbol;
                completionType = fun.Parameters.MakeCompletionList("").Any()
                    ? CompletionType.ContentFunctionRefWithParams
                    : CompletionType.ContentFunctionRef;
                imageSource = ComponentImage;

            } else if (symbol is GlobalDeclaration.TypeFunctionSymbol) {
                var fun = (GlobalDeclaration.TypeFunctionSymbol)symbol;
                completionType = fun.Parameters.MakeCompletionList("").Any()
                    ? CompletionType.TypeFunctionRefWithParams
                    : CompletionType.TypeFunctionRef;
                imageSource = ComponentImage;

            } else if (symbol is Member.PropertySymbol) {
                var fullName = symbol.FullName;
                if (fullName.StartsWith("System.Windows.Media.Brushes") || fullName.StartsWith("Windows.UI.Colors"))
                    imageSource = ColorWheelImage;
                else
                    imageSource = PropertyImage;

                completionType = CompletionType.Property;

            } else if (symbol is VariableRefSymbol) {
                name = "$" + name;
                imageSource = ReferenceImage;
            } else if (symbol is Member.MethodSymbol) {
                imageSource = MethodImage;
            } else if (symbol is Member.EventSymbol) {
                imageSource = EventImage;
            } else if (symbol is Member.FieldSymbol) {
                imageSource = FieldImage;
            } else if (symbol is EnumMemberSymbol) {
                imageSource = EnumItemImage;
            }

            if (symbol is GlobalDeclaration.TypeFunctionSymbol)
                name = "@" + name;
            else if (symbol is GlobalDeclaration.ContentFunctionSymbol)
                name = "#" + name;

            return new MyCompletion(name, name, symbol.FullName, imageSource, "", completionType, symbol, stack);
        }
    }
}