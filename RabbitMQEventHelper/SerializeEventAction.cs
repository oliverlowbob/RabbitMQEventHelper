using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using Newtonsoft.Json;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;

// Define your action
[Action("SerializeEventAction", "Serialize Event to JSON", Id = 123456)]
public class SerializeEventAction : IExecutableAction
{
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        var solution = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION);
        if (solution == null)
        {
            return false;
        }

        var psiSourceFile = context.GetData(JetBrains.ReSharper.Psi.DataContext.PsiDataConstants.SOURCE_FILE);
        if (psiSourceFile == null || !psiSourceFile.IsValid())
        {
            return false;
        }

        return psiSourceFile.PrimaryPsiLanguage.Is<CSharpLanguage>();
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
        var psiFile = context.GetData(JetBrains.ReSharper.Psi.DataContext.PsiDataConstants.SOURCE_FILE) as ICSharpFile;

        if (psiFile == null)
        {
            return;
        }

        foreach (var namespaceDeclaration in psiFile.NamespaceDeclarations)
        {
            foreach (var typeDeclaration in namespaceDeclaration.TypeDeclarations)
            {
                if (typeDeclaration is IClassLikeDeclaration classLikeDeclaration)
                {
                    var jsonString = SerializeToJson(classLikeDeclaration);
                    Console.WriteLine(jsonString); // For demonstration, show JSON in console
                }
            }
        }
    }

    private string SerializeToJson(IClassLikeDeclaration classDeclaration)
    {
        dynamic message = new System.Dynamic.ExpandoObject();
        foreach (var member in classDeclaration.PropertyDeclarations)
        {
            ((IDictionary<string, object>)message)[member.DeclaredElement.ShortName] = "<insert-value>";
        }

        var envelope = new
        {
            messageType = new[] { $"urn:message:{classDeclaration.GetContainingNamespaceDeclaration()?.QualifiedName}:{classDeclaration.DeclaredName}" },
            message = message
        };

        return JsonConvert.SerializeObject(envelope, Formatting.Indented);
    }
}