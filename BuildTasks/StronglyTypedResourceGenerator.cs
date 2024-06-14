
extern alias DRESEXT;

using System;
using System.IO;
using System.Text;
using System.CodeDom;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using DRESEXT::DotNetResourcesExtensions;

namespace DotNetResourcesExtensions.BuildTasks
{
    /// <summary>
    /// Generates strongly-typed resources using CodeDOM. <br />
    /// You can then call in any CodeDOM-defined language 
    /// the <see cref="Generate(ICodeGenerator, TextWriter, CodeGeneratorOptions)"/> method 
    /// and generate the class itself. <br />
    /// The <see cref="StronglyTypedCodeProviderBuilder"/> class provides default language generators to use for generating 
    /// such a class.
    /// </summary>
    public class StronglyTypedResourceGenerator
    {
        private System.String manifestresname , classname;
        private IResourceLoader loader;
        private ResourceClassVisibilty visibilty;

        public StronglyTypedResourceGenerator(
            IResourceLoader ldr , System.String MRN , 
            System.String CN , ResourceClassVisibilty visible) 
        {
            loader = ldr;
            manifestresname = MRN;
            classname = CN;
            visibilty = visible;
        }

        public void Generate(ICodeGenerator g , TextWriter output  , CodeGeneratorOptions opt)
        {
            CodeCompileUnit ccu = new();
            CodeNamespace classnamespace = new();
            if (classname.IndexOf('.') > -1)
            {
                classnamespace.Name = classname.Remove(classname.LastIndexOf('.'));
            }
            CodeNamespaceImport cni = new("DotNetResourcesExtensions");
            classnamespace.Imports.Add(cni);
            classnamespace.Comments.Add(new CodeCommentStatement("This is an Automatically generated file: It means that it's contents might be replaced if the StronglyTypedResourceGenerator is run on this file."));
            classnamespace.Comments.Add(new CodeCommentStatement("© MDCDI1315. DotNetResourcesExtensions is a project under the MIT Liscense."));
            classnamespace.Comments.Add(new CodeCommentStatement("About: This file is a strongly-typed resource loader convenient for getting your resources through code."));
            classnamespace.Comments.Add(new CodeCommentStatement("Unlike the usual StronglyTypedResourceBuilder defined in System.Resources.Tools , it does not use the System.Resources.ResourceManager"));
            classnamespace.Comments.Add(new CodeCommentStatement("class  , so if you have created your resources using the default .NET PreserializedResourceWriter , you will not have issues regarding assembly"));
            classnamespace.Comments.Add(new CodeCommentStatement("binding because that assembly is loaded by DotNetResourcesExtensions at run-time."));
            classnamespace.Comments.Add(new CodeCommentStatement("Additionally , this class implementation defines a Dispose method to dispose the loader if you do not need it after some time."));
            CodeTypeDeclaration typedecl = new();
            CreateBaseClassData(typedecl);
            classnamespace.Types.Add(typedecl);
            ccu.Namespaces.Add(classnamespace);
            ccu.ReferencedAssemblies.Add("DotNetResourcesExtensions.dll");
            g.GenerateCodeFromCompileUnit(ccu , output , opt);
        }

        private System.String GetClassName()
        {
            System.Int32 idx;
            if ((idx = classname.LastIndexOf('.')) != -1) 
            {
                return classname.Substring(idx + 1);
            } else
            {
                return classname;
            }
        }

