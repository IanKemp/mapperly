using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Builder for member mappings (member of objects).
/// </summary>
internal static class MemberMappingBuilder
{
    public enum CodeStyle
    {
        Expression,
        Statement
    }

    public static bool TryBuildContainerAssignment(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberMappingInfo memberInfo,
        out bool requiresNullHandling,
        [NotNullWhen(true)] out MemberAssignmentMapping? mapping
    )
    {
        if (!TryBuild(ctx, memberInfo, CodeStyle.Statement, out var mappedSourceValue))
        {
            mapping = null;
            requiresNullHandling = false;
            return false;
        }

        var setterTargetPath = SetterMemberPath.Build(ctx.BuilderContext, memberInfo.TargetMember);
        requiresNullHandling = mappedSourceValue is MappedMemberSourceValue { RequiresSourceNullCheck: true };
        mapping = new MemberAssignmentMapping(setterTargetPath, mappedSourceValue, memberInfo);
        return true;
    }

    public static bool TryBuildAssignment(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberInfo,
        [NotNullWhen(true)] out MemberAssignmentMapping? mapping
    )
    {
        if (!TryBuild(ctx, memberInfo, CodeStyle.Expression, out var mappedSourceValue))
        {
            mapping = null;
            return false;
        }

        var setterTargetPath = SetterMemberPath.Build(ctx.BuilderContext, memberInfo.TargetMember);
        mapping = new MemberAssignmentMapping(setterTargetPath, mappedSourceValue, memberInfo);
        return true;
    }

    public static bool TryBuild(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        CodeStyle codeStyle,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    )
    {
        var mappingKey = memberMappingInfo.ToTypeMappingKey();
        var delegateMapping = ctx.BuilderContext.FindOrBuildLooseNullableMapping(
            mappingKey,
            diagnosticLocation: memberMappingInfo.Configuration?.Location
        );

        var sourceMember = memberMappingInfo.SourceMember;
        var targetMember = memberMappingInfo.TargetMember;
        if (delegateMapping == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapMember,
                sourceMember.ToDisplayString(),
                targetMember.ToDisplayString()
            );
            sourceValue = null;
            return false;
        }

        if (codeStyle == CodeStyle.Statement)
        {
            sourceValue = BuildBlockNullHandlingMapping(ctx, delegateMapping, sourceMember, targetMember);
            return true;
        }

        if (!ValidateLoopMapping(ctx, delegateMapping, sourceMember, targetMember))
        {
            sourceValue = null;
            return false;
        }

        sourceValue = BuildInlineNullHandlingMapping(ctx, delegateMapping, sourceMember, targetMember.MemberType);
        return true;
    }

    private static bool ValidateLoopMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourceMember,
        NonEmptyMemberPath targetMember
    )
    {
        if (!ReferenceEqualityComparer.Instance.Equals(delegateMapping, ctx.Mapping))
            return true;

        if (targetMember.Member is ConstructorParameterMember)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceLoopInCtorMapping,
                sourceMember.ToDisplayString(includeMemberType: false),
                ctx.Mapping.TargetType,
                targetMember.ToDisplayString(includeRootType: false, includeMemberType: false)
            );
        }
        else
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceLoopInInitOnlyMapping,
                sourceMember.ToDisplayString(includeMemberType: false),
                targetMember.ToDisplayString(includeMemberType: false)
            );
        }
        return false;
    }

    private static NullMappedMemberSourceValue BuildInlineNullHandlingMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourcePath,
        ITypeSymbol targetMemberType
    )
    {
        var getterSourcePath = GetterMemberPath.Build(ctx.BuilderContext, sourcePath);

        var nullFallback = NullFallbackValue.Default;
        if (!delegateMapping.SourceType.IsNullable() && sourcePath.IsAnyNullable())
        {
            nullFallback = ctx.BuilderContext.GetNullFallbackValue(targetMemberType);
        }

        return new NullMappedMemberSourceValue(
            delegateMapping,
            getterSourcePath,
            targetMemberType,
            nullFallback,
            !ctx.BuilderContext.IsExpression
        );
    }

    private static ISourceValue BuildBlockNullHandlingMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourceMember,
        NonEmptyMemberPath targetMember
    )
    {
        var getterSourcePath = GetterMemberPath.Build(ctx.BuilderContext, sourceMember);

        // no member of the source path is nullable, no null handling needed
        if (!sourceMember.IsAnyNullable())
        {
            return new MappedMemberSourceValue(delegateMapping, getterSourcePath, false, true);
        }

        // If null property assignments are allowed,
        // and the delegate mapping accepts nullable types (and converts it to a non-nullable type),
        // or the mapping is synthetic and the target accepts nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (
            ctx.BuilderContext.Configuration.Mapper.AllowNullPropertyAssignment
            && (delegateMapping.SourceType.IsNullable() || delegateMapping.IsSynthetic && targetMember.Member.IsNullable)
        )
        {
            return new MappedMemberSourceValue(delegateMapping, getterSourcePath, true, false);
        }

        // additional null condition check
        // (only map if the source is not null, else may throw depending on settings)
        // via RequiresNullCheck
        return new MappedMemberSourceValue(delegateMapping, getterSourcePath, false, true);
    }
}