        private void CreateBaseClassData(CodeTypeDeclaration ctd)
        {
            ctd.IsPartial = false;
            ctd.IsClass = true;
            ctd.IsStruct = false;
            ctd.Name = GetClassName();
            ctd.Attributes = MemberAttributes.Static;
            if (visibilty == ResourceClassVisibilty.Public)
            {
                ctd.Attributes |= MemberAttributes.Public;
            } else {
                ctd.Attributes |= MemberAttributes.Assembly;
            }
            ctd.Members.Add(CreateInternalLoaderField());
            ctd.Members.Add(CreateUnderlyingStreamField());
            ctd.Members.Add(CreateResourceLoaderProperty());
            ctd.Members.Add(CreateDisposeMethod());
            CreateResourcesMembers(ctd);
            CodeAttributeDeclaration generatedcodeattribute = new(new CodeTypeReference(typeof(GeneratedCodeAttribute)));
            generatedcodeattribute.Arguments.Add(new CodeAttributeArgument(new CodeSnippetExpression("\"StronglyTypedResourceGenerator-dotnetresourcesextensions-v1\"")));
            generatedcodeattribute.Arguments.Add(new CodeAttributeArgument(new CodeSnippetExpression("\"1.0.0.1\"")));
            CodeAttributeDeclaration compilergeneratedcode = new(new CodeTypeReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)));
            ctd.CustomAttributes.Add(generatedcodeattribute);
            ctd.CustomAttributes.Add(compilergeneratedcode);
        }

        private CodeCommentStatement[] GenerateDocCommentFromLines(params System.String[] lines)
        {
            List<CodeCommentStatement> result = new();
            CodeComment cmt = new();
            cmt.DocComment = true;
            cmt.Text = "<summary>";
            result.Add(new(cmt));
            for (System.Byte I = 0; I < lines.Length; I++)
            {
                cmt = new();
                cmt.DocComment = true;
                cmt.Text = lines[I];
                result.Add(new(cmt));
            }
            cmt = new();
            cmt.DocComment = true;
            cmt.Text = "</summary>";
            result.Add(new(cmt));
            return result.ToArray();
        }

        private void CreateResourcesMembers(CodeTypeDeclaration ctd) 
        {
            foreach (var resource in loader)
            {
                CodeMemberProperty Resprop = new();
                Resprop.Name = ToValidResourceName(resource.Name);
                Resprop.Type = new CodeTypeReference(resource.TypeOfValue); // Will have the resource defined type by default
                Resprop.Attributes = MemberAttributes.Static | MemberAttributes.Public; // Public and static
                Resprop.Comments.AddRange(GenerateDocCommentFromLines($"Gets the resource with name {resource.Name} ..."));
                CodePropertyReferenceExpression cfre = new();
                cfre.PropertyName = "ResourceLoader";
                CodeMethodInvokeExpression invokegetresource;
                if (resource.TypeOfValue == typeof(System.String)) {
                    var cmre = new CodeMethodReferenceExpression(cfre, "GetStringResource");
                    invokegetresource = new CodeMethodInvokeExpression(cmre , new CodeSnippetExpression($"\"{resource.Name}\""));
                } else if (resource.TypeOfValue == typeof(System.Byte[])) {
                    var cmre = new CodeMethodReferenceExpression(cfre, "GetByteArrayResource");
                    invokegetresource = new CodeMethodInvokeExpression(cmre, new CodeSnippetExpression($"\"{resource.Name}\""));
                } else {
                    var cmre = new CodeMethodReferenceExpression(cfre, "GetResource");
                    cmre.TypeArguments.Add(new CodeTypeReference(resource.TypeOfValue));
                    invokegetresource = new CodeMethodInvokeExpression(cmre, new CodeSnippetExpression($"\"{resource.Name}\""));
                }
                CodeMethodReturnStatement returnstmt = new(invokegetresource);
                Resprop.GetStatements.Add(returnstmt);
                ctd.Members.Add(Resprop);
            }
        }

        private CodeMemberProperty CreateResourceLoaderProperty()
        {
            CodeMemberProperty ctm = new();
            ctm.Comments.AddRange(GenerateDocCommentFromLines("Gets an <see cref=\"IResourceLoader\"/> instance to the resources that the assembly contains.",
                "Gets the assembly that this type belongs to and by using GetManifestResourceStream gets a stream to the data itself."));
            ctm.Name = "ResourceLoader";
            ctm.HasGet = true;
            ctm.HasSet = false;
            ctm.Type = new CodeTypeReference(typeof(IResourceLoader));
            ctm.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            CodeFieldReferenceExpression cfre = new();
            cfre.FieldName = "resloader";
            CodeFieldReferenceExpression cfre2 = new();
            cfre2.FieldName = "stream";
            CodeBinaryOperatorExpression cboe = new();
            cboe.Left = cfre;
            cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
            cboe.Right = new CodeDefaultValueExpression(new CodeTypeReference(typeof(IResourceLoader)));
            // Equals to calling resloader == null , or even resloader == default.
            CodeConditionStatement ccs = new(cboe); // Start of if statement
            CodeTryCatchFinallyStatement ctcfs = new(); 
            CodeTypeOfExpression current = new(GetClassName());
            CodePropertyReferenceExpression reference = new(current, "Assembly");
            CodeMethodReferenceExpression getmrs = new(reference, "GetManifestResourceStream");
            CodeMethodInvokeExpression invokemrs = new(getmrs, new CodeSnippetExpression($"\"{manifestresname}\""));
            CodeAssignStatement state = new(cfre2, invokemrs);
            System.Type underlying;
            if (loader is JSONResourcesLoader) {
                underlying = typeof(JSONResourcesLoader);
            } else if (loader is CustomDataResourcesLoader) {
                underlying = typeof(CustomDataResourcesLoader);
            } else if (loader is DotNetResourceLoader) {
                underlying = typeof(DotNetResourceLoader);
            } else {
                throw new InvalidDataException($"Cannot create the information required using the {loader.GetType().FullName} class.");
            }
            CodeAssignStatement state2 = new(cfre, new CodeObjectCreateExpression(new CodeTypeReference(underlying), cfre2));
            ctcfs.TryStatements.Add(state);
            ctcfs.TryStatements.Add(state2);
            cboe = new();
            cboe.Left = cfre;
            cboe.Right = new CodeDefaultValueExpression(new CodeTypeReference(typeof(IResourceLoader)));
            cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
            CodeConditionStatement resloadernotnull = new(cboe);
            CodeMethodInvokeExpression dispose1 = new(cfre, "Dispose");
            CodeAssignStatement cassign1 = new(cfre, new CodeDefaultValueExpression(new CodeTypeReference(typeof(IResourceLoader))));
            resloadernotnull.TrueStatements.Add(dispose1);
            resloadernotnull.TrueStatements.Add(cassign1);
            CodeBinaryOperatorExpression condstreamnonnull = new();
            condstreamnonnull.Left = cfre2;
            condstreamnonnull.Operator = CodeBinaryOperatorType.IdentityInequality;
            condstreamnonnull.Right = new CodeDefaultValueExpression(new CodeTypeReference(typeof(System.IO.Stream)));
            CodeConditionStatement streamnotnull = new(condstreamnonnull);
            CodeMethodInvokeExpression dispose2 = new(cfre2, "Dispose");
            CodeAssignStatement cassign2 = new(cfre2, new CodeDefaultValueExpression(new CodeTypeReference(typeof(Stream))));
            streamnotnull.TrueStatements.Add(dispose2);
            streamnotnull.TrueStatements.Add(cassign2);
            CodeCatchClause ccc = new();
            ccc.Statements.Add(resloadernotnull);
            ccc.Statements.Add(streamnotnull);
            ccc.CatchExceptionType = null;
            ctcfs.CatchClauses.Add(ccc);
            ccs.TrueStatements.Add(ctcfs); // When true add our try-catch clause
            CodeMethodReturnStatement retmethod = new(cfre);
            ctm.GetStatements.Add(ccs);
            ctm.GetStatements.Add(retmethod);
            return ctm;
        }

        private CodeMemberMethod CreateDisposeMethod()
        {
            CodeMemberMethod cmm = new();
            cmm.Name = "Dispose";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.Comments.AddRange(GenerateDocCommentFromLines(
                "Disposes the resources used by this class. You must ONLY AND ONLY CALL THIS" ,
                "when you are DONE using THESE RESOURCES. <br />" , 
                "Note: You may also temporarily dispose resources then reload them again by calling the ResourceLoader property <br />" ,
                "or any of the generated resource properties."));
            CodeBinaryOperatorExpression cboe = new();
            CodeFieldReferenceExpression cfre = new();
            cfre.FieldName = "resloader";
            CodeFieldReferenceExpression cfre2 = new();
            cfre2.FieldName = "stream";
            cboe.Left = cfre;
            cboe.Right = new CodeDefaultValueExpression(new CodeTypeReference(typeof(IResourceLoader)));
            cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
            CodeConditionStatement resloadernotnull = new(cboe);
            CodeMethodInvokeExpression dispose1 = new(cfre, "Dispose");
            CodeAssignStatement cassign1 = new(cfre , new CodeDefaultValueExpression(new CodeTypeReference(typeof(IResourceLoader))));
            resloadernotnull.TrueStatements.Add(dispose1);
            resloadernotnull.TrueStatements.Add(cassign1);
            CodeBinaryOperatorExpression condstreamnonnull = new();
            condstreamnonnull.Left = cfre2;
            condstreamnonnull.Operator = CodeBinaryOperatorType.IdentityInequality;
            condstreamnonnull.Right = new CodeDefaultValueExpression(new CodeTypeReference(typeof(System.IO.Stream)));
            CodeConditionStatement streamnotnull = new(condstreamnonnull);
            CodeMethodInvokeExpression dispose2 = new(cfre2, "Dispose");
            CodeAssignStatement cassign2 = new(cfre2, new CodeDefaultValueExpression(new CodeTypeReference(typeof(Stream))));
            streamnotnull.TrueStatements.Add(dispose2);
            streamnotnull.TrueStatements.Add(cassign2);
            cmm.Statements.Add(resloadernotnull);
            cmm.Statements.Add(streamnotnull);
            return cmm;
        }

        private CodeMemberField CreateInternalLoaderField()
        {
            CodeMemberField cmf = new();
            cmf.Name = "resloader";
            cmf.Type = new CodeTypeReference(typeof(IResourceLoader));
            cmf.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            return cmf;
        }

        private CodeMemberField CreateUnderlyingStreamField()
        {
            CodeMemberField cmf = new();
            cmf.Name = "stream";
            cmf.Type = new CodeTypeReference(typeof(System.IO.Stream));
            cmf.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            return cmf;
        }
    
        public System.String ToValidResourceName(System.String name)
        {
            System.String result = System.String.Empty;
            if (name == null || name.Length == 0) { return result; }
            if (name[0] <= '9' && name[0] >= '0') {
                result = $"Resource_{name[0]}";
            } else {
                result += name[0];
            }
            for (System.Int32 I = 1; I < name.Length; I++)
            {
                switch (name[I])
                {
                    case '\n':
                        result += 'n';
                        break;
                    case '\r':
                        result += 'r';
                        break;
                    case '.':
                    case '@':
                    case '/':
                    case ':':
                    case '\\':
                        result += "_";
                        break;
                    default:
                        result += name[I];
                        break;
                }
            }
            return result;
        }
    }

    public enum ResourceClassVisibilty : System.Byte
    {
        Public,
        Internal
    }

    /// <summary>
    /// Default provider for generating the code given by the <see cref="StronglyTypedResourceGenerator"/> class.
    /// </summary>
    public static class StronglyTypedCodeProviderBuilder
    {
        public static void WithCSharp(IResourceLoader loader , System.String ManifestResourceName , System.String ClassName , System.String outfile , ResourceClassVisibilty visible)
        {
            if (String.IsNullOrEmpty(ClassName)) {
                ClassName = ManifestResourceName;
            }
            System.IO.FileStream fs = null;
            System.IO.TextWriter writer = null;
            try {
                fs = new(outfile , System.IO.FileMode.Create);
                writer = new System.IO.StreamWriter(fs);
                CSharpCodeProvider prov = new();
                StronglyTypedResourceGenerator build = new(loader, ManifestResourceName , ClassName , visible);
                build.Generate(prov.CreateGenerator(writer), writer, new() { BlankLinesBetweenMembers = true, IndentString = "  " , VerbatimOrder = true });
            } catch (Exception e) { 
                throw new AggregateException("Could not generate the result. Code generation failed.", e);
            } finally {
                writer?.Close();
                writer?.Dispose();
                fs?.Close();
                fs?.Dispose();
            }
        }

        public static void WithVisualBasic(IResourceLoader loader, System.String ManifestResourceName,System.String ClassName, System.String outfile, ResourceClassVisibilty visible)
        {
            if (String.IsNullOrEmpty(ClassName)) {
                ClassName = ManifestResourceName;
            }
            System.IO.FileStream fs = null;
            System.IO.TextWriter writer = null;
            try {
                fs = new(outfile, System.IO.FileMode.Create);
                writer = new System.IO.StreamWriter(fs);
                VBCodeProvider prov = new();
                StronglyTypedResourceGenerator build = new(loader, ManifestResourceName, ClassName, visible);
                build.Generate(prov.CreateGenerator(writer), writer, new() { BlankLinesBetweenMembers = true, IndentString = "  ", VerbatimOrder = true });
            } catch (Exception e) {
                throw new AggregateException("Could not generate the result. Code generation failed.", e);
            } finally {
                writer?.Close();
                writer?.Dispose();
                fs?.Close();
                fs?.Dispose();
            }
        }
    }
}
